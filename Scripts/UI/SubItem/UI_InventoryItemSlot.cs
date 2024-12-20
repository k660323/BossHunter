using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_InventoryItemSlot : UI_Base
{
    enum Images
    {
        ItemImage,     // 아이템 아이콘 이미지
        HighLightImage // 하이라이트 이미지
    }

    enum Texts
    {
        AmountText,    // 아이템 갯수     
    }

    enum GameObjects
    {
        ItemImage,
        AmountText,
        HighLightImage
    }

    // 아이템 슬롯 이미지
    Image ItemSlotImage;

    // 하이라이트 알파값
    [SerializeField]
    private const float highLightAlpha = 0.5f;

    // 논 하이라이트 <-> 하이라이트 까지 걸리는 시간
    [SerializeField]
    private const float highLightFadeDuration = 0.2f;

    // 현재 하이라이트 알파값
    private float currentHLAlpha = 0.0f;

    // 인벤토리에 몇번째 생성된 아이템인지 확인하는 인덱스
    [SerializeField]
    private int index;
    public int Index { get { return index; } private set { index = value; } }

    // 아이템 이미지가 존재하면 아이템이 존재한다.
    public bool HasItem { get { return Get<Image>((int)Images.ItemImage).sprite != null; } }

    // 슬롯 접근 여부
    bool isAccessibleSlot = true;
    // 아이템 접근 여부
    bool isAccessibleItem = true;

    // 접근 가능한 인덱스 인지
    public bool IsAccessible { get { return isAccessibleSlot && isAccessibleItem; } }

    // 슬롯의 트랜스폼
    RectTransform slotRect;
    public RectTransform SlotRect { get { return slotRect; } }

    // 아이템 아이콘의 트랜스폼
    RectTransform iconRect;
    public RectTransform IconRect { get { return iconRect; } }

    RectTransform highLightRect;
   
    // ui 인벤토리
    UI_Inventory ui_Inventory;

    private static readonly Color InaccessibleSlotColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
    private static readonly Color InaccessibleIconColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);

    // 아이템 이미지 활성화 여부
    private void SetShowIcon(bool isActive)
    {
        Get<GameObject>((int)GameObjects.ItemImage).SetActive(isActive);
    }

    // 아이템 수량 텍스트 활성화 여부
    private void SetShowText(bool isActive)
    {
        Get<GameObject>((int)GameObjects.AmountText).SetActive(isActive);
    }

    // 현재 아이템 슬롯 설정
    public void SetSlotIndex(int _index)
    {
        Index = _index;
    }

    // 초기화
    public override void Init()
    {
        TryGetComponent(out ItemSlotImage);
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));
        slotRect = transform as RectTransform;
        iconRect = Get<GameObject>((int)GameObjects.ItemImage).transform as RectTransform;
        highLightRect = Get<GameObject>((int)GameObjects.HighLightImage).transform as RectTransform;

        Get<GameObject>((int)GameObjects.ItemImage).SetActive(false);
        Get<GameObject>((int)GameObjects.AmountText).SetActive(false);
        Get<GameObject>((int)GameObjects.HighLightImage).SetActive(false);
    }

    // 슬롯 접근 상태
    public void SetSlotAccessibleState(bool value)
    {
        // 중복 처리 지양
        if (isAccessibleSlot == value)
            return;

        // true면 접근 가능
        if(value)
        {
            ItemSlotImage.color = Color.white;
        }
        // false 접근 불가능
        else
        {
            ItemSlotImage.color = InaccessibleSlotColor;

            SetShowIcon(false);
            SetShowText(false);
        }

        // 슬롯 접근 상태 업데이트
        isAccessibleSlot = value;
    }
    
    // 아이템 접근 상태
    public void SetItemAccessibleState(bool value)
    {
        // 중복처리 지양
        if (isAccessibleItem == value)
            return;

        // 접근 가능
        if(value)
        {
            Get<Image>((int)Images.ItemImage).color = Color.white;
            Get<Text>((int)Texts.AmountText).color = Color.white;
        }
        else
        {
            Get<Image>((int)Images.ItemImage).color = InaccessibleIconColor;
            Get<Text>((int)Texts.AmountText).color = InaccessibleIconColor;
        }

        isAccessibleItem = value;
    }

    // 아이템 아이콘 교환
    public void SwapOrMoveItem(UI_InventoryItemSlot other)
    {
        if (other == null)
            return;
        if (other == this)
            return;
        if (!IsAccessible)
            return;
        if (!other.IsAccessible)
            return;

        Sprite temp = Get<Image>((int)Images.ItemImage).sprite;

        // 1. 대상에 아이템이 있는 경우 교환
        if (other.HasItem)
            SetItem(other.Get<Image>((int)Images.ItemImage).sprite);
        else // 2. 없는 경우 내아이템을 지우고 other 오브젝트로 이동
            RemoveItem();

        other.SetItem(temp);
    }

    // 슬롯에 아이템 등록
    public void SetItem(Sprite itemSprite)
    {
        if(itemSprite != null)
        {
            Get<Image>((int)Images.ItemImage).sprite = itemSprite;
            SetShowIcon(true);
        }
        else
        {
            RemoveItem();
        }
    }

    // 슬롯에서 아이템 제거
    public void RemoveItem()
    {
        Get<Image>((int)Images.ItemImage).sprite = null;
        SetShowIcon(false);
        SetShowText(false);
    }

    // 아이템 이미지 투명도 설정
    public void SetIconAlpha(float alpha)
    {
        Color color = Get<Image>((int)Images.ItemImage).color;
        Get<Image>((int)Images.ItemImage).color = new Color(color.r, color.b, color.a, alpha);
    }

    // 아이템 존재하고 1개 초과일 경우 텍스쳐 활성화 이하일 경우 비활성화
    public void SetItemAmount(int amount)
    {
        if (HasItem && amount > 1)
            SetShowText(true);
        else
            SetShowText(false);

        Get<Text>((int)Texts.AmountText).text = amount.ToString();
    }

    public void HighLight(bool show)
    {
        if (show)
            StartCoroutine(nameof(HighlightFadeInRoutine));
        else
            StartCoroutine(nameof(HighlightFadeOutRoutine));
    }

    public void SetHighLightOnTop(bool value)
    {
        if (value)
            highLightRect.SetAsLastSibling();
        else
            highLightRect.SetAsFirstSibling();
    }

    IEnumerator HighlightFadeInRoutine()
    {
        StopCoroutine(nameof(HighlightFadeOutRoutine));

        GameObject highLightObject = Get<GameObject>((int)GameObjects.HighLightImage);
        highLightObject.SetActive(true);

        Image highLightImage = Get<Image>((int)Images.HighLightImage);

        float unit = highLightAlpha / highLightFadeDuration;

        Color color = highLightImage.color;
        for (; currentHLAlpha <= highLightAlpha; currentHLAlpha += unit *Time.deltaTime)
        {
            color.a = currentHLAlpha;
            highLightImage.color = color;
            yield return null;
        }
    }

    IEnumerator HighlightFadeOutRoutine()
    {
        StopCoroutine(nameof(HighlightFadeInRoutine));

        GameObject highLightObject = Get<GameObject>((int)GameObjects.HighLightImage);


        Image highLightImage = Get<Image>((int)Images.HighLightImage);

        float unit = highLightAlpha / highLightFadeDuration;

        Color color = highLightImage.color;
        for (; currentHLAlpha >= 0; currentHLAlpha -= unit * Time.deltaTime)
        {
            color.a = currentHLAlpha;
            highLightImage.color = color;

            yield return null;
        }

        highLightObject.SetActive(false);
    }
}
