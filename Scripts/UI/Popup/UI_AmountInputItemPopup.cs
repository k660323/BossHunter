using System;
using TMPro;
using UnityEngine.UI;

public class UI_AmountInputItemPopup : UI_Popup
{
    enum Texts
    {
        ItemNameText,
        ContentText
    }

    enum Buttons
    {
        MinusButton,
        PlusButton,
        OkButton,
        CancelButton
    }

    enum TMP_InputFields
    {
        InputField
    }

    // 확인 버튼 눌렀을 때 동작할 이벤트
    private event Action<int> OnAmountInputOK;
    // 수량 입력 제한 개수
    private int _maxAmount;


    public override void Init()
    {
        base.Init();
         
        Bind<Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));
        Bind<TMP_InputField>(typeof(TMP_InputFields));

        Get<Button>((int)Buttons.MinusButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            TMP_InputField text = Get<TMP_InputField>((int)TMP_InputFields.InputField);
            int.TryParse(text.text, out int amount);
            if (amount > 1)
            {
                int nextAmount = amount - 1;
                if (nextAmount < 1)
                    nextAmount = 1;
                text.text = nextAmount.ToString();
            }
        });

        Get<Button>((int)Buttons.PlusButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            TMP_InputField text = Get<TMP_InputField>((int)TMP_InputFields.InputField);
            int.TryParse(text.text, out int amount);
            if (amount < _maxAmount)
            {
                int nextAmount = amount + 1;
                if (nextAmount > _maxAmount)
                    nextAmount = _maxAmount;
                text.text = nextAmount.ToString();
            }
        });

        Get<TMP_InputField>((int)TMP_InputFields.InputField).onValueChanged.AddListener(str =>
        {
            int.TryParse(str, out int amount);
            bool flag = false;

            if (amount < 1)
            {
                flag = true;
                amount = 1;
            }
            else if (amount > _maxAmount)
            {
                flag = true;
                amount = _maxAmount;
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
            OnAmountInputOK?.Invoke(int.Parse(Get<TMP_InputField>((int)TMP_InputFields.InputField).text));
            Managers.UI.ClosePopupUI();
        });


        Get<Button>((int)Buttons.CancelButton).gameObject.BindEvent((data) =>
        {
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);
            Managers.UI.ClosePopupUI();
        });
    }

    public void SetAmountInputItemPopup(Action<int> okAction, int currentAmount, string itemName, bool isDrop)
    {
        string str;
        if(isDrop)
        {
            str = "정말로 버리시겠습니까?";
            _maxAmount = currentAmount;
        }
        else
        {
            str = "나눌 개수를 입력하세요.";
            _maxAmount = currentAmount - 1;

        }
        Get<Text>((int)Texts.ContentText).text = str;

        TMP_InputField text = Get<TMP_InputField>((int)TMP_InputFields.InputField);
        text.text = "1";

        Get<Text>((int)Texts.ItemNameText).text = itemName;
        OnAmountInputOK = okAction;
    }
}