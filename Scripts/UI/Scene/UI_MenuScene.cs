using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_MenuScene : UI_Scene
{
    enum Buttons
    {
        HostPlayButton,
        JoinServerButton,
        OptionButton,
        GameExitButton
    }

    enum Texts
    {
        VersionText,
    }

    public override void Init()
    {
       base.Init();

        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));

        // 바로 호스트 서버 생성 및 캐릭터 선택 씬으로 이동
        Get<Button>((int)Buttons.HostPlayButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            StartCoroutine(JoinWorld());
        });

        // IP주소 적는 팝업 뛰우기
        Get<Button>((int)Buttons.JoinServerButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            Managers.UI.ShowPopupUI<UI_ServerJoin>();
        });

        // 옵션 창 띄우기
        Get<Button>((int)Buttons.OptionButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            Managers.UI.ShowPopupUI<UI_Preferences>();
        });

        // 게임 종료 팝업 띄우기
        Get<Button>((int)Buttons.GameExitButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            Managers.UI.ShowPopupUI<UI_GameExit>();
        });

        // 게임 버전 표시
        Get<Text>((int)Texts.VersionText).text = $"Ver {Application.version}";
    }

    IEnumerator JoinWorld()
    {
        yield return Managers.Instance.FadeInOut.FadeIn();
        Managers.Instance.StartHost();
    }
}
