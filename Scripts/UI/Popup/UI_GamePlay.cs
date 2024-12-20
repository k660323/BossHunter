using UnityEngine;
using UnityEngine.UI;

public class UI_GamePlay : UI_Base
{
    UI_Preferences parent;

    enum Sliders
    {
        MouseVerticalSlider,
        MouseHorizontalSlider,
        WheelSlider
    }

    enum InputFields
    {
        MouseVerticalInputField,
        MouseHorizontalInputField,
        WheelInputField
    }

    enum Buttons
    {
        CloseButton
    }

    public override void Init()
    {
        Bind<Slider>(typeof(Sliders));
        Bind<InputField>(typeof(InputFields));
        Bind<Button>(typeof(Buttons));

        Get<Slider>((int)Sliders.MouseHorizontalSlider).minValue = Managers.Option.gamePlayOption.mouseHVMin;
        Get<Slider>((int)Sliders.MouseHorizontalSlider).maxValue = Managers.Option.gamePlayOption.mouseHVMax;
        Get<Slider>((int)Sliders.MouseVerticalSlider).minValue = Managers.Option.gamePlayOption.mouseHVMin;
        Get<Slider>((int)Sliders.MouseVerticalSlider).maxValue = Managers.Option.gamePlayOption.mouseHVMax;
        Get<Slider>((int)Sliders.WheelSlider).minValue = Managers.Option.gamePlayOption.wheelMin;
        Get<Slider>((int)Sliders.WheelSlider).maxValue = Managers.Option.gamePlayOption.wheelMax;

        Get<Slider>((int)Sliders.MouseHorizontalSlider).value = Managers.Option.gamePlayOption.mouseHorizontal;
        Get<Slider>((int)Sliders.MouseVerticalSlider).value = Managers.Option.gamePlayOption.mouseVirtical;
        Get<Slider>((int)Sliders.WheelSlider).value = Managers.Option.gamePlayOption.wheel;

        Get<InputField>((int)InputFields.MouseHorizontalInputField).text = (Managers.Option.gamePlayOption.mouseHorizontal).ToString();
        Get<InputField>((int)InputFields.MouseVerticalInputField).text = (Managers.Option.gamePlayOption.mouseVirtical).ToString();
        Get<InputField>((int)InputFields.WheelInputField).text = (Managers.Option.gamePlayOption.wheel).ToString();

        Get<Slider>((int)Sliders.MouseHorizontalSlider).onValueChanged.AddListener(value =>
        {
            int _value = Mathf.Clamp((int)value, Managers.Option.gamePlayOption.mouseHVMin, Managers.Option.gamePlayOption.mouseHVMax);
            Get<InputField>((int)InputFields.MouseHorizontalInputField).text = _value.ToString();
            Managers.Option.gamePlayOption.ApplyOption(_value, Managers.Option.gamePlayOption.mouseVirtical, Managers.Option.gamePlayOption.wheel);
        });

        Get<Slider>((int)Sliders.MouseVerticalSlider).onValueChanged.AddListener(value =>
        {
            int _value = Mathf.Clamp((int)value, Managers.Option.gamePlayOption.mouseHVMin, Managers.Option.gamePlayOption.mouseHVMax);
            Get<InputField>((int)InputFields.MouseVerticalInputField).text = _value.ToString();
            Managers.Option.gamePlayOption.ApplyOption(Managers.Option.gamePlayOption.mouseHorizontal, _value, Managers.Option.gamePlayOption.wheel);
        });

        Get<Slider>((int)Sliders.WheelSlider).onValueChanged.AddListener(value =>
        {
            int _value = Mathf.Clamp((int)value, Managers.Option.gamePlayOption.wheelMin, Managers.Option.gamePlayOption.wheelMax);
            Get<InputField>((int)InputFields.WheelInputField).text = _value.ToString();
            Managers.Option.gamePlayOption.ApplyOption(Managers.Option.gamePlayOption.mouseHorizontal, Managers.Option.gamePlayOption.mouseVirtical, _value);
        });

        Get<InputField>((int)InputFields.MouseHorizontalInputField).onEndEdit.AddListener(value =>
        {
            int _value = Mathf.Clamp(int.Parse(value), Managers.Option.gamePlayOption.mouseHVMin, Managers.Option.gamePlayOption.mouseHVMax);
            Get<InputField>((int)InputFields.MouseHorizontalInputField).text = _value.ToString();
            Get<Slider>((int)Sliders.MouseHorizontalSlider).value = _value;
            Managers.Option.gamePlayOption.ApplyOption(_value, Managers.Option.gamePlayOption.mouseVirtical, Managers.Option.gamePlayOption.wheel);
        });

        Get<InputField>((int)InputFields.MouseVerticalInputField).onEndEdit.AddListener(value =>
        {
            int _value = Mathf.Clamp(int.Parse(value), Managers.Option.gamePlayOption.mouseHVMin, Managers.Option.gamePlayOption.mouseHVMax);
            Get<InputField>((int)InputFields.MouseVerticalInputField).text = _value.ToString();
            Get<Slider>((int)Sliders.MouseVerticalSlider).value = _value;
            Managers.Option.gamePlayOption.ApplyOption(Managers.Option.gamePlayOption.mouseHorizontal, _value, Managers.Option.gamePlayOption.wheel);
        });

        Get<InputField>((int)InputFields.WheelInputField).onEndEdit.AddListener(value =>
        {
            int _value = Mathf.Clamp(int.Parse(value), Managers.Option.gamePlayOption.wheelMin, Managers.Option.gamePlayOption.wheelMax);
            Get<InputField>((int)InputFields.WheelInputField).text = _value.ToString();
            Get<Slider>((int)Sliders.WheelSlider).value = _value;
            Managers.Option.gamePlayOption.ApplyOption(Managers.Option.gamePlayOption.mouseHorizontal, Managers.Option.gamePlayOption.mouseVirtical, _value);
        });

        parent = transform.parent.GetComponent<UI_Preferences>();
        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);
            Managers.Option.gamePlayOption.ApplyOption(); 
            Managers.UI.ClosePopupUI(parent); 
        });
    }
}
