using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PartyInvitation : UI_Popup
{
    enum Texts
    {
        ContentText
    }

    enum Buttons
    {
        AcceptButton,
        IgnoreButton
    }

    PlayerInfo requestPlayerInfo;

    public override void Init()
    {
        base.Init();

        Bind<Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        // 파티 수락
        Get<Button>((int)Buttons.AcceptButton).gameObject.BindEvent(data =>
        {
            if (Player.Instance == null || Player.Instance.GetParty.IsMaster)
                return;

            if (requestPlayerInfo.playerObject.TryGetComponent(out Player player) == false)
                return;

            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            PlayerInfo playerInfo = new PlayerInfo() { pIndex = Player.Instance.GetParty.GetPartyIndex, level = Player.Instance.GetStat.Level, creatureName = Player.Instance.GetStat.CreatureName, nickName = Player.Instance.GetClientInfo.NickName, isMaster = false, playerObject = Player.Instance.gameObject};
            Player.Instance.GetParty.CTS_RequsetJoinParty(requestPlayerInfo, playerInfo);
            Managers.UI.ClosePopupUI();
        });

        Get<Button>((int)Buttons.IgnoreButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);
            Managers.UI.ClosePopupUI();
        });

        Managers.Sound.Play2D("FX/Notificationsound12", Define.Sound2D.Effect2D);
    }

    public void Set(PlayerInfo requestPlayerInfo)
    {
        this.requestPlayerInfo = requestPlayerInfo;

        Get<Text>((int)Texts.ContentText).text = $"Lv : {requestPlayerInfo.level} {requestPlayerInfo.creatureName}\n" +
            $"'{requestPlayerInfo.nickName}'님의\n" +
            $"파티 초대 입니다.";

    }
}
