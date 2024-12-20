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

    #region ���̾ƿ� ����
    // ������Ʈ ���̾ƿ�
    private GridLayoutGroup _contentLayout;

    // ���� ���� ������ ���� ����
    [SerializeField, Range(1, 10)]
    private int _horizontalCount = 5;
    [SerializeField, Range(1, 10)]
    private int _verticalCount = 8;

    // ���� �ܺ��� ����
    [SerializeField]
    private int _contentAreaPadding = 10;

    // �� ������ ����
    [SerializeField]
    private float _slotSpaceing = 15.0f;

    // �� ������ ũ��
    [SerializeField]
    private float _slotSize = 100.0f;

    #endregion

    // ������ ������ ������ ��ġ
    private RectTransform _contentAreaRt;

    // ���� ������ ������ �����ϴ� ����Ʈ
    List<UI_InventoryItemSlot> _slotUIList;

    // UI Raycast�ϴ� ����
    GraphicRaycaster _gr;
    PointerEventData _ped;
    List<RaycastResult> _rrList;

    // �巡�� ���۽� ������ ����
    UI_InventoryItemSlot _beginDragSlot;
    // �巡�� ���۽� ������ �������� transform
    Transform _beginDragIconTransform;

    // �巡�� ���۽� ������ ������ ��ġ
    Vector2 _beginDragIconPos;
    // �巡�� ���۽� Ŀ�� ��ġ
    Vector2 _beginDragCursorPoint;

    // content�� ���° �ڽ�����
    int _beginDragIconSiblingIndex;

    // ���� �����Ͱ� ��ġ�� ���� ����
    UI_InventoryItemSlot _pointerOverSlot;

    // ������ ������ ���� �̸�
    [Header("�ʼ� ����"), SerializeField]
    private string _slotPrefabName;
    [Header("�ʼ� ����"), SerializeField]
    UI_ItemTooltip _itemTooltip;

    InputAction leftShiftAction;
    bool IsLeftShiftDown;

    // ������Ʈ ��ġ �� ����, ĳ��
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
            // �����¿� �е� ����
            _contentLayout.padding.top = _contentAreaPadding;
            _contentLayout.padding.bottom = _contentAreaPadding;
            _contentLayout.padding.left = _contentAreaPadding;
            _contentLayout.padding.right = _contentAreaPadding;

            // ���Գ��� ���� ����
            _contentLayout.spacing.Set(_slotSpaceing, _slotSpaceing);

            // ���� ũ�� ����
            _contentLayout.cellSize.Set(_slotSize, _slotSize);

            // x��� ������Ʈ ���� ����
            _contentLayout.constraintCount = _horizontalCount;
        }

        // ������ ������ �����Ǵ� Translate
        content.TryGetComponent(out _contentAreaRt);

        // UI Raycast�ϴ� ����
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

    // ������ ������ŭ ���� �������� ���Ե� ���� ����
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
        var prevSlot = _pointerOverSlot;
        
        // ���� ������ ����
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
        // ���콺�� ��ȿ�� ������ ������ ���� �ö�� �ִٸ� ���� �����ֱ�
        bool isValid =
            _pointerOverSlot != null && _pointerOverSlot.HasItem && _pointerOverSlot.IsAccessible
            && (_pointerOverSlot != _beginDragSlot); // �巡�� ������ �����̸� �������� �ʱ�

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

        // ���� ���� ����
        _itemTooltip.SetItemInfo(inventory.GetItemData(slot.Index));

        // ���� ��ġ ����
        _itemTooltip.SetRectPosition(slot.SlotRect);
    }

    void OnPointerDown()
    {
        // ��Ŭ�� ������
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            _beginDragSlot = RaycastAndGetFirstComponent<UI_InventoryItemSlot>();

            // ������ �����ϰ� && �������� ������ �ִ� ���Ը�
            if(_beginDragSlot != null && _beginDragSlot.HasItem && _beginDragSlot.IsAccessible)
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
                GameObject viewPort = Get<GameObject>((int)GameObjects.ScrollViewGroup);
                _beginDragIconTransform.SetParent(viewPort.transform);
                _beginDragIconTransform.transform.SetAsLastSibling();

                // �ش� ������ ���̶���Ʈ �̹����� �����ܺ��� �ڿ� ��ġ��Ű��
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
        if(Mouse.current.leftButton.wasReleasedThisFrame)
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
        if(endDragSlot != null && endDragSlot.IsAccessible)
        {
            // ���� ������ ����
            // 1) ���콺 Ŭ�� ���� ���� ���� Ctrl �Ǵ� ShiftŰ ����
            // 2) begin : �� �� �ִ� ������ / end : ����ִ� ����
            // 3) begin �������� ���� > 1

            bool isSeparatable =
                (IsLeftShiftDown &&
                (inventory.IsCountableItem(_beginDragSlot.Index) && !inventory.HasItem(endDragSlot.Index)));

            // true : ���� ������, false : ��ȯ �Ǵ� �̵�
            bool isSeparation = false;
            int currentAmount = 0;

            // ������ �ִ�.
            if(isSeparatable)
            {
                // ���� ���� Ȯ��
                currentAmount = inventory.GetCurrentAmount(_beginDragSlot.Index);

                // ���� ������ �ȴ�.
                if (currentAmount > 1)
                    isSeparation = true;
            }

            // 1. ���� ������
            if (isSeparation)
                TrySeparateAmount(_beginDragSlot.Index, endDragSlot.Index, currentAmount);
            // 2. ��ȯ �Ǵ� �̵�
            else
                TrySwapItems(_beginDragSlot, endDragSlot);

            // ���� ����
            UpdateTooltipUI(endDragSlot);
        }

        // ������ (Ŀ���� UI ����ĳ��Ʈ Ÿ�� ���� ���� ���� ���)
        if(!IsOverUI())
        {
            int index = _beginDragSlot.Index;
            Data.ItemData data = inventory.GetItemData(index);
            string itemName = inventory.GetItemName(index);
            int amount = inventory.GetCurrentAmount(index);

            // ���� �ִ� �������� ���, ���� ǥ��
            if (amount > 1)
                itemName += $" x{amount}";
         
            // Ȯ�� �˾� ���� �ݹ�����
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
        // �ٸ� UI���� �ִٸ�
        else
        {
            // ���� ����
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

    // �� �� �ִ� ������ ���� ������
    void TrySeparateAmount(int indexA, int indexB, int amount)
    {
        if (indexA == indexB)
            return;

        string itemName = $"{inventory.GetItemName(indexA)} x{amount}";

        UI_AmountInputItemPopup popup = Managers.UI.ShowPopupUI<UI_AmountInputItemPopup>();
        popup.SetAmountInputItemPopup(amt => inventory.SeparateAmount(indexA, indexB, amt), amount, itemName, false);
    }

    /// <summary> ������ ��� </summary>
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
        // ���� ������Ʈ�� ��� ��ŵ
        if (from == to)
            return;

        // from.SwapOrMoveItem(to);
        inventory.Swap(from.Index, to .Index);
    }

    /// <summary> ���Կ� ������ ������ ��� </summary>
    public void SetItemIcon(int index, string iconPath)
    {
        Sprite sprite = Managers.Resource.Load<Sprite>(iconPath);
        _slotUIList[index].SetItem(sprite);
    }


    /// <summary> �ش� ������ ������ ���� �ؽ�Ʈ ���� </summary>
    public void SetItemAmountText(int index, int amount)
    {
        // NOTE : amount�� 1 ������ ��� �ؽ�Ʈ ��ǥ��
        _slotUIList[index].SetItemAmount(amount);
    }

    public void SetCoinText(long coin)
    {
        Get<Text>((int)Texts.CoinText).text = coin.ToString();
    }

    /// <summary> �ش� ������ ������ ���� �ؽ�Ʈ ���� </summary>
    public void HideItemAmountText(int index)
    {
        _slotUIList[index].SetItemAmount(1);
    }

    /// <summary> ���Կ��� ������ ������ ����, ���� �ؽ�Ʈ ����� </summary>
    public void RemoveItem(int index)
    {
        _slotUIList[index].RemoveItem();
    }

    /// <summary> ���� ������ ���� ���� ���� </summary>
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
