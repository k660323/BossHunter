using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PlayerInfo
{
    public long pIndex;
    public int level;
    public string creatureName;
    public string nickName;
    public bool isMaster;
    public GameObject playerObject;
}

public class Party : NetworkBehaviour
{
    // �÷��̾� ���� �ε���
    static long GenerationPartyIndex = 0;

    public long GetGenerationPartyIndex { get { return GenerationPartyIndex++; } }

    [SyncVar]
    long partyIndex = -1;

    public long GetPartyIndex { get { return partyIndex; } }

    // �÷��̾�
    Player myPlayer;

    // ����� ��Ƽ UI
    [SerializeField]
    private UI_Party uI_Party;

    // ��Ƽ ���� ����
    public bool IsParty => partyDic.Count > 0;

    // ���� ����
    [SyncVar]
    public bool IsMaster = false;

    // ��Ƽ ���
    SyncDictionary<long, PlayerInfo> partyDic = new SyncDictionary<long, PlayerInfo>();

    [ServerCallback]
    private void Awake()
    {
        partyIndex = GetGenerationPartyIndex;
        TryGetComponent(out myPlayer);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if (Managers.UI.SceneUI is IPlayerUI playerUI)
        {
            uI_Party = playerUI.GetPlayerUI.UI_Party;
            uI_Party.party = this;

            partyDic.Callback -= OnPartPlayerUpdated;
            partyDic.Callback += OnPartPlayerUpdated;

            foreach (var item in partyDic)
            {
                OnPartPlayerUpdated(SyncDictionary<long, PlayerInfo>.Operation.OP_ADD, item.Key, item.Value);
            }
        }
    }

    // UI ������Ʈ
    public void OnPartPlayerUpdated(SyncDictionary<long, PlayerInfo>.Operation op, long key, PlayerInfo playerInfo)
    {
        switch (op)
        {
            case SyncDictionary<long, PlayerInfo>.Operation.OP_ADD:
                uI_Party.AddPlayerInfoSlot(key, playerInfo);
                break;
            case SyncDictionary<long, PlayerInfo>.Operation.OP_SET:
                uI_Party.SetPlayerInfoSlot(key, playerInfo);
                break;
            case SyncDictionary<long, PlayerInfo>.Operation.OP_REMOVE:
                uI_Party.RemovePlayerInfoSlot(key);
                break;
            case SyncDictionary<long, PlayerInfo>.Operation.OP_CLEAR:
                uI_Party.ClearPlayerInfoSlot();
                break;
        }
    }

    // ��Ƽ �����
    [Command]
    public void CTS_CreateParty()
    {
        // �̹� ��Ƽ ����
        if (IsParty)
            return;

        // ����
        IsMaster = true;
        partyDic.Add(partyIndex, new PlayerInfo() { pIndex = partyIndex, level = myPlayer.GetStat.Level, creatureName = myPlayer.GetStat.CreatureName, nickName = myPlayer.GetClientInfo.NickName, isMaster = true, playerObject = gameObject });
    }

    // ��Ƽ ����
    [Command(requiresAuthority = false)]
    public void CTS_RequsetJoinParty(PlayerInfo requestPlayerInfo, PlayerInfo JoinPlayerInfo)
    {
        if (requestPlayerInfo.playerObject == null)
            return;

        if (requestPlayerInfo.playerObject.TryGetComponent(out Player masterPlayer) == false)
            return;

        masterPlayer.GetParty.JoinParty(JoinPlayerInfo);
        
    }

    // ���� �������� �÷��̾ �߰��ϰ� �˷��ش�.
    public void JoinParty(PlayerInfo joinPlayerInfo)
    {
        // �̹� ������ �÷��̾�� �߰��� �÷��̾ �˸���.
        foreach (PlayerInfo pInfo in partyDic.Values)
        {
            // ���� ��ŵ
            if (pInfo.pIndex == partyIndex)
            {
                continue;
            }
            else if (pInfo.playerObject != null && pInfo.playerObject.TryGetComponent(out Player otherPlayer))
            {
                otherPlayer.GetParty.partyDic.Add(joinPlayerInfo.pIndex, joinPlayerInfo);
            }
        }

        // ���� ���� ����
        partyDic.Add(joinPlayerInfo.pIndex, joinPlayerInfo);

        GameObject joinPlayerObject = joinPlayerInfo.playerObject;

        // �����ڿ��� �̹� ���ŵ� �÷��̾� ����� �־��ش�.
        if (joinPlayerObject != null && joinPlayerObject.TryGetComponent(out Player joinPlayer))
        {
            foreach (var pair in partyDic)
            {
                joinPlayer.GetParty.partyDic.Add(pair.Key, pair.Value);
            }
        }
    }

