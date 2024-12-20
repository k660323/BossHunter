using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class UI_NetworkObjectInfo : UI_Base
{
    // 카메라 방향으로 바라보는 컴포넌트
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

    // Awake에서 하지 않고 NetworkObjectInfo가 수동으로 초기화 해준다.
    // Awake보다 StartClient가 빠른 버그? 발생 아마 자식 컴포넌트라 순서가 밀린듯 하다.
    public override void Init()
    {
        TryGetComponent(out lookAtMainCamera);
        TryGetComponent(out canvasGroup);
        Bind<Text>(typeof(Texts));
        Bind<Slider>(typeof(Sliders));
        Bind<GameObject>(typeof(GameObjects));
        Bind<Image>(typeof(Images));
        // 50칸 까지 빌보드 적용
        lookAtMainCamera.Init(Get<GameObject>((int)GameObjects.UIGroup), 2500.0f);
    }

    // UI_Text에 닉네임 갱신
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

    // 플레이어 오브젝트 색상 설정
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
