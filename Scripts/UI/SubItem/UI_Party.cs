using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Party : UI_Base
{
    enum Buttons
    {
        CloseButton,
        PartyCreateButton,
        PartSecessionButton
    }

    enum GameObjects
    {
        Content
    }

    [HideInInspector]
    public Party party;

    // ������ ������ ������ ��ġ
    private RectTransform _contentAreaRt;

    // ������ ������ ���� �̸�
    [Header("�ʼ� ����"), SerializeField]
    private string _slotPrefabName;

    Dictionary<long, UI_PartyPlayerInfoSlot> playerInfoDict = new Dictionary<long, UI_PartyPlayerInfoSlot>();

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<GameObject>(typeof(GameObjects));

        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);
            gameObject.SetActive(false);
        });

        // ��Ƽ ����
        Get<Button>((int)Buttons.PartyCreateButton).gameObject.BindEvent(data =>
        {
            if (party.IsParty)
            {
                Managers.UI.ShowPopupUI<UI_Notice>().SetContext("�̹� ��Ƽ�� �����մϴ�.");
            }
            else
            {
                Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
                party.CTS_CreateParty();
            }
        });

        // ��Ƽ Ż��
        Get<Button>((int)Buttons.PartSecessionButton).gameObject.BindEvent(data =>
        {
            if(party.IsParty)
            {
                Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
                party.CTS_SecessionParty();
            }
            else
            {
                Managers.UI.ShowPopupUI<UI_Notice>().SetContext("��Ƽ�� �������� �ʽ��ϴ�.");
            }
        });

        Get<GameObject>((int)GameObjects.Content).TryGetComponent(out _contentAreaRt);
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void AddPlayerInfoSlot(long id, PlayerInfo playerInfo)
    {
        var slotRt = CloneSlot();

        if (slotRt.TryGetComponent(out UI_PartyPlayerInfoSlot slotUI) == false)
            return;

        bool isMaster = playerInfo.isMaster;
        slotUI.SetPlayerInfo(this, id, playerInfo.nickName, isMaster);
        playerInfoDict.Add(id, slotUI);

        RectTransform CloneSlot()
        {
            GameObject slotGo = Managers.Resource.Instantiate(_slotPrefabName, null);
            if (slotGo.TryGetComponent(out RectTransform tr) == false)
            {
                Destroy(slotGo);
                return null;
            }

            tr.SetParent(_contentAreaRt, false);

            return tr;
        }
    }

    public void SetPlayerInfoSlot(long id, PlayerInfo playerInfo)
    {
        if (playerInfoDict.ContainsKey(id) == false)
            return;

        bool isMaster = playerInfo.isMaster;

        var slotUI = playerInfoDict[id];

        slotUI.SetPlayerInfo(this, id, playerInfo.nickName, isMaster);
    }

    public void RemovePlayerInfoSlot(long id)
    {
        if (playerInfoDict.ContainsKey(id) == false)
            return;

        var slotUI = playerInfoDict[id];
        Managers.Resource.Destroy(slotUI.gameObject);
        playerInfoDict.Remove(id);
    }

    public void ClearPlayerInfoSlot()
    {
        foreach(var slotUI in playerInfoDict.Values)
        {
            Managers.Resource.Destroy(slotUI.gameObject);
        }

        playerInfoDict.Clear();
    }

    public void ResignPlayerInfoSlot(uint id)
    {
        party.CTS_ResignPlayerToParty(id);
    }

}
