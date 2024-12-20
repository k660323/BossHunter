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

    // UI Raycast하는 변수
    GraphicRaycaster _gr;
    PointerEventData _ped;
    List<RaycastResult> _rrList;

    // 드래그 시작시 아이템 슬롯
    UI_EquipmentSlot _beginDragSlot;

    // 드래그 시작시 아이템 아이콘의 transform
    Transform _beginDragIconTransform;

    // 드래그 시작시 아이템 아이콘 위치
    Vector2 _beginDragIconPos;
    // 드래그 시작시 커서 위치
    Vector2 _beginDragCursorPoint;

    // content의 몇번째 자식인지
    int _beginDragIconSiblingIndex;

    // 현재 포인터가 위치한 곳의 슬롯
    UI_EquipmentSlot _pointerOverSlot;

    [Header("필수 기입"), SerializeField]
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

        // UI Raycast하는 변수
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

        // GraphicRaycaster _gr라는 도구로, PointerEventData _ped 영역에 Ray를 쏴서 결과를
        // List<RaycastResult> _rrList; 담아 반환한다.
        _gr.Raycast(_ped, _rrList);

        // 결과가 없으면 리턴
        if (_rrList.Count == 0)
            return null;

        if (_rrList[0].gameObject.TryGetComponent(out T result))
            return result;

        return null;
    }

    void OnPointerEnterAndExit()
    {
        // 현재 프레임 슬롯
        _pointerOverSlot = RaycastAndGetFirstComponent<UI_EquipmentSlot>();
    }

    void ShowOrHideItemTooltip()
    {
        // 마우스가 유효한 아이템 아이콘 위에 올라와 있다면 툴팁 보여주기
        bool isValid =
            _pointerOverSlot != null && _pointerOverSlot.HasItem
            && (_pointerOverSlot != _beginDragSlot); // 드래그 시작한 슬롯이면 보여주지 않기

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

        // 툴팁 정보 갱신
        _itemTooltip.SetItemInfo(equipmentManager.GetItemData(slot.EquipType));

        // 툴팁 위치 조정
        _itemTooltip.SetRectPosition(slot.SlotRect);
    }

    void OnPointerDown()
    {
        // 좌클릭 했을시
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _beginDragSlot = RaycastAndGetFirstComponent<UI_EquipmentSlot>();

            // 슬롯이 존재하고 && 아이템을 가지고 있는 슬롯만
            if (_beginDragSlot != null && _beginDragSlot.HasItem)
            {
                Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);

                // 아이템 아이콘 transform 저장
                _beginDragIconTransform = _beginDragSlot.IconRect.transform;

                // 아이콘 시작 위치 저장
                _beginDragIconPos = _beginDragIconTransform.position;

                // 마우스 위치 저장
                _beginDragCursorPoint = Mouse.current.position.ReadValue();

                // UI 맨위에 보이기
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
                // 아이템 벗기
                TryUnEquipItem(slot.EquipType);

            }
        }
    }

    // 드래그중
    void OnPointerDrag()
    {
        if (_beginDragSlot == null)
            return;

        if (Mouse.current.leftButton.ReadValue() > 0)
        {
            // 위치 이동
            _beginDragIconTransform.position = _beginDragIconPos + (Mouse.current.position.ReadValue() - _beginDragCursorPoint);
        }
    }

    // 클릭을 땔 경우
    void OnPointerUp()
    {
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (_beginDragSlot == null)
                return;
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);
            // 아이콘 위치 복원
            _beginDragIconTransform.position = _beginDragIconPos;

            // UI 순서 복원
            _beginDragIconTransform.SetParent(_beginDragSlot.transform);
            _beginDragIconTransform.transform.SetSiblingIndex(_beginDragIconSiblingIndex);

            // 드래그 완료
            EndDrag();

            // 참조 제거
            _beginDragSlot = null;
            _beginDragIconTransform = null;
        }
    }

    void EndDrag()
    {
        UI_InventoryItemSlot endDragSlot = RaycastAndGetFirstComponent<UI_InventoryItemSlot>();
        if (endDragSlot != null && endDragSlot.IsAccessible)
        {
            // 교환 또는 이동
            // TrySwapItems(_beginDragSlot, endDragSlot);

            // 툴팁 갱신
            // UpdateTooltipUI(endDragSlot);
        }

        return;
    }

    /// <summary> 아이템 사용 </summary>
    private void TryUnEquipItem(Define.ItemSubType itemSubType)
    {
        Managers.Sound.Play2D("FX/ItemUnEquip", Define.Sound2D.Effect2D);
        equipmentManager.Use(itemSubType);
    }

    /// <summary> 슬롯에 아이템 아이콘 등록 </summary>
    public void SetItemIcon(Define.ItemSubType itemSubType, string iconPath)
    {
        Sprite sprite = Managers.Resource.Load<Sprite>(iconPath);
        Get<UI_EquipmentSlot>((int)itemSubType).SetItem(sprite);
    }

    /// <summary> 슬롯에서 아이템 아이콘 제거, 개수 텍스트 숨기기 </summary>
    public void RemoveItem(int index)
    {
        Get<UI_EquipmentSlot>(index).RemoveItem();
    }
}
