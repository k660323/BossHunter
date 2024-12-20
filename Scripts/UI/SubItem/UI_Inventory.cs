using Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_Inventory : UI_Base
{
    enum GameObjects
    {
        Content,
        ScrollViewGroup
    }

    enum Buttons
    {
        CloseButton,
        CoinDropButton
    }

    enum Texts
    {
        CoinText
    }

    enum Scrollbars
    {
        ScrollbarVertical
    }

    [HideInInspector]
    public Inventory inventory;

    #region 레이아웃 설정
    // 오브젝트 레이아웃
    private GridLayoutGroup _contentLayout;

    // 가소 세로 아이템 슬롯 개수
    [SerializeField, Range(1, 10)]
    private int _horizontalCount = 5;
    [SerializeField, Range(1, 10)]
    private int _verticalCount = 8;

    // 슬롯 외부의 여백
    [SerializeField]
    private int _contentAreaPadding = 10;

    // 한 슬롯의 여백
    [SerializeField]
    private float _slotSpaceing = 15.0f;

    // 각 슬롯의 크기
    [SerializeField]
    private float _slotSize = 100.0f;

    #endregion

    // 아이템 슬롯이 생성될 위치
    private RectTransform _contentAreaRt;

    // 생성 아이템 슬롯을 관리하는 리스트
    List<UI_InventoryItemSlot> _slotUIList;

    // UI Raycast하는 변수
    GraphicRaycaster _gr;
    PointerEventData _ped;
    List<RaycastResult> _rrList;

    // 드래그 시작시 아이템 슬롯
    UI_InventoryItemSlot _beginDragSlot;
    // 드래그 시작시 아이템 아이콘의 transform
    Transform _beginDragIconTransform;

    // 드래그 시작시 아이템 아이콘 위치
    Vector2 _beginDragIconPos;
    // 드래그 시작시 커서 위치
    Vector2 _beginDragCursorPoint;

    // content의 몇번째 자식인지
    int _beginDragIconSiblingIndex;

    // 현재 포인터가 위치한 곳의 슬롯
    UI_InventoryItemSlot _pointerOverSlot;

    // 생성될 아이템 슬롯 이름
    [Header("필수 기입"), SerializeField]
    private string _slotPrefabName;
    [Header("필수 기입"), SerializeField]
    UI_ItemTooltip _itemTooltip;

    InputAction leftShiftAction;
    bool IsLeftShiftDown;

    // 오브젝트 서치 및 저장, 캐싱
    public override void Init()
    {
        Bind<GameObject>(typeof(GameObjects));
        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));
        Bind<Scrollbar>(typeof(Scrollbars));

        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("FX/ClickBack", Define.Sound2D.Effect2D);
            gameObject.SetActive(false);
        });

        Get<Button>((int)Buttons.CoinDropButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("FX/Click", Define.Sound2D.Effect2D);
            UI_CoinDropPopup popup = Managers.UI.ShowPopupUI<UI_CoinDropPopup>();
            popup.SetCoinInputPopup(amt => { Managers.Sound.Play2D("FX/CoinsDrop", Define.Sound2D.Effect2D); TrySeparateCoinRemove(amt); }, inventory.Coin);
        });

        GameObject content = Get<GameObject>((int)GameObjects.Content);
        if (content.TryGetComponent(out _contentLayout))
        {
            // 상하좌우 패딩 설정
            _contentLayout.padding.top = _contentAreaPadding;
            _contentLayout.padding.bottom = _contentAreaPadding;
            _contentLayout.padding.left = _contentAreaPadding;
            _contentLayout.padding.right = _contentAreaPadding;

            // 슬롯끼리 여백 설정
            _contentLayout.spacing.Set(_slotSpaceing, _slotSpaceing);

            // 슬롯 크기 지정
            _contentLayout.cellSize.Set(_slotSize, _slotSize);

            // x축당 오브젝트 개수 제한
            _contentLayout.constraintCount = _horizontalCount;
        }

        // 아이템 슬롯이 생성되는 Translate
        content.TryGetComponent(out _contentAreaRt);

        // UI Raycast하는 변수
        _gr = GetComponentInParent<GraphicRaycaster>();
        if(_gr == null)
            _gr = gameObject.AddComponent<GraphicRaycaster>();
        _ped = new PointerEventData(EventSystem.current);
        _rrList = new List<RaycastResult>(5);

        leftShiftAction = new InputAction(binding: "<Keyboard>/leftShift");
        leftShiftAction.performed += ctx => IsLeftShiftDown = true;
        leftShiftAction.canceled += ctx => IsLeftShiftDown = false;
     
        InitSlots();
    }

    private void OnEnable()
    {
        leftShiftAction?.Enable();
    }

    private void OnDisable()
    {
        leftShiftAction?.Disable();
    }

    // 지정된 개수만큼 슬롯 영역내에 슬롯들 동적 생성
    void InitSlots()
    {
        _slotUIList = new List<UI_InventoryItemSlot>(_verticalCount * _horizontalCount);
        int slotIndex = 0;
        for (int y = 0; y < _verticalCount; y++)
        {
            for (int x = 0; x < _horizontalCount; x++)
            {
                var slotRt = CloneSlot();
                slotRt.gameObject.name = $"Item Slot {slotIndex}";

                if (slotRt.TryGetComponent(out UI_InventoryItemSlot slotUI) == false)
                    return;
                slotUI.SetSlotIndex(slotIndex);
                _slotUIList.Add(slotUI);

                slotIndex++;
            }
        }

        RectTransform CloneSlot()
        {
            GameObject slotGo = Managers.Resource.Instantiate(_slotPrefabName, null);
            if (slotGo.TryGetComponent(out RectTransform tr) == false)
            {
                Destroy(slotGo);
                return null;
            }

            tr.SetParent(_contentAreaRt, false);

            return tr;
        }
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
        // 이전 프레임 슬롯
        var prevSlot = _pointerOverSlot;
        
        // 현재 프레임 슬롯
        var curSlot = _pointerOverSlot = RaycastAndGetFirstComponent<UI_InventoryItemSlot>();

        if (prevSlot == null)
        {
            if(curSlot != null)
            {
                OnCurrentEnter();
            }
        }
        else
        {
            if(curSlot == null)
            {
                OnPrevExit();
            }
            else if (prevSlot != curSlot)
            {
                OnPrevExit();
                OnCurrentEnter();
            }
        }

        void OnCurrentEnter()
        {
            curSlot.HighLight(true);
        }

        void OnPrevExit()
        {
            prevSlot.HighLight(false);
        }
    }

    void ShowOrHideItemTooltip()
    {
        // 마우스가 유효한 아이템 아이콘 위에 올라와 있다면 툴팁 보여주기
        bool isValid =
            _pointerOverSlot != null && _pointerOverSlot.HasItem && _pointerOverSlot.IsAccessible
            && (_pointerOverSlot != _beginDragSlot); // 드래그 시작한 슬롯이면 보여주지 않기

        if(isValid)
        {
            UpdateTooltipUI(_pointerOverSlot);
            _itemTooltip.SetShow(true);
        }
        else
        {
            _itemTooltip.SetShow(false);
        }
    }

    void UpdateTooltipUI(UI_InventoryItemSlot slot)
    {
        if (!slot.IsAccessible || !slot.HasItem)
            return;

        // 툴팁 정보 갱신
        _itemTooltip.SetItemInfo(inventory.GetItemData(slot.Index));

        // 툴팁 위치 조정
        _itemTooltip.SetRectPosition(slot.SlotRect);
    }

    void OnPointerDown()
    {
        // 좌클릭 했을시
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            _beginDragSlot = RaycastAndGetFirstComponent<UI_InventoryItemSlot>();

            // 슬롯이 존재하고 && 아이템을 가지고 있는 슬롯만
            if(_beginDragSlot != null && _beginDragSlot.HasItem && _beginDragSlot.IsAccessible)
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
                GameObject viewPort = Get<GameObject>((int)GameObjects.ScrollViewGroup);
                _beginDragIconTransform.SetParent(viewPort.transform);
                _beginDragIconTransform.transform.SetAsLastSibling();

                // 해당 슬롯의 하이라이트 이미지를 아이콘보다 뒤에 위치시키기
                _beginDragSlot.SetHighLightOnTop(false);
              
            }
            else
            {
                _beginDragSlot = null;
            }
        }
        else if(Mouse.current.rightButton.wasPressedThisFrame)
        {
            UI_InventoryItemSlot slot = RaycastAndGetFirstComponent<UI_InventoryItemSlot>();

            if(slot != null && slot.HasItem && slot.IsAccessible)
            {
                TryUseItem(slot.Index);
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
        if(Mouse.current.leftButton.wasReleasedThisFrame)
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
        if(endDragSlot != null && endDragSlot.IsAccessible)
        {
            // 수량 나누기 조건
            // 1) 마우스 클릭 떼는 순간 좌측 Ctrl 또는 Shift키 유지
            // 2) begin : 셀 수 있는 아이템 / end : 비어있는 슬롯
            // 3) begin 아이템의 수량 > 1

            bool isSeparatable =
                (IsLeftShiftDown &&
                (inventory.IsCountableItem(_beginDragSlot.Index) && !inventory.HasItem(endDragSlot.Index)));

            // true : 수량 나누기, false : 교환 또는 이동
            bool isSeparation = false;
            int currentAmount = 0;

            // 나눌수 있다.
            if(isSeparatable)
            {
                // 현재 개수 확인
                currentAmount = inventory.GetCurrentAmount(_beginDragSlot.Index);

                // 나눌 개수가 된다.
                if (currentAmount > 1)
                    isSeparation = true;
            }

            // 1. 개수 나누기
            if (isSeparation)
                TrySeparateAmount(_beginDragSlot.Index, endDragSlot.Index, currentAmount);
            // 2. 교환 또는 이동
            else
                TrySwapItems(_beginDragSlot, endDragSlot);

            // 툴팁 갱신
            UpdateTooltipUI(endDragSlot);
        }

        // 버리기 (커서가 UI 레이캐스트 타겟 위에 있지 않은 경우)
        if(!IsOverUI())
        {
            int index = _beginDragSlot.Index;
            Data.ItemData data = inventory.GetItemData(index);
            string itemName = inventory.GetItemName(index);
            int amount = inventory.GetCurrentAmount(index);

            // 셀수 있는 아이템의 경우, 수량 표시
            if (amount > 1)
                itemName += $" x{amount}";
         
            // 확인 팝업 띄우고 콜백위임
            if(data._itemType == Define.ItemType.Countable)
            {
                UI_AmountInputItemPopup popup = Managers.UI.ShowPopupUI<UI_AmountInputItemPopup>();
                popup.SetAmountInputItemPopup(amt => TrySeparateAmountRemoveItem(index, amt), amount, itemName, true);
            }
            else
            {
                UI_ConfirmItemPopup popup = Managers.UI.ShowPopupUI<UI_ConfirmItemPopup>();
                popup.SetConfirmItemPopup(() => TryRemoveItem(index), itemName);
            }
        }
        // 다른 UI위에 있다면
        else
        {
            // 추후 개발
        }
        
        return;
    }

    private void TryRemoveItem(int index)
    {
        inventory.Remove(index);
    }

    private void TrySeparateAmountRemoveItem(int index, int amount)
    {
        inventory.SeparateAmountRemove(index, amount);
    }

    private void TrySeparateCoinRemove(long removeCoin)
    {
        inventory.SeparateCoinRemove(removeCoin);
    }

    // 셀 수 있는 아이템 개수 나누기
    void TrySeparateAmount(int indexA, int indexB, int amount)
    {
        if (indexA == indexB)
            return;

        string itemName = $"{inventory.GetItemName(indexA)} x{amount}";

        UI_AmountInputItemPopup popup = Managers.UI.ShowPopupUI<UI_AmountInputItemPopup>();
        popup.SetAmountInputItemPopup(amt => inventory.SeparateAmount(indexA, indexB, amt), amount, itemName, false);
    }

    /// <summary> 아이템 사용 </summary>
    private void TryUseItem(int index)
    {
        ItemData item = inventory.GetItemData(index);
        if (item != null)
        {
            if(item._itemSubType == Define.ItemSubType.Consumption)
                Managers.Sound.Play2D("FX/PotionDrink", Define.Sound2D.Effect2D);
            else
                Managers.Sound.Play2D("FX/ItemEquip", Define.Sound2D.Effect2D);
        }
       
        inventory.Use(index);
    }

    void TrySwapItems(UI_InventoryItemSlot from, UI_InventoryItemSlot to)
    {
        // 같은 컴포넌트일 경우 스킵
        if (from == to)
            return;

        // from.SwapOrMoveItem(to);
        inventory.Swap(from.Index, to .Index);
    }

    /// <summary> 슬롯에 아이템 아이콘 등록 </summary>
    public void SetItemIcon(int index, string iconPath)
    {
        Sprite sprite = Managers.Resource.Load<Sprite>(iconPath);
        _slotUIList[index].SetItem(sprite);
    }


    /// <summary> 해당 슬롯의 아이템 개수 텍스트 지정 </summary>
    public void SetItemAmountText(int index, int amount)
    {
        // NOTE : amount가 1 이하일 경우 텍스트 미표시
        _slotUIList[index].SetItemAmount(amount);
    }

    public void SetCoinText(long coin)
    {
        Get<Text>((int)Texts.CoinText).text = coin.ToString();
    }

    /// <summary> 해당 슬롯의 아이템 개수 텍스트 지정 </summary>
    public void HideItemAmountText(int index)
    {
        _slotUIList[index].SetItemAmount(1);
    }

    /// <summary> 슬롯에서 아이템 아이콘 제거, 개수 텍스트 숨기기 </summary>
    public void RemoveItem(int index)
    {
        _slotUIList[index].RemoveItem();
    }

    /// <summary> 접근 가능한 슬롯 범위 설정 </summary>
    public void SetAccessibleSlotRange(int accessibleSlotCount)
    {
        for (int i = 0; i < _slotUIList.Count; i++)
        {
            _slotUIList[i].SetSlotAccessibleState(i < accessibleSlotCount);
        }
    }

    bool IsOverUI()
    {
        return Managers.Input.IsOverUI;
    }
}
