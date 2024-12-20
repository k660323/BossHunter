using Mirror.Examples.MultipleMatch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Interaction : UI_Base
{
    enum Texts
    {
        PlayerInfoText
    }

    enum Buttons
    {
        PartyInvitationButton,
        CloseButton
    }

    Player targetPlayer;

    public override void Init()
    {
        Bind<Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        // 파티 요청
        Get<Button>((int)Buttons.PartyInvitationButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);

            if (targetPlayer == null || Player.Instance == null)
                return;
            if (Player.Instance.GetParty.IsMaster == false)
            {
                Managers.UI.ShowPopupUI<UI_Notice>().SetContext("파티장만이 파티초대를 할 수 있습니다.");
                return;
            }

            PlayerInfo playerInfo = new PlayerInfo() { pIndex = Player.Instance.GetParty.GetPartyIndex, level = Player.Instance.GetStat.Level, creatureName = Player.Instance.GetStat.CreatureName, nickName = Player.Instance.GetClientInfo.NickName, isMaster = true, playerObject = Player.Instance.gameObject };
            targetPlayer.GetParty.CTS_PartyApplication(playerInfo);

            gameObject.SetActive(false);
        });

        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);
            gameObject.SetActive(false);
        });
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void SetPlayerInfo(Vector2 startPos, Player targetPlayer)
    {
        transform.position = startPos;
        this.targetPlayer = targetPlayer;

        string text = $"Lv : {targetPlayer.GetStat.Level} {targetPlayer.GetStat.CreatureName}\n{targetPlayer.GetClientInfo.NickName}";
        Get<Text>((int)Texts.PlayerInfoText).text = text;

        gameObject.SetActive(true);
    }
}
