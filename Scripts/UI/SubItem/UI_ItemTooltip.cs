using Data;
using kcp2k;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemTooltip : UI_Base
{
    enum Images
    {
        ItemImage
    }

    enum Texts
    {
        ItemNameText,
        ItemSubTypeText,
        ItemEquipLevelText,
        ItemGradeText,
        ItemToolTipText
    }

    RectTransform _rt;
    CanvasScaler _canvasScaler;

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
        _rt = transform as RectTransform;
        _rt.pivot = new Vector2(0.0f, 1.0f); // Left Top
        _canvasScaler = GetComponentInParent<CanvasScaler>();

        // DisableAllChildrenRaycastTarget(transform);
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    // 아이템 정보를 toolTip UI에 배치
    public void SetItemInfo(ItemData data)
    {
        if (data == null)
            return;

        Get<Image>((int)Images.ItemImage).sprite = Managers.Resource.Load<Sprite>(data._iconSpritePath);

        Get<Text>((int)Texts.ItemNameText).text = data._name;

        Get<Text>((int)Texts.ItemSubTypeText).text = Util.GetSubTypeToString(data._itemSubType);

        if( data is EquipmentItemData eItemData)
            Get<Text>((int)Texts.ItemEquipLevelText).text = $"Lv : {eItemData._equipmentStat._requiredLevel}";
        else
            Get<Text>((int)Texts.ItemEquipLevelText).text = "";
       
        Get<Text>((int)Texts.ItemGradeText).text = Util.GetGradeToString(data._itemGrade);
        Get<Text>((int)Texts.ItemToolTipText).text = data._tooltip;

        Color color = Util.GetGradeToColor(data._itemGrade);
        Get<Text>((int)Texts.ItemNameText).color = color;
        Get<Text>((int)Texts.ItemGradeText).color = color;
    }

    public void SetRectPosition(RectTransform slotRect)
    {
        // 캔버스 스케일러에 따른 해상도 대응
        float wRatio = Screen.width / _canvasScaler.referenceResolution.x;
        float hRatio = Screen.height / _canvasScaler.referenceResolution.y;

        float ratio =
               wRatio * (1f - _canvasScaler.matchWidthOrHeight) +
               hRatio * (_canvasScaler.matchWidthOrHeight);

        // 툴팁 초기 위치(슬롯 우하단) 설정
        _rt.position = slotRect.position;
        Vector2 pos = _rt.position;

        // 툴팁의 크기
        float width = _rt.rect.width * ratio;
        float height = _rt.rect.height * ratio;

        // 우측, 하단이 잘렸는지 여부
        bool rightTruncated = pos.x + width > Screen.width;
        bool bottomTruncated = pos.y - height < 0;

        ref bool R = ref rightTruncated;
        ref bool B = ref bottomTruncated;

        // 오른쪽만 잘림 => 슬롯의 Left Bottom 방향으로 표시
        if (R && !B)
        {
            _rt.position = new Vector2(pos.x - width, pos.y);
        }
        // 아래쪽만 잘림 => 슬롯의 Right Top 방향으로 표시
        else if (!R && B)
        {
            _rt.position = new Vector2(pos.x, pos.y + height);
        }
        // 모두 잘림 => 슬롯의 Left Top 방향으로 표시
        else if (R && B)
        {
            _rt.position = new Vector2(pos.x - width, pos.y + height);
        }
        // 잘리지 않음 => 슬롯의 Right Bottom 방향으로 표시
        // Do Nothing
    }

    public void SetShow(bool show)
    {
        gameObject.SetActive(show);
    }
  
}
