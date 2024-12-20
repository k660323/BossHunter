using Data;
using Mirror;
using System;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    [SyncVar]
    private int capacity;
    // 아이템 수용 한도
    public int Capacity { get { return capacity; } private set { capacity = value; } }

    // 초기 수용 한도 <= 최대 수용한도

    // 초기 수용 한도
    [SerializeField, SyncVar, Range(8, 64)]
    private int _initalCapacity = 32;

    // 최대 수용 한도 (아이템 배열 크기)
    [SerializeField, SyncVar, Range(8, 64)]
    private int _maxCapacity = 64;

    // 연결된 인벤토리 UI
    [SerializeField]
    private UI_Inventory uI_Inventory;

    [SerializeField]
    SyncList<Item> syncLists = new SyncList<Item>();

    [SerializeField, SyncVar(hook = nameof(OnInventroyCoinUpdated))]
    long coin;
    public long Coin { get { return coin; } set { coin = Math.Max(0, value); } }

    Action<long> OnCoinUpdated;

    [ServerCallback]
    private void Awake()
    {
        Capacity = _initalCapacity;
        for(int i=0; i< _maxCapacity; i++)
            syncLists.Add(null);
    }

    public void OnInventroyCoinUpdated(long _old, long _new)
    {
        OnCoinUpdated?.Invoke(_new);
    }

    // 클라이언트 콜백 이벤트
    public void OnInventoryUpdated<T>(SyncList<Item>.Operation op, int index, T oldItem, T newItem) where T : Item
    {
        switch (op)
        {
            case SyncList<Item>.Operation.OP_SET:
                UpdateSlot(index);
                break;
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if(Managers.UI.SceneUI is IPlayerUI playerUI)
        {
            uI_Inventory = playerUI.GetPlayerUI.UI_Inventory;
            uI_Inventory.inventory = this;
            // UI 초기화
            UpdateAccessibleStatesAll();

            // UI 인벤토리 연결될때
            // Process initial SyncList payload
            syncLists.Callback -= OnInventoryUpdated;
            syncLists.Callback += OnInventoryUpdated;
            OnCoinUpdated -= UpdateCoin;
            OnCoinUpdated += UpdateCoin;

            UpdateCoin(coin);
            for (int index = 0; index < syncLists.Count; index++)
                OnInventoryUpdated(SyncList<Item>.Operation.OP_SET, index, new Item(), syncLists[index]);
        }
    }

    // 모든 슬롯 UI에 접근 가능 여부 업데이트
    public void UpdateAccessibleStatesAll()
    {
        uI_Inventory.SetAccessibleSlotRange(Capacity);
    }

    // 접근 가능한 인덱스 인지 확인
    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < Capacity;
    }

    // 앞에서부터 비어 있는 인덱스 탐색 
    private int FindEmptySlotIndex(int startIndex = 0)
    {
        for (int i = startIndex; i < Capacity; i++)
        {
            if (syncLists[i] == null)
                return i;
        }

        // 비어 있는 인덱스가 없으면 -1을 반환
        return -1;
    }

    /// <summary> 앞에서부터 개수 여유가 있는 Countable 아이템의 슬롯 인덱스 탐색 </summary>
    [ServerCallback]
    private int FindCountableItemSlotIndex(CountableItemData target, int startIndex = 0)
    {
        for (int i = startIndex; i < Capacity; i++)
        {
            // var current = items[i];
            var current = syncLists[i];

            if (current == null)
                continue;

            // 아이템 종류 일치, 개수 여유 확인
            if (current.Data == target && current is CountableItem ci)
            {
                if (!ci.IsMax)
                    return i;
            }
        }

        return -1;
    }

    // 해당 인덱스에 아이템이 있는지 확인
    public bool HasItem(int index)
    {
        return IsValidIndex(index) && syncLists[index] != null;
    }

    // 해당 슬롯의 아이템이 셀 수 있는 아이템일 경우
    [ServerCallback]
    public bool IsCountableItem(int index)
    {
        return HasItem(index) && syncLists[index] is CountableItem;
    }

    // 해당 인덱스의 아이템 수 반환
    public int GetCurrentAmount(int index)
    {
        // 유요하지 않은 인덱스 -1 반환
        if (!IsValidIndex(index))
            return -1;

        // 아이템이 존재하지 않음 0 반환
        if (syncLists[index] == null)
            return 0;

        if (Managers.Instance.mode == NetworkManagerMode.Host)
        {
            if (syncLists[index] is CountableItem)
            {
                CountableItem ci = syncLists[index] as CountableItem;
                return ci.Amount;
            }
        }
        else
        {
            if (syncLists[index].Data._itemType == Define.ItemType.Countable)
            {
                return syncLists[index].Amount;
            }
        }

        return 1;
    }

    // 해당 슬롯의 아이템 정보 반환
    public ItemData GetItemData(int index)
    {
        if (!IsValidIndex(index))
            return null;

        if (syncLists[index] == null)
            return null;

        return syncLists[index].Data;
    }

    // 해당 슬롯의 아이템 이름 리턴
    public string GetItemName(int index)
    {
        if (!IsValidIndex(index))
            return "";

        if (syncLists[index] == null)
            return "";

        return syncLists[index].Data._name;
    }

    // 해당 인덱스 슬롯 상태 및 UI 갱신
    [ClientCallback]
    public void UpdateSlot(int index)
    {
        if (!IsValidIndex(index))
            return;

        Item item = syncLists[index];

        // 1. 아이템 슬롯이 존재하는 경우
        if (item != null)
        {
            // 아이콘 등록
            uI_Inventory.SetItemIcon(index, item.Data._iconSpritePath);

            // 셀 수 있는 아이템
            // 소비, 기타템 일 경우
            if (item.Data._itemType == Define.ItemType.Countable)
            {
                // 수량이 0인 경우 아이템 제거
                if (item.Amount <= 0)
                {
                    // 서버에게 제거
                    Remove(index);
                    RemoveIcon();
                    return;
                }
                else
                {
                    uI_Inventory.SetItemAmountText(index, item.Amount);
                }
            }
            else
            {
                uI_Inventory.HideItemAmountText(index);
            }
        }
        // 빈 슬롯일 경우 아이콘 제거
        else
        {
            RemoveIcon();
        }

        void RemoveIcon()
        {
            // 아이템 제거
            uI_Inventory.RemoveItem(index);
            // 수량 텍스트 숨기기
            uI_Inventory.HideItemAmountText(index); 
        }
    }

    [ClientCallback]
    public void UpdateCoin(long coin)
    {
        uI_Inventory.SetCoinText(coin);
    }

    //[ClientCallback]
    //public void UpdateSlot(int index, Item item)
    //{
    //    if (!IsValidIndex(index))
    //        return;

    //    // 1. 아이템 슬롯이 존재하는 경우
    //    if (item != null)
    //    {
    //        // 아이콘 등록
    //        uI_Inventory.SetItemIcon(index, item.Data._iconSprite);

    //        // 셀 수 있는 아이템
    //        if (item.Data._itemType == Define.ItemType.Countable)
    //        {
    //            // 수량이 0인 경우 아이템 제거
    //            if (item.Amount <= 0)
    //            {
    //                // 서버에게 제거
    //                Remove(index);
    //                RemoveIcon();
    //                return;
    //            }
    //            else
    //            {
    //                uI_Inventory.SetItemAmountText(index, item.Amount);
    //            }
    //        }
    //        else
    //        {
    //            Debug.Log("셀수 없는 아이템");
    //            uI_Inventory.HideItemAmountText(index);
    //        }
    //    }
    //    // 빈 슬롯일 경우 아이콘 제거
    //    else
    //    {
    //        RemoveIcon();
    //    }

    //    void RemoveIcon()
    //    {
    //        // 아이템 제거
    //        uI_Inventory.RemoveItem(index);
    //        // 수량 텍스트 숨기기
    //        uI_Inventory.HideItemAmountText(index);
    //    }
    //}

    /// <summary> 해당하는 인덱스의 슬롯들의 상태 및 UI 갱신 </summary>
    private void UpdateSlot(params int[] indices)
    {
        foreach (var i in indices)
        {
            UpdateSlot(i);
        }
    }

    // a -> b
    [Command]
    public void Swap(int indexA, int indexB)
    {
        if (!IsValidIndex(indexA))
            return;
        if (!IsValidIndex(indexB))
            return;

        Item itemA = syncLists[indexA];
        Item itemB = syncLists[indexB];

        // 아이템이 둘다 존재하고, 동일한 아이템이고, 셀 수 있는 아이템일 경우
        if(itemA != null && itemB != null &&
            itemA.Data == itemB.Data &&
            itemA is CountableItem ciA && itemB is CountableItem ciB)
        {
            int maxAmount = ciB.MaxAmount;
            int sum = ciA.Amount + ciB.Amount;

            if (sum <= maxAmount)
            {
                ciA.SetAmount(0);
                ciB.SetAmount(sum);
            }
            else
            {
                ciA.SetAmount(sum - maxAmount);
                ciB.SetAmount(maxAmount);
            }

            syncLists[indexA] = null;
            syncLists[indexB] = null;
            syncLists[indexA] = itemA;
            syncLists[indexB] = itemB;
        }
        // 일반적인 경우 : 슬롯 교체
        else
        {
            syncLists[indexA] = itemB;
            syncLists[indexB] = itemA;
        }
    }

    /// <summary> 셀 수 있는 아이템의 수량 나누기(A -> B 슬롯으로) </summary>
    [ServerCallback]
    public void SeparateAmount(int indexA, int indexB, int amount)
    {
        // amount : 나눌 목표 수량

        if (!IsValidIndex(indexA)) 
            return;
        if (!IsValidIndex(indexB)) 
            return;

        Item _itemA = syncLists[indexA];
        Item _itemB = syncLists[indexB];

        CountableItem _ciA = _itemA as CountableItem;

        // 조건 : A 슬롯 - 셀 수 있는 아이템 / B 슬롯 - Null
        // 조건에 맞는 경우, 복제하여 슬롯 B에 추가
        if (_ciA != null && _itemB == null)
        {
            CountableItem _cloneItem = _ciA.SeperateAndClone(amount);
            if(_cloneItem != null)
            {
                syncLists[indexB] = _cloneItem;
                syncLists[indexA] = null;
                syncLists[indexA] = _ciA;
            }
        }
    }

    [Command]
    public void Use(int index)
    {
        if(!IsValidIndex(index)) 
            return;

        if (syncLists[index] == null)
            return;

        // 사용 가능한 아이템일 경우
        if (syncLists[index] is IUsableItem uItem)
        {
            // 아이템 사용
            bool succeeded = uItem.Use(gameObject);
            if (succeeded)
            {
                Item _itemA = syncLists[index];

                // 아이템 업데이트를 위해 해당 인덱스를 null로 밀어낸다.
                syncLists[index] = null;

                // 셀 수 있는 아이템일 경우
                if (_itemA is CountableItem)
                {
                    CountableItem _cntItem = _itemA as CountableItem;
                    // 비어 있지 않으면 다시 아이템 갱신
                    if (_cntItem.IsEmpty == false)
                        syncLists[index] = _itemA;
                }
            }
        }
        else if (syncLists[index] is IEquipableItem eItem)
        {
            // 아이템 장착
            bool succeeded = eItem.Equip(gameObject);
            if (succeeded)
            {
                // 장착성공시 인벤토리에 아이템을 null로 밀어낸다.
                syncLists[index] = null;
            }
        }
    }

    // 인벤토리에 아이템 추가
    // 성공적으로 추가 하면 0 반환
    // 실패시 나머지 반환
    [ServerCallback]
    public int Add(ItemData itemData, int amount = 1)
    {
        int index;

        // 수량이 있는 아이템
        if(itemData is CountableItemData ciData)
        {
            bool findNextCountable = true;
            index = -1;

            while(amount > 0)
            {
                // 해당 아이템이 인번토리 내에 존재하고, 개수 여유 있는지 검사
                if(findNextCountable)
                {
                    index = FindCountableItemSlotIndex(ciData, index + 1);

                    // 여유 있는 슬롯이 더이상 없으면 빈 슬롯부터 탐색
                    if(index == -1)
                    {
                        findNextCountable = false;
                    }
                    // 기존 아이템을 찾으면 양을 증가시키고 amount에 초기화
                    else
                    {
                        CountableItem ci = syncLists[index] as CountableItem;
                        amount = ci.AddAmountAndGetExcess(amount);
                        syncLists[index] = null;
                        syncLists[index] = ci;
                    }
                }
                // 1-2. 빈 슬롯 탐색
                else
                {
                    index = FindEmptySlotIndex(index + 1);

                    // 빈 슬롯이 존재하지 않을시 종료
                    if (index == -1)
                        break;
                    // 빈 슬롯 발견 시, 슬롯에 아이템 추가 및 잉여량 계산
                    else
                    {
                        // 새로운 아이템 생성
                        CountableItem ci = ciData.CreateItem() as CountableItem;
                        ci.SetAmount(amount);

                        // 슬롯에 추가
                        syncLists[index] = ci;

                        // 남은 개수 계산
                        amount = (amount > ciData._maxAmount) ? (amount - ciData._maxAmount) : 0;
                    }
                }
            }
        }
        // 수량이 없는 아이템
        else
        {
            // 1개만 넣는 경우, 간단히 수행
            if(amount == 1)
            {
                index = FindEmptySlotIndex();
                if(index != -1)
                {
                    // 아이템을 생성하여 슬롯에 추가
                    syncLists[index] = itemData.CreateItem();
                    amount = 0;
                }
            }

            // 2개 이상의 수량 없는 아이템을 동시에 추가하는 경우
            index = -1;
            for (; amount > 0; amount--)
            {
                // 아이템을 넣은 인덱스의 다음 인덱스부터 슬롯 탐색
                index = FindEmptySlotIndex(index + 1);

                // 다 넣지 못한 경우 루프 종료
                if (index == -1)
                    break;

                // 아이템을 생성하여 슬롯에 추가
                syncLists[index] = itemData.CreateItem();
            }
        }

        return amount;
    }

    [ServerCallback]
    public Item Add(Item item)
    {
        int index;

        // 수량이 있는 아이템
        if (item is CountableItem cItem)
        {
            bool findNextCountable = true;
            index = -1;

            while (cItem.Amount > 0)
            {
                // 해당 아이템이 인번토리 내에 존재하고, 개수 여유 있는지 검사
                if (findNextCountable)
                {
                    index = FindCountableItemSlotIndex(cItem.countableItemData, index + 1);

                    // 여유 있는 슬롯이 더이상 없으면 빈 슬롯부터 탐색
                    if (index == -1)
                    {
                        findNextCountable = false;
                    }
                    // 기존 아이템을 찾으면 양을 증가시키고 amount에 초기화
                    else
                    {
                        CountableItem ci = syncLists[index] as CountableItem;
                        cItem.Amount = ci.AddAmountAndGetExcess(cItem.Amount);
                        syncLists[index] = null;
                        syncLists[index] = ci;
                    }
                }
                // 1-2. 빈 슬롯 탐색
                else
                {
                    index = FindEmptySlotIndex(index + 1);

                    // 빈 슬롯이 존재하지 않을시 종료
                    if (index == -1)
                    {
                        // 월드 아이템 으로 스폰 (추후 구현)
                        return item;
                    }
                    // 빈 슬롯 발견 시, 슬롯에 아이템 추가 및 잉여량 계산
                    else
                    {
                        // 해당 영역에 아이템 푸시
                        syncLists[index] = cItem;
                        return null;
                    }
                }
            }
        }
        else if (item is CoinItem coinItem)
        {
            Coin += coinItem.coinValue;
        }
        // 수량이 없는 아이템
        else
        {
            index = FindEmptySlotIndex();
            if (index != -1)
            {
                // 슬롯에 추가
                syncLists[index] = item;
            }
            else if (index == -1)
            {
                // 월드 아이템 으로 스폰 (추후 구현)
                return item;
            }
        }

        // 성공적으로 추가시 null로 반환
        return null;
    }

    [Command]
    public void SeparateAmountRemove(int index, int amount)
    {
        if (amount == 0)
            return;

        if (!IsValidIndex(index))
            return;

        if (syncLists[index] is CountableItem == false)
            return;

        CountableItem item = syncLists[index] as CountableItem;

        if (item.IsEmpty)
            return;

        // 수량 설정하고 갱신
        int cacheAmount = item.Amount;
        int dropAmount = Mathf.Clamp(((item.Amount - amount) > 0 ? amount : item.Amount), 0, item.MaxAmount);

        item.SetAmount(item.Amount - amount);

        // 아이템이 비어있으면 새로운 아이템을 생성하지 않고 이 아이템을 그대로 월드 아이템 클래스에 넣어준다.
        if (item.IsEmpty)
        {
            syncLists[index] = null;
            item.SetAmount(cacheAmount);
            Managers.Game.CreateWorldItem(item, gameObject);
        }
        // 만약 비어있찌 않으면 배열을 갱신해주고 새로운 아이템을 생성해 월드 아이템 클래스에 넣어준다.
        else
        {
            syncLists[index] = null;
            syncLists[index] = item;

            Item cloneItem = item.countableItemData.CreateItem();
            cloneItem.Amount = dropAmount;
            Managers.Game.CreateWorldItem(cloneItem, gameObject);
        }
    }

    [Command]
    public void SeparateCoinRemove(long removeCoin)
    {
        if (Coin == 0 || Coin < removeCoin)
            return;

        Coin -= removeCoin;
        // 코인 아이템 드랍
        Data.CoinItemData itemData = Managers.Data.CoinItemDict[0];
        Item coinItem = itemData.CreateItem(removeCoin);
        Managers.Game.CreateWorldItem(coinItem, gameObject);
    }

    [Command]
    public void Remove(int index)
    {
        if (!IsValidIndex(index))
            return;

        Managers.Game.CreateWorldItem(syncLists[index], gameObject);

        syncLists[index] = null;
    }
}
