using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Notice : UI_Popup
{
    enum Texts
    {
        ContextText
    }

    enum Buttons
    {
        ConfirmButton
    }

    public override void Init()
    {
        base.Init();
        Bind<Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        Get<Button>((int)Buttons.ConfirmButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);
            Managers.UI.ClosePopupUI();
        });

        Managers.Sound.Play2D("FX/Error", Define.Sound2D.Effect2D);
    }

    public void SetContext(string context)
    {
        Get<Text>((int)Texts.ContextText).text = context;
    }
}
