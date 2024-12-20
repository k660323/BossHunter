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

    // 아이템 이미지가 존재하면 아이템이 존재한다.
    public bool HasItem { get { return Get<Image>((int)Images.ItemImage).sprite != null; } }

    // 슬롯의 트랜스폼
    RectTransform slotRect;
    public RectTransform SlotRect { get { return slotRect; } }

    // 아이템 아이콘의 트랜스폼
    RectTransform iconRect;
    public RectTransform IconRect { get { return iconRect; } }

    // 인벤토리에 몇번째 생성된 아이템인지 확인하는 인덱스
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

    // 슬롯에 아이템 등록
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

    // 슬롯에서 아이템 제거
    public void RemoveItem()
    {
        Get<Image>((int)Images.ItemImage).sprite = null;
        SetShowIcon(false);
    }

    // 아이템 이미지 활성화 여부
    private void SetShowIcon(bool isActive)
    {
        Get<GameObject>((int)GameObjects.PreItemImage).SetActive(!isActive);
        Get<GameObject>((int)GameObjects.ItemImage).SetActive(isActive);
    }
}
