using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Setting : UI_Popup
{
    enum Buttons
    {
        ExitButton,
        OptionButton,
        GoTownButton,
        DisconnectButton,
        GameCloseButton
    }

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));

        // 창 닫기
        Get<Button>((int)Buttons.ExitButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);
            Managers.UI.ClosePopupUI();
        });

        // 옵션 창 띄우기
        Get<Button>((int)Buttons.OptionButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            Managers.UI.ShowPopupUI<UI_Preferences>();
        });

        Get<Button>((int)Buttons.GoTownButton).gameObject.BindEvent((data) =>
        {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);

            GameObject myPlayer = NetworkClient.localPlayer.gameObject;
            if (myPlayer == null)
            {
                Managers.Sound.Play2D("FX/Error", Define.Sound2D.Effect2D);
                return;
            }

            string destinationScene = Enum.GetName(typeof(Define.Scene), Define.Scene.Town);
            if (myPlayer.scene.name == destinationScene)
            {
                Managers.Sound.Play2D("FX/Error", Define.Sound2D.Effect2D);
                return;
            }

            // 캐릭터 생성 및 맵 이동 (메시지 전송)
            MoveToScene message = new MoveToScene
            {
                player = myPlayer,
                sceneType = Define.Scene.Town,
                destinationScene = destinationScene
            };

            NetworkClient.Send(message);
        });

        // 월드 퇴장
        Get<Button>((int)Buttons.DisconnectButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);

            if (Managers.Instance.mode == Mirror.NetworkManagerMode.ClientOnly)
                Managers.Instance.StopClient();
            else if (Managers.Instance.mode == Mirror.NetworkManagerMode.Host)
                Managers.Instance.StopHost();
        });

        // 게임 종료
        Get<Button>((int)Buttons.GameCloseButton).gameObject.BindEvent((data) =>
        {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);

            if (Managers.Instance.mode == Mirror.NetworkManagerMode.ClientOnly)
                Managers.Instance.StopClient();
            else if (Managers.Instance.mode == Mirror.NetworkManagerMode.Host)
                Managers.Instance.StopHost();

            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        });
    }
}
