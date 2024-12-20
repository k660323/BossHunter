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
    // 플레이어 전용 인덱스
    static long GenerationPartyIndex = 0;

    public long GetGenerationPartyIndex { get { return GenerationPartyIndex++; } }

    [SyncVar]
    long partyIndex = -1;

    public long GetPartyIndex { get { return partyIndex; } }

    // 플레이어
    Player myPlayer;

    // 연결된 파티 UI
    [SerializeField]
    private UI_Party uI_Party;

    // 파티 생성 여부
    public bool IsParty => partyDic.Count > 0;

    // 방장 여부
    [SyncVar]
    public bool IsMaster = false;

    // 파티 목록
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

    // UI 업데이트
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

    // 파티 만들기
    [Command]
    public void CTS_CreateParty()
    {
        // 이미 파티 있음
        if (IsParty)
            return;

        // 파장
        IsMaster = true;
        partyDic.Add(partyIndex, new PlayerInfo() { pIndex = partyIndex, level = myPlayer.GetStat.Level, creatureName = myPlayer.GetStat.CreatureName, nickName = myPlayer.GetClientInfo.NickName, isMaster = true, playerObject = gameObject });
    }

    // 파티 참여
    [Command(requiresAuthority = false)]
    public void CTS_RequsetJoinParty(PlayerInfo requestPlayerInfo, PlayerInfo JoinPlayerInfo)
    {
        if (requestPlayerInfo.playerObject == null)
            return;

        if (requestPlayerInfo.playerObject.TryGetComponent(out Player masterPlayer) == false)
            return;

        masterPlayer.GetParty.JoinParty(JoinPlayerInfo);
        
    }

    // 방장 기준으로 플레이어를 추가하고 알려준다.
    public void JoinParty(PlayerInfo joinPlayerInfo)
    {
        // 이미 참여한 플레이어에게 추가된 플레이어를 알린다.
        foreach (PlayerInfo pInfo in partyDic.Values)
        {
            // 나는 스킵
            if (pInfo.pIndex == partyIndex)
            {
                continue;
            }
            else if (pInfo.playerObject != null && pInfo.playerObject.TryGetComponent(out Player otherPlayer))
            {
                otherPlayer.GetParty.partyDic.Add(joinPlayerInfo.pIndex, joinPlayerInfo);
            }
        }

        // 방장 정보 갱신
        partyDic.Add(joinPlayerInfo.pIndex, joinPlayerInfo);

        GameObject joinPlayerObject = joinPlayerInfo.playerObject;

        // 참여자에게 이미 갱신된 플레이어 목록을 넣어준다.
        if (joinPlayerObject != null && joinPlayerObject.TryGetComponent(out Player joinPlayer))
        {
            foreach (var pair in partyDic)
            {
                joinPlayer.GetParty.partyDic.Add(pair.Key, pair.Value);
            }
        }
    }

    // 파티 탈퇴
    [Command(requiresAuthority = false)]
    public void CTS_SecessionParty()
    {
        // 파티 없음
        if (partyDic.Count == 0)
            return;

        // 파티장이면
        if (IsMaster)
        {
            // 파티장 제거
            IsMaster = false;

            // 파티원 모두에게 파티가 사라짐을 알려준다.
            foreach (var pair in partyDic)
            {
                // 나는 스킵
                if (pair.Key == partyIndex)
                    continue;

                if (pair.Value.playerObject != null && pair.Value.playerObject.TryGetComponent(out Player player))
                {
                    player.GetParty.partyDic.Clear();
                }
            }

        }
        // 파티장이 아니면
        else
        {
            // 현재 파티 구성원들한테 해당 플레이어를 지운다.
            foreach (var pair in partyDic)
            {
                // 나는 스킵
                if (pair.Key == partyIndex)
                    continue;

                if (pair.Value.playerObject != null && pair.Value.playerObject.TryGetComponent(out Player player))
                {
                    player.GetParty.partyDic.Remove(partyIndex);
                }
            }
        }

        // 방장 정보 클리어
        // 해당 플레이어의 파티 딕셔너리 클리어
        partyDic.Clear();
    }

    [Command(requiresAuthority = false)]
    public void CTS_ResignPlayerToParty(long id)
    {
        if (IsMaster == false)
            return;

        if (partyDic.ContainsKey(id) == false)
            return;

        // 자기 자신일 경우 스킵
        if (partyIndex == id)
            return;

        // 게임 오브젝트가 없으면 스킵
        if (partyDic.ContainsKey(id) == false)
            return;

        // 해당 플레이어는 파티에 나간다.
        if (partyDic[id].playerObject != null && partyDic[id].playerObject.TryGetComponent(out Player outPlayer))
        {
            outPlayer.GetParty.CTS_SecessionParty();
        }
    }

    // 서버가 파티 요청 중계한다.
    [Command(requiresAuthority = false)]
    public void CTS_PartyApplication(PlayerInfo requestedPlayerInfo)
    {
        // 파티가 있으면 초대 스킵
        if (IsParty)
            return;
        RPC_ShowPartyInvitation(connectionToClient, requestedPlayerInfo);
    }

    // 해당 플레이어가 요청을 받고 UI를 띄어준다.
    [TargetRpc]
    public void RPC_ShowPartyInvitation(NetworkConnection conn, PlayerInfo requestedPlayerInfo)
    {
        UI_PartyInvitation uI_partyInvitation = Managers.UI.ShowPopupUI<UI_PartyInvitation>();
        uI_partyInvitation.Set(requestedPlayerInfo);
    }

    // 파티가 있으면 파티 딕셔너리에서 플레이어 오브젝트를 담아 리턴 없으면 자기자신을 리턴
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
        // 파티 없음
        if (IsParty == false)
            return;

        // 파티장이면
        if (IsMaster)
        {
            // 파티장 제거
            IsMaster = false;

            // 파티원 모두에게 파티가 사라짐을 알려준다.
            foreach (var pair in partyDic)
            {
                // 나는 스킵
                if (pair.Key == partyIndex)
                    continue;

                if (pair.Value.playerObject != null && pair.Value.playerObject.TryGetComponent(out Player player))
                {
                    player.GetParty.partyDic.Clear();
                }
            }

        }
        // 파티장이 아니면
        else
        {
            // 현재 파티 구성원들한테 해당 플레이어를 지운다.
            foreach (var pair in partyDic)
            {
                // 나는 스킵
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