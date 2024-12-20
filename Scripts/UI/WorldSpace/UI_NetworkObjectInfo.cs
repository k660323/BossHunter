using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class UI_NetworkObjectInfo : UI_Base
{
    // ī�޶� �������� �ٶ󺸴� ������Ʈ
    protected LookAtMainCamera lookAtMainCamera;
    public LookAtMainCamera GetlookAtMainCamera { get { return lookAtMainCamera; } }

    protected CanvasGroup canvasGroup;

    public CanvasGroup GetcanvasGroup { get { return canvasGroup; } }

    protected enum Texts
    {
        NickNameText,
        LevelText
    }

    protected enum Sliders
    {
        HpSlider
    }

    protected enum GameObjects
    {
        UIGroup
    }

    protected enum Images
    {
        Fill
    }

    [ClientCallback]
    protected override void Awake()
    {
        
    }

    // Awake���� ���� �ʰ� NetworkObjectInfo�� �������� �ʱ�ȭ ���ش�.
    // Awake���� StartClient�� ���� ����? �߻� �Ƹ� �ڽ� ������Ʈ�� ������ �и��� �ϴ�.
    public override void Init()
    {
        TryGetComponent(out lookAtMainCamera);
        TryGetComponent(out canvasGroup);
        Bind<Text>(typeof(Texts));
        Bind<Slider>(typeof(Sliders));
        Bind<GameObject>(typeof(GameObjects));
        Bind<Image>(typeof(Images));
        // 50ĭ ���� ������ ����
        lookAtMainCamera.Init(Get<GameObject>((int)GameObjects.UIGroup), 2500.0f);
    }

    // UI_Text�� �г��� ����
    public void UpdateNickName(string nickName)
    {
        Text text = Get<Text>((int)Texts.NickNameText);
        text.text = nickName;
    }

    public void UpdateLevel(int level)
    {
        Text text = Get<Text>((int)Texts.LevelText);
        text.text = $"Lv : {level}";
    }

    // �÷��̾� ������Ʈ ���� ����
    public void SetHpBarColor(Color color)
    {
        Get<Image>((int)Images.Fill).color = color;
    }

    public void SetHpBarObject(bool isActive)
    {
        Get<Slider>((int)Sliders.HpSlider).gameObject.SetActive(isActive);
    }

    public void UpdateHpBar(int curHp, int maxHp)
    {
        Get<Slider>((int)Sliders.HpSlider).value = (float)curHp / maxHp;
    }
}
