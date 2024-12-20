using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ConfirmItemPopup : UI_Popup
{
    enum Texts
    {
        ItemNameText,
        ContentText
    }

    enum Buttons
    {
        OkButton,
        CancelButton
    }

    private event Action OnConfirmationOK; // 확인 버튼 누를 경우 실행할 이벤트

    public override void Init()
    {
        base.Init();

        Bind<Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        Get<Button>((int)Buttons.OkButton).gameObject.BindEvent(data => 
        {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            OnConfirmationOK?.Invoke();
            Managers.UI.ClosePopupUI();
        });

        Get<Button>((int)Buttons.CancelButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);
            Managers.UI.ClosePopupUI();
        });
    }

    public void SetConfirmItemPopup(Action okAction, string itemName)
    {
        Get<Text>((int)Texts.ItemNameText).text = itemName;
        OnConfirmationOK = okAction;
    }
}
