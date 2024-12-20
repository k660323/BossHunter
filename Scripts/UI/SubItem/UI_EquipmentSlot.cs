using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_EquipmentSlot : UI_Base
{
    [SerializeField]
    private Define.ItemSubType type;

    public Define.ItemSubType EquipType {  get { return type; } }

    enum Images
    {
        ItemImage
    }

    enum GameObjects
    {
        ItemImage,
        PreItemImage
    }

    // ������ �̹����� �����ϸ� �������� �����Ѵ�.
    public bool HasItem { get { return Get<Image>((int)Images.ItemImage).sprite != null; } }

    // ������ Ʈ������
    RectTransform slotRect;
    public RectTransform SlotRect { get { return slotRect; } }

    // ������ �������� Ʈ������
    RectTransform iconRect;
    public RectTransform IconRect { get { return iconRect; } }

    // �κ��丮�� ���° ������ ���������� Ȯ���ϴ� �ε���
    [SerializeField]
    private int index;
    public int Index { get { return index; } private set { index = value; } }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));

        slotRect = transform as RectTransform;
        iconRect = Get<GameObject>((int)GameObjects.ItemImage).transform as RectTransform;
        index = (int)type;

        Get<GameObject>((int)GameObjects.ItemImage).SetActive(false);
    }

    // ���Կ� ������ ���
    public void SetItem(Sprite itemSprite)
    {
        if (itemSprite != null)
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
    }

    // ������ �̹��� Ȱ��ȭ ����
    private void SetShowIcon(bool isActive)
    {
        Get<GameObject>((int)GameObjects.PreItemImage).SetActive(!isActive);
        Get<GameObject>((int)GameObjects.ItemImage).SetActive(isActive);
    }
}
