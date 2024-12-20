using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_InventoryItemSlot : UI_Base
{
    enum Images
    {
        ItemImage,     // ������ ������ �̹���
        HighLightImage // ���̶���Ʈ �̹���
    }

    enum Texts
    {
        AmountText,    // ������ ����     
    }

    enum GameObjects
    {
        ItemImage,
        AmountText,
        HighLightImage
    }

    // ������ ���� �̹���
    Image ItemSlotImage;

    // ���̶���Ʈ ���İ�
    [SerializeField]
    private const float highLightAlpha = 0.5f;

    // �� ���̶���Ʈ <-> ���̶���Ʈ ���� �ɸ��� �ð�
    [SerializeField]
    private const float highLightFadeDuration = 0.2f;

    // ���� ���̶���Ʈ ���İ�
    private float currentHLAlpha = 0.0f;

    // �κ��丮�� ���° ������ ���������� Ȯ���ϴ� �ε���
    [SerializeField]
    private int index;
    public int Index { get { return index; } private set { index = value; } }

    // ������ �̹����� �����ϸ� �������� �����Ѵ�.
    public bool HasItem { get { return Get<Image>((int)Images.ItemImage).sprite != null; } }

    // ���� ���� ����
    bool isAccessibleSlot = true;
    // ������ ���� ����
    bool isAccessibleItem = true;

    // ���� ������ �ε��� ����
    public bool IsAccessible { get { return isAccessibleSlot && isAccessibleItem; } }

    // ������ Ʈ������
    RectTransform slotRect;
    public RectTransform SlotRect { get { return slotRect; } }

    // ������ �������� Ʈ������
    RectTransform iconRect;
    public RectTransform IconRect { get { return iconRect; } }

    RectTransform highLightRect;
   
    // ui �κ��丮
    UI_Inventory ui_Inventory;

    private static readonly Color InaccessibleSlotColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
    private static readonly Color InaccessibleIconColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);

    // ������ �̹��� Ȱ��ȭ ����
    private void SetShowIcon(bool isActive)
    {
        Get<GameObject>((int)GameObjects.ItemImage).SetActive(isActive);
    }

    // ������ ���� �ؽ�Ʈ Ȱ��ȭ ����
    private void SetShowText(bool isActive)
    {
        Get<GameObject>((int)GameObjects.AmountText).SetActive(isActive);
    }

    // ���� ������ ���� ����
    public void SetSlotIndex(int _index)
    {
        Index = _index;
    }

    // �ʱ�ȭ
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

    // ���� ���� ����
    public void SetSlotAccessibleState(bool value)
    {
        // �ߺ� ó�� ����
        if (isAccessibleSlot == value)
            return;

        // true�� ���� ����
        if(value)
        {
            ItemSlotImage.color = Color.white;
        }
        // false ���� �Ұ���
        else
        {
            ItemSlotImage.color = InaccessibleSlotColor;

            SetShowIcon(false);
            SetShowText(false);
        }

        // ���� ���� ���� ������Ʈ
        isAccessibleSlot = value;
    }
    
    // ������ ���� ����
    public void SetItemAccessibleState(bool value)
    {
        // �ߺ�ó�� ����
        if (isAccessibleItem == value)
            return;

        // ���� ����
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

    // ������ ������ ��ȯ
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

        // 1. ��� �������� �ִ� ��� ��ȯ
        if (other.HasItem)
            SetItem(other.Get<Image>((int)Images.ItemImage).sprite);
        else // 2. ���� ��� ���������� ����� other ������Ʈ�� �̵�
            RemoveItem();

        other.SetItem(temp);
    }

    // ���Կ� ������ ���
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

    // ���Կ��� ������ ����
    public void RemoveItem()
    {
        Get<Image>((int)Images.ItemImage).sprite = null;
        SetShowIcon(false);
        SetShowText(false);
    }

    // ������ �̹��� ���� ����
    public void SetIconAlpha(float alpha)
    {
        Color color = Get<Image>((int)Images.ItemImage).color;
        Get<Image>((int)Images.ItemImage).color = new Color(color.r, color.b, color.a, alpha);
    }

    // ������ �����ϰ� 1�� �ʰ��� ��� �ؽ��� Ȱ��ȭ ������ ��� ��Ȱ��ȭ
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
