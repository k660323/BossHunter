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

        // �ٷ� ȣ��Ʈ ���� ���� �� ĳ���� ���� ������ �̵�
        Get<Button>((int)Buttons.HostPlayButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            StartCoroutine(JoinWorld());
        });

        // IP�ּ� ���� �˾� �ٿ��
        Get<Button>((int)Buttons.JoinServerButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            Managers.UI.ShowPopupUI<UI_ServerJoin>();
        });

        // �ɼ� â ����
        Get<Button>((int)Buttons.OptionButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            Managers.UI.ShowPopupUI<UI_Preferences>();
        });

        // ���� ���� �˾� ����
        Get<Button>((int)Buttons.GameExitButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            Managers.UI.ShowPopupUI<UI_GameExit>();
        });

        // ���� ���� ǥ��
        Get<Text>((int)Texts.VersionText).text = $"Ver {Application.version}";
    }

    IEnumerator JoinWorld()
    {
        yield return Managers.Instance.FadeInOut.FadeIn();
        Managers.Instance.StartHost();
    }
}
