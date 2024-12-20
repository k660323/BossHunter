using UnityEngine;
using UnityEngine.UI;

public class UI_GameExit : UI_Popup
{
    enum Buttons
    {
        YesButton,
        NoButton
    }

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));

        Get<Button>((int)Buttons.YesButton).gameObject.BindEvent(data => {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        });

        Get<Button>((int)Buttons.NoButton).gameObject.BindEvent(data => {
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);
            Managers.UI.ClosePopupUI();
        });
    }
}