    // ��Ƽ Ż��
    [Command(requiresAuthority = false)]
    public void CTS_SecessionParty()
    {
        // ��Ƽ ����
        if (partyDic.Count == 0)
            return;

        // ��Ƽ���̸�
        if (IsMaster)
        {
            // ��Ƽ�� ����
            IsMaster = false;

            // ��Ƽ�� ��ο��� ��Ƽ�� ������� �˷��ش�.
            foreach (var pair in partyDic)
            {
                // ���� ��ŵ
                if (pair.Key == partyIndex)
                    continue;

                if (pair.Value.playerObject != null && pair.Value.playerObject.TryGetComponent(out Player player))
                {
                    player.GetParty.partyDic.Clear();
                }
            }

        }
        // ��Ƽ���� �ƴϸ�
        else
        {
            // ���� ��Ƽ ������������ �ش� �÷��̾ �����.
            foreach (var pair in partyDic)
            {
                // ���� ��ŵ
                if (pair.Key == partyIndex)
                    continue;

                if (pair.Value.playerObject != null && pair.Value.playerObject.TryGetComponent(out Player player))
                {
                    player.GetParty.partyDic.Remove(partyIndex);
                }
            }
        }

        // ���� ���� Ŭ����
        // �ش� �÷��̾��� ��Ƽ ��ųʸ� Ŭ����
        partyDic.Clear();
    }

    [Command(requiresAuthority = false)]
    public void CTS_ResignPlayerToParty(long id)
    {
        if (IsMaster == false)
            return;

        if (partyDic.ContainsKey(id) == false)
            return;

        // �ڱ� �ڽ��� ��� ��ŵ
        if (partyIndex == id)
            return;

        // ���� ������Ʈ�� ������ ��ŵ
        if (partyDic.ContainsKey(id) == false)
            return;

        // �ش� �÷��̾�� ��Ƽ�� ������.
        if (partyDic[id].playerObject != null && partyDic[id].playerObject.TryGetComponent(out Player outPlayer))
        {
            outPlayer.GetParty.CTS_SecessionParty();
        }
    }

    // ������ ��Ƽ ��û �߰��Ѵ�.
    [Command(requiresAuthority = false)]
    public void CTS_PartyApplication(PlayerInfo requestedPlayerInfo)
    {
        // ��Ƽ�� ������ �ʴ� ��ŵ
        if (IsParty)
            return;
        RPC_ShowPartyInvitation(connectionToClient, requestedPlayerInfo);
    }

    // �ش� �÷��̾ ��û�� �ް� UI�� ����ش�.
    [TargetRpc]
    public void RPC_ShowPartyInvitation(NetworkConnection conn, PlayerInfo requestedPlayerInfo)
    {
        UI_PartyInvitation uI_partyInvitation = Managers.UI.ShowPopupUI<UI_PartyInvitation>();
        uI_partyInvitation.Set(requestedPlayerInfo);
    }

    // ��Ƽ�� ������ ��Ƽ ��ųʸ����� �÷��̾� ������Ʈ�� ��� ���� ������ �ڱ��ڽ��� ����
    public List<GameObject> GetPlayerList()
    {
        List<GameObject> list = new List<GameObject>();

        if(IsParty)
        {
            foreach (var pInfo in partyDic.Values)
            {
                list.Add(pInfo.playerObject);
            }
        }
        else
        {
            list.Add(myPlayer.gameObject);
        }
      
        return list;
    }


    [ServerCallback]
    private void OnDestroy()
    {
        // ��Ƽ ����
        if (IsParty == false)
            return;

        // ��Ƽ���̸�
        if (IsMaster)
        {
            // ��Ƽ�� ����
            IsMaster = false;

            // ��Ƽ�� ��ο��� ��Ƽ�� ������� �˷��ش�.
            foreach (var pair in partyDic)
            {
                // ���� ��ŵ
                if (pair.Key == partyIndex)
                    continue;

                if (pair.Value.playerObject != null && pair.Value.playerObject.TryGetComponent(out Player player))
                {
                    player.GetParty.partyDic.Clear();
                }
            }

        }
        // ��Ƽ���� �ƴϸ�
        else
        {
            // ���� ��Ƽ ������������ �ش� �÷��̾ �����.
            foreach (var pair in partyDic)
            {
                // ���� ��ŵ
                if (pair.Key == partyIndex)
                    continue;

                if (pair.Value.playerObject != null && pair.Value.playerObject.TryGetComponent(out Player player))
                {
                    player.GetParty.partyDic.Remove(partyIndex);
                }
            }
        }
    }


}