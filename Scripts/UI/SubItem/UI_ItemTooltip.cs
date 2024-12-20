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

    // ������ ������ toolTip UI�� ��ġ
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
        // ĵ���� �����Ϸ��� ���� �ػ� ����
        float wRatio = Screen.width / _canvasScaler.referenceResolution.x;
        float hRatio = Screen.height / _canvasScaler.referenceResolution.y;

        float ratio =
               wRatio * (1f - _canvasScaler.matchWidthOrHeight) +
               hRatio * (_canvasScaler.matchWidthOrHeight);

        // ���� �ʱ� ��ġ(���� ���ϴ�) ����
        _rt.position = slotRect.position;
        Vector2 pos = _rt.position;

        // ������ ũ��
        float width = _rt.rect.width * ratio;
        float height = _rt.rect.height * ratio;

        // ����, �ϴ��� �߷ȴ��� ����
        bool rightTruncated = pos.x + width > Screen.width;
        bool bottomTruncated = pos.y - height < 0;

        ref bool R = ref rightTruncated;
        ref bool B = ref bottomTruncated;

        // �����ʸ� �߸� => ������ Left Bottom �������� ǥ��
        if (R && !B)
        {
            _rt.position = new Vector2(pos.x - width, pos.y);
        }
        // �Ʒ��ʸ� �߸� => ������ Right Top �������� ǥ��
        else if (!R && B)
        {
            _rt.position = new Vector2(pos.x, pos.y + height);
        }
        // ��� �߸� => ������ Left Top �������� ǥ��
        else if (R && B)
        {
            _rt.position = new Vector2(pos.x - width, pos.y + height);
        }
        // �߸��� ���� => ������ Right Bottom �������� ǥ��
        // Do Nothing
    }

    public void SetShow(bool show)
    {
        gameObject.SetActive(show);
    }
  
}
