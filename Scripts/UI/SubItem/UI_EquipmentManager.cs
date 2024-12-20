using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_EquipmentManager : UI_Base
{
    enum Buttons
    {
        CloseButton
    }

    enum UI_Sockets
    {
        None = 0,
        MeleeWeapon = 1,
        RangeWeapon = 2,
        TargetWeapon = 3,
        Helmet = 4,
        Shoulder = 5,
        Top = 6,
        Pants = 7,
        Belt = 8,
        Shoes = 9,
        Ring = 10,
        Necklace = 11,
        Bracelet = 12
    }

    enum GameObjects
    {
        Content
    }

    [HideInInspector]
    public EquipmentManager equipmentManager;

    // UI Raycast�ϴ� ����
    GraphicRaycaster _gr;
    PointerEventData _ped;
    List<RaycastResult> _rrList;

    // �巡�� ���۽� ������ ����
    UI_EquipmentSlot _beginDragSlot;

    // �巡�� ���۽� ������ �������� transform
    Transform _beginDragIconTransform;

    // �巡�� ���۽� ������ ������ ��ġ
    Vector2 _beginDragIconPos;
    // �巡�� ���۽� Ŀ�� ��ġ
    Vector2 _beginDragCursorPoint;

    // content�� ���° �ڽ�����
    int _beginDragIconSiblingIndex;

    // ���� �����Ͱ� ��ġ�� ���� ����
    UI_EquipmentSlot _pointerOverSlot;

    [Header("�ʼ� ����"), SerializeField]
    UI_ItemTooltip _itemTooltip;

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<UI_EquipmentSlot>(typeof(UI_Sockets));
        Bind<GameObject>(typeof(GameObjects));

        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);
            gameObject.SetActive(false);
        });

        // UI Raycast�ϴ� ����
        _gr = GetComponentInParent<GraphicRaycaster>();
        if (_gr == null)
            _gr = gameObject.AddComponent<GraphicRaycaster>();
        _ped = new PointerEventData(EventSystem.current);
        _rrList = new List<RaycastResult>(5);
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        _ped.position = Mouse.current.position.ReadValue();
        OnPointerEnterAndExit();
        ShowOrHideItemTooltip();
        OnPointerDown();
        OnPointerDrag();
        OnPointerUp();
    }

    T RaycastAndGetFirstComponent<T>() where T : Component
    {
        _rrList.Clear();

        // GraphicRaycaster _gr��� ������, PointerEventData _ped ������ Ray�� ���� �����
        // List<RaycastResult> _rrList; ��� ��ȯ�Ѵ�.
        _gr.Raycast(_ped, _rrList);

        // ����� ������ ����
        if (_rrList.Count == 0)
            return null;

        if (_rrList[0].gameObject.TryGetComponent(out T result))
            return result;

        return null;
    }

    void OnPointerEnterAndExit()
    {
        // ���� ������ ����
        _pointerOverSlot = RaycastAndGetFirstComponent<UI_EquipmentSlot>();
    }

    void ShowOrHideItemTooltip()
    {
        // ���콺�� ��ȿ�� ������ ������ ���� �ö�� �ִٸ� ���� �����ֱ�
        bool isValid =
            _pointerOverSlot != null && _pointerOverSlot.HasItem
            && (_pointerOverSlot != _beginDragSlot); // �巡�� ������ �����̸� �������� �ʱ�

        if (isValid)
        {
            UpdateTooltipUI(_pointerOverSlot);
            _itemTooltip.SetShow(true);
        }
        else
        {
            _itemTooltip.SetShow(false);
        }
    }

    void UpdateTooltipUI(UI_EquipmentSlot slot)
    {
        if (!slot.HasItem)
            return;

        // ���� ���� ����
        _itemTooltip.SetItemInfo(equipmentManager.GetItemData(slot.EquipType));

        // ���� ��ġ ����
        _itemTooltip.SetRectPosition(slot.SlotRect);
    }

    void OnPointerDown()
    {
        // ��Ŭ�� ������
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _beginDragSlot = RaycastAndGetFirstComponent<UI_EquipmentSlot>();

            // ������ �����ϰ� && �������� ������ �ִ� ���Ը�
            if (_beginDragSlot != null && _beginDragSlot.HasItem)
            {
                Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);

                // ������ ������ transform ����
                _beginDragIconTransform = _beginDragSlot.IconRect.transform;

                // ������ ���� ��ġ ����
                _beginDragIconPos = _beginDragIconTransform.position;

                // ���콺 ��ġ ����
                _beginDragCursorPoint = Mouse.current.position.ReadValue();

                // UI ������ ���̱�
                _beginDragIconSiblingIndex = _beginDragIconTransform.GetSiblingIndex();
                GameObject viewPort = Get<GameObject>((int)GameObjects.Content);
                _beginDragIconTransform.SetParent(viewPort.transform);
                _beginDragIconTransform.transform.SetAsLastSibling();
            }
            else
            {
                _beginDragSlot = null;
            }
        }
        else if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            UI_EquipmentSlot slot = RaycastAndGetFirstComponent<UI_EquipmentSlot>();

            if (slot != null && slot.HasItem)
            {
                // ������ ����
                TryUnEquipItem(slot.EquipType);

            }
        }
    }

    // �巡����
    void OnPointerDrag()
    {
        if (_beginDragSlot == null)
            return;

        if (Mouse.current.leftButton.ReadValue() > 0)
        {
            // ��ġ �̵�
            _beginDragIconTransform.position = _beginDragIconPos + (Mouse.current.position.ReadValue() - _beginDragCursorPoint);
        }
    }

    // Ŭ���� �� ���
    void OnPointerUp()
    {
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (_beginDragSlot == null)
                return;
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);
            // ������ ��ġ ����
            _beginDragIconTransform.position = _beginDragIconPos;

            // UI ���� ����
            _beginDragIconTransform.SetParent(_beginDragSlot.transform);
            _beginDragIconTransform.transform.SetSiblingIndex(_beginDragIconSiblingIndex);

            // �巡�� �Ϸ�
            EndDrag();

            // ���� ����
            _beginDragSlot = null;
            _beginDragIconTransform = null;
        }
    }

    void EndDrag()
    {
        UI_InventoryItemSlot endDragSlot = RaycastAndGetFirstComponent<UI_InventoryItemSlot>();
        if (endDragSlot != null && endDragSlot.IsAccessible)
        {
            // ��ȯ �Ǵ� �̵�
            // TrySwapItems(_beginDragSlot, endDragSlot);

            // ���� ����
            // UpdateTooltipUI(endDragSlot);
        }

        return;
    }

    /// <summary> ������ ��� </summary>
    private void TryUnEquipItem(Define.ItemSubType itemSubType)
    {
        Managers.Sound.Play2D("FX/ItemUnEquip", Define.Sound2D.Effect2D);
        equipmentManager.Use(itemSubType);
    }

    /// <summary> ���Կ� ������ ������ ��� </summary>
    public void SetItemIcon(Define.ItemSubType itemSubType, string iconPath)
    {
        Sprite sprite = Managers.Resource.Load<Sprite>(iconPath);
        Get<UI_EquipmentSlot>((int)itemSubType).SetItem(sprite);
    }

    /// <summary> ���Կ��� ������ ������ ����, ���� �ؽ�Ʈ ����� </summary>
    public void RemoveItem(int index)
    {
        Get<UI_EquipmentSlot>(index).RemoveItem();
    }
}
