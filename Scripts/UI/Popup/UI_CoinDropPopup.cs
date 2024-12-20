using System;
using TMPro;
using UnityEngine.UI;

public class UI_CoinDropPopup : UI_Popup
{
    enum TMP_InputFields
    {
        InputField
    }

    enum Buttons
    {
        OkButton,
        CancelButton
    }

    // 확인 버튼 눌렀을 때 동작할 이벤트
    private event Action<long> OnCoinInputOK;
    private long _maxCoin;

    public override void Init()
    {
        base.Init();
        Bind<TMP_InputField>(typeof(TMP_InputFields));
        Bind<Button>(typeof(Buttons));

        Get<TMP_InputField>((int)TMP_InputFields.InputField).onValueChanged.AddListener(str =>
        {
            long.TryParse(str, out long amount);
            bool flag = false;

            if (amount < 0)
            {
                flag = true;
                amount = 0;
            }
            else if (amount > _maxCoin)
            {
                flag = true;
                amount = _maxCoin;
            }

            if (flag)
            {
                TMP_InputField text = Get<TMP_InputField>((int)TMP_InputFields.InputField);
                text.text = amount.ToString();
            }
        });

        Get<Button>((int)Buttons.OkButton).gameObject.BindEvent((data) =>
        {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            OnCoinInputOK?.Invoke(long.Parse(Get<TMP_InputField>((int)TMP_InputFields.InputField).text));
            Managers.UI.ClosePopupUI();
        });


        Get<Button>((int)Buttons.CancelButton).gameObject.BindEvent((data) =>
        {
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);
            Managers.UI.ClosePopupUI();
        });
    }

    public void SetCoinInputPopup(Action<long> okAction, long currentCoin)
    {
        _maxCoin = currentCoin;

        TMP_InputField text = Get<TMP_InputField>((int)TMP_InputFields.InputField);
        text.text = "0";

        OnCoinInputOK = okAction;
    }
}
