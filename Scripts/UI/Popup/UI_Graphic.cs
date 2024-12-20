using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Graphic : UI_Base
{
    List<Resolution> resolutions = new List<Resolution>();

    UI_Preferences parent;

    enum Dropdowns
    {
        ResolutionDropdown,
        GraphicQualityDropdown
    }

    enum Toggles
    {
        FullScreenToggle,
        SyncToggle,
        FrameToggle
    }

    enum Sliders
    {
        TargetFrameSlider
    }

    enum InputFields
    {
        TargetFrameInputField
    }

    enum Buttons
    {
        ApplyButton,
        CloseButton
    }

    public override void Init()
    {
        Bind<Dropdown>(typeof(Dropdowns));
        Bind<Toggle>(typeof(Toggles));
        Bind<Slider>(typeof(Sliders));
        Bind<InputField>(typeof(InputFields));
        Bind<Button>(typeof(Buttons));

        // 해상도를 넣어줘야함..
        ResoultionInit();

        // 전체 화면
        Get<Toggle>((int)Toggles.FullScreenToggle).isOn = Managers.Option.gOption.isFullScreen;

        // 목표 프레임
        Get<Slider>((int)Sliders.TargetFrameSlider).minValue = Managers.Option.gOption.minRefreshRate;
        Get<Slider>((int)Sliders.TargetFrameSlider).maxValue = Managers.Option.gOption.maxRefreshRate;
        Get<Slider>((int)Sliders.TargetFrameSlider).value = Managers.Option.gOption.targetFrame;
        Get<Slider>((int)Sliders.TargetFrameSlider).onValueChanged.AddListener(value => { FrameSlideToInputField((int)value); });

        Get<InputField>((int)InputFields.TargetFrameInputField).text = Managers.Option.gOption.targetFrame.ToString();
        Get<InputField>((int)InputFields.TargetFrameInputField).onEndEdit.AddListener(value => { InputFieldToFrameSlide(value); });
        
        // 수직 동기화
        Get<Toggle>((int)Toggles.SyncToggle).isOn = Managers.Option.gOption.isSync;

        // 프레임 카운터
        Get<Toggle>((int)Toggles.FrameToggle).isOn = Managers.Option.gOption.isFrame;

        // 그래픽 품질
        Get<Dropdown>((int)Dropdowns.GraphicQualityDropdown).value = Managers.Option.gOption.graphicQualityIndex;

        // 적용
        Get<Button>((int)Buttons.ApplyButton).gameObject.BindEvent((data) => SetOption());
        // 나가기
        parent = transform.parent.GetComponent<UI_Preferences>();
        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent((data) => { Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D); Managers.UI.ClosePopupUI(parent); });
    }

    void ResoultionInit()
    {
        var dropDown = Get<Dropdown>((int)Dropdowns.ResolutionDropdown);
        dropDown.options.Clear();

        resolutions.AddRange(Screen.resolutions);
        
        Resolution data = new Resolution();
        data.width = Managers.Option.gOption.width;
        data.height = Managers.Option.gOption.height;
        data.refreshRate = Managers.Option.gOption.refreshRate;

        int screenIndex = 0;
        int value = 0;

        foreach (Resolution item in resolutions)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = item.width + " x " + item.height + " @ " + item.refreshRate + "hz";
            dropDown.options.Add(option);

            if (item.refreshRate == data.refreshRate && item.height == data.height && item.refreshRate == data.refreshRate)
            {
                screenIndex = value;
            }
            value++;
        }
        dropDown.RefreshShownValue();

        dropDown.value = screenIndex;
    }

    void FrameSlideToInputField(int value)
    {
        Get<InputField>((int)InputFields.TargetFrameInputField).text = value.ToString();
    }

    void InputFieldToFrameSlide(string value)
    {
        Slider frame = Get<Slider>((int)Sliders.TargetFrameSlider);
        if (int.TryParse(value, out int num))
        {
            frame.value = num;
        }
        else
        {
            Get<InputField>((int)InputFields.TargetFrameInputField).text = frame.value.ToString();
        }
    }

    void SetOption()
    {
        Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
        Resolution resolution = resolutions[Get<Dropdown>((int)Dropdowns.ResolutionDropdown).value];
        bool isFull = Get<Toggle>((int)Toggles.FullScreenToggle).isOn;
        bool isVsync = Get<Toggle>((int)Toggles.SyncToggle).isOn;
        bool isFrame = Get<Toggle>((int)Toggles.FrameToggle).isOn;
        int targetFrame = (int)Get<Slider>((int)Sliders.TargetFrameSlider).value;
        int qualityindex = Get<Dropdown>((int)Dropdowns.GraphicQualityDropdown).value;
        Managers.Option.gOption.ApplyOption(resolution.width, resolution.height, isFull, resolution.refreshRate, isVsync, isFrame, targetFrame, qualityindex);
    }
}
