using UnityEngine;
using UnityEngine.UI;

public class UI_Sound : UI_Base
{
    UI_Preferences parent;

    enum Sliders
    {
        MasterVolSlider,
        BgVolSlider,
        EffectVolSlider
    }

    enum InputFields
    {
        MasterVolInputField,
        BgVolInputField,
        EffectVolInputField
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

        Get<Slider>((int)Sliders.MasterVolSlider).minValue = Managers.Option.sOption.volMin;
        Get<Slider>((int)Sliders.MasterVolSlider).maxValue = Managers.Option.sOption.volMax;
        Get<Slider>((int)Sliders.BgVolSlider).minValue = Managers.Option.sOption.volMin;
        Get<Slider>((int)Sliders.BgVolSlider).maxValue = Managers.Option.sOption.volMax;
        Get<Slider>((int)Sliders.EffectVolSlider).minValue = Managers.Option.sOption.volMin;
        Get<Slider>((int)Sliders.EffectVolSlider).maxValue = Managers.Option.sOption.volMax;

        Get<Slider>((int)Sliders.MasterVolSlider).value = Managers.Option.sOption.masterVol;
        Get<Slider>((int)Sliders.BgVolSlider).value = Managers.Option.sOption.backgroundVol;
        Get<Slider>((int)Sliders.EffectVolSlider).value = Managers.Option.sOption.effectVol;

        Get<InputField>((int)InputFields.MasterVolInputField).text = (Managers.Option.sOption.masterVol).ToString();
        Get<InputField>((int)InputFields.BgVolInputField).text = (Managers.Option.sOption.backgroundVol).ToString();
        Get<InputField>((int)InputFields.EffectVolInputField).text = (Managers.Option.sOption.effectVol).ToString();

        Get<Slider>((int)Sliders.MasterVolSlider).onValueChanged.AddListener(value =>
        {
            int _value = (int)value;
            Get<InputField>((int)InputFields.MasterVolInputField).text = _value.ToString();
            Managers.Option.sOption.ApplyOption(_value, Managers.Option.sOption.backgroundVol, Managers.Option.sOption.effectVol);
        });

        Get<Slider>((int)Sliders.BgVolSlider).onValueChanged.AddListener(value =>
        {
            int _value = (int)value;
            Get<InputField>((int)InputFields.BgVolInputField).text = _value.ToString();
            Managers.Option.sOption.ApplyOption(Managers.Option.sOption.masterVol, _value, Managers.Option.sOption.effectVol);
        });

        Get<Slider>((int)Sliders.EffectVolSlider).onValueChanged.AddListener(value =>
        {
            int _value = (int)value;
            Get<InputField>((int)InputFields.EffectVolInputField).text = _value.ToString();
            Managers.Option.sOption.ApplyOption(Managers.Option.sOption.masterVol, Managers.Option.sOption.backgroundVol, _value);
        });

        Get<InputField>((int)InputFields.MasterVolInputField).onEndEdit.AddListener(value => 
        {
            int _value = Mathf.Clamp(int.Parse(value), 0, 100);
            Get<InputField>((int)InputFields.MasterVolInputField).text = _value.ToString();
            Get<Slider>((int)Sliders.MasterVolSlider).value = _value;
            Managers.Option.sOption.ApplyOption(_value, Managers.Option.sOption.backgroundVol, Managers.Option.sOption.effectVol);
        });

        Get<InputField>((int)InputFields.BgVolInputField).onEndEdit.AddListener(value =>
        {
            int _value = Mathf.Clamp(int.Parse(value), 0, 100);
            Get<InputField>((int)InputFields.BgVolInputField).text = _value.ToString();
            Get<Slider>((int)Sliders.BgVolSlider).value = _value;
            Managers.Option.sOption.ApplyOption(Managers.Option.sOption.masterVol, _value, Managers.Option.sOption.effectVol);
        });

        Get<InputField>((int)InputFields.EffectVolInputField).onEndEdit.AddListener(value =>
        {
            int _value = Mathf.Clamp(int.Parse(value), 0, 100);
            Get<InputField>((int)InputFields.EffectVolInputField).text = _value.ToString();
            Get<Slider>((int)Sliders.EffectVolSlider).value = _value;
            Managers.Option.sOption.ApplyOption(Managers.Option.sOption.masterVol, Managers.Option.sOption.backgroundVol, _value);
        });

        parent = transform.parent.GetComponent<UI_Preferences>();
        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent((data) => { Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D); Managers.Option.sOption.ApplyOption(); Managers.UI.ClosePopupUI(parent); });
    }
}
