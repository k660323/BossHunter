using Data;
using Mirror;
using System;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    [SyncVar]
    private int capacity;
    // ������ ���� �ѵ�
    public int Capacity { get { return capacity; } private set { capacity = value; } }

    // �ʱ� ���� �ѵ� <= �ִ� �����ѵ�

    // �ʱ� ���� �ѵ�
    [SerializeField, SyncVar, Range(8, 64)]
    private int _initalCapacity = 32;

    // �ִ� ���� �ѵ� (������ �迭 ũ��)
    [SerializeField, SyncVar, Range(8, 64)]
    private int _maxCapacity = 64;

    // ����� �κ��丮 UI
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

    // Ŭ���̾�Ʈ �ݹ� �̺�Ʈ
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
            // UI �ʱ�ȭ
            UpdateAccessibleStatesAll();

            // UI �κ��丮 ����ɶ�
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

    // ��� ���� UI�� ���� ���� ���� ������Ʈ
    public void UpdateAccessibleStatesAll()
    {
        uI_Inventory.SetAccessibleSlotRange(Capacity);
    }

    // ���� ������ �ε��� ���� Ȯ��
    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < Capacity;
    }

    // �տ������� ��� �ִ� �ε��� Ž�� 
    private int FindEmptySlotIndex(int startIndex = 0)
    {
        for (int i = startIndex; i < Capacity; i++)
        {
            if (syncLists[i] == null)
                return i;
        }

        // ��� �ִ� �ε����� ������ -1�� ��ȯ
        return -1;
    }

    /// <summary> �տ������� ���� ������ �ִ� Countable �������� ���� �ε��� Ž�� </summary>
    [ServerCallback]
    private int FindCountableItemSlotIndex(CountableItemData target, int startIndex = 0)
    {
        for (int i = startIndex; i < Capacity; i++)
        {
            // var current = items[i];
            var current = syncLists[i];

            if (current == null)
                continue;

            // ������ ���� ��ġ, ���� ���� Ȯ��
            if (current.Data == target && current is CountableItem ci)
            {
                if (!ci.IsMax)
                    return i;
            }
        }

        return -1;
    }

    // �ش� �ε����� �������� �ִ��� Ȯ��
    public bool HasItem(int index)
    {
        return IsValidIndex(index) && syncLists[index] != null;
    }

    // �ش� ������ �������� �� �� �ִ� �������� ���
    [ServerCallback]
    public bool IsCountableItem(int index)
    {
        return HasItem(index) && syncLists[index] is CountableItem;
    }

    // �ش� �ε����� ������ �� ��ȯ
    public int GetCurrentAmount(int index)
    {
        // �������� ���� �ε��� -1 ��ȯ
        if (!IsValidIndex(index))
            return -1;

        // �������� �������� ���� 0 ��ȯ
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

    // �ش� ������ ������ ���� ��ȯ
    public ItemData GetItemData(int index)
    {
        if (!IsValidIndex(index))
            return null;

        if (syncLists[index] == null)
            return null;

        return syncLists[index].Data;
    }

    // �ش� ������ ������ �̸� ����
    public string GetItemName(int index)
    {
        if (!IsValidIndex(index))
            return "";

        if (syncLists[index] == null)
            return "";

        return syncLists[index].Data._name;
    }

    // �ش� �ε��� ���� ���� �� UI ����
    [ClientCallback]
    public void UpdateSlot(int index)
    {
        if (!IsValidIndex(index))
            return;

        Item item = syncLists[index];

        // 1. ������ ������ �����ϴ� ���
        if (item != null)
        {
            // ������ ���
            uI_Inventory.SetItemIcon(index, item.Data._iconSpritePath);

            // �� �� �ִ� ������
            // �Һ�, ��Ÿ�� �� ���
            if (item.Data._itemType == Define.ItemType.Countable)
            {
                // ������ 0�� ��� ������ ����
                if (item.Amount <= 0)
                {
                    // �������� ����
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
        // �� ������ ��� ������ ����
        else
        {
            RemoveIcon();
        }

        void RemoveIcon()
        {
            // ������ ����
            uI_Inventory.RemoveItem(index);
            // ���� �ؽ�Ʈ �����
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

    //    // 1. ������ ������ �����ϴ� ���
    //    if (item != null)
    //    {
    //        // ������ ���
    //        uI_Inventory.SetItemIcon(index, item.Data._iconSprite);

    //        // �� �� �ִ� ������
    //        if (item.Data._itemType == Define.ItemType.Countable)
    //        {
    //            // ������ 0�� ��� ������ ����
    //            if (item.Amount <= 0)
    //            {
    //                // �������� ����
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
    //            Debug.Log("���� ���� ������");
    //            uI_Inventory.HideItemAmountText(index);
    //        }
    //    }
    //    // �� ������ ��� ������ ����
    //    else
    //    {
    //        RemoveIcon();
    //    }

    //    void RemoveIcon()
    //    {
    //        // ������ ����
    //        uI_Inventory.RemoveItem(index);
    //        // ���� �ؽ�Ʈ �����
    //        uI_Inventory.HideItemAmountText(index);
    //    }
    //}

    /// <summary> �ش��ϴ� �ε����� ���Ե��� ���� �� UI ���� </summary>
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

        // �������� �Ѵ� �����ϰ�, ������ �������̰�, �� �� �ִ� �������� ���
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
        // �Ϲ����� ��� : ���� ��ü
        else
        {
            syncLists[indexA] = itemB;
            syncLists[indexB] = itemA;
        }
    }

    /// <summary> �� �� �ִ� �������� ���� ������(A -> B ��������) </summary>
    [ServerCallback]
    public void SeparateAmount(int indexA, int indexB, int amount)
    {
        // amount : ���� ��ǥ ����

        if (!IsValidIndex(indexA)) 
            return;
        if (!IsValidIndex(indexB)) 
            return;

        Item _itemA = syncLists[indexA];
        Item _itemB = syncLists[indexB];

        CountableItem _ciA = _itemA as CountableItem;

        // ���� : A ���� - �� �� �ִ� ������ / B ���� - Null
        // ���ǿ� �´� ���, �����Ͽ� ���� B�� �߰�
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

        // ��� ������ �������� ���
        if (syncLists[index] is IUsableItem uItem)
        {
            // ������ ���
            bool succeeded = uItem.Use(gameObject);
            if (succeeded)
            {
                Item _itemA = syncLists[index];

                // ������ ������Ʈ�� ���� �ش� �ε����� null�� �о��.
                syncLists[index] = null;

                // �� �� �ִ� �������� ���
                if (_itemA is CountableItem)
                {
                    CountableItem _cntItem = _itemA as CountableItem;
                    // ��� ���� ������ �ٽ� ������ ����
                    if (_cntItem.IsEmpty == false)
                        syncLists[index] = _itemA;
                }
            }
        }
        else if (syncLists[index] is IEquipableItem eItem)
        {
            // ������ ����
            bool succeeded = eItem.Equip(gameObject);
            if (succeeded)
            {
                // ���������� �κ��丮�� �������� null�� �о��.
                syncLists[index] = null;
            }
        }
    }

    // �κ��丮�� ������ �߰�
    // ���������� �߰� �ϸ� 0 ��ȯ
    // ���н� ������ ��ȯ
    [ServerCallback]
    public int Add(ItemData itemData, int amount = 1)
    {
        int index;

        // ������ �ִ� ������
        if(itemData is CountableItemData ciData)
        {
            bool findNextCountable = true;
            index = -1;

            while(amount > 0)
            {
                // �ش� �������� �ι��丮 ���� �����ϰ�, ���� ���� �ִ��� �˻�
                if(findNextCountable)
                {
                    index = FindCountableItemSlotIndex(ciData, index + 1);

                    // ���� �ִ� ������ ���̻� ������ �� ���Ժ��� Ž��
                    if(index == -1)
                    {
                        findNextCountable = false;
                    }
                    // ���� �������� ã���� ���� ������Ű�� amount�� �ʱ�ȭ
                    else
                    {
                        CountableItem ci = syncLists[index] as CountableItem;
                        amount = ci.AddAmountAndGetExcess(amount);
                        syncLists[index] = null;
                        syncLists[index] = ci;
                    }
                }
                // 1-2. �� ���� Ž��
                else
                {
                    index = FindEmptySlotIndex(index + 1);

                    // �� ������ �������� ������ ����
                    if (index == -1)
                        break;
                    // �� ���� �߰� ��, ���Կ� ������ �߰� �� �׿��� ���
                    else
                    {
                        // ���ο� ������ ����
                        CountableItem ci = ciData.CreateItem() as CountableItem;
                        ci.SetAmount(amount);

                        // ���Կ� �߰�
                        syncLists[index] = ci;

                        // ���� ���� ���
                        amount = (amount > ciData._maxAmount) ? (amount - ciData._maxAmount) : 0;
                    }
                }
            }
        }
        // ������ ���� ������
        else
        {
            // 1���� �ִ� ���, ������ ����
            if(amount == 1)
            {
                index = FindEmptySlotIndex();
                if(index != -1)
                {
                    // �������� �����Ͽ� ���Կ� �߰�
                    syncLists[index] = itemData.CreateItem();
                    amount = 0;
                }
            }

            // 2�� �̻��� ���� ���� �������� ���ÿ� �߰��ϴ� ���
            index = -1;
            for (; amount > 0; amount--)
            {
                // �������� ���� �ε����� ���� �ε������� ���� Ž��
                index = FindEmptySlotIndex(index + 1);

                // �� ���� ���� ��� ���� ����
                if (index == -1)
                    break;

                // �������� �����Ͽ� ���Կ� �߰�
                syncLists[index] = itemData.CreateItem();
            }
        }

        return amount;
    }

    [ServerCallback]
    public Item Add(Item item)
    {
        int index;

        // ������ �ִ� ������
        if (item is CountableItem cItem)
        {
            bool findNextCountable = true;
            index = -1;

            while (cItem.Amount > 0)
            {
                // �ش� �������� �ι��丮 ���� �����ϰ�, ���� ���� �ִ��� �˻�
                if (findNextCountable)
                {
                    index = FindCountableItemSlotIndex(cItem.countableItemData, index + 1);

                    // ���� �ִ� ������ ���̻� ������ �� ���Ժ��� Ž��
                    if (index == -1)
                    {
                        findNextCountable = false;
                    }
                    // ���� �������� ã���� ���� ������Ű�� amount�� �ʱ�ȭ
                    else
                    {
                        CountableItem ci = syncLists[index] as CountableItem;
                        cItem.Amount = ci.AddAmountAndGetExcess(cItem.Amount);
                        syncLists[index] = null;
                        syncLists[index] = ci;
                    }
                }
                // 1-2. �� ���� Ž��
                else
                {
                    index = FindEmptySlotIndex(index + 1);

                    // �� ������ �������� ������ ����
                    if (index == -1)
                    {
                        // ���� ������ ���� ���� (���� ����)
                        return item;
                    }
                    // �� ���� �߰� ��, ���Կ� ������ �߰� �� �׿��� ���
                    else
                    {
                        // �ش� ������ ������ Ǫ��
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
        // ������ ���� ������
        else
        {
            index = FindEmptySlotIndex();
            if (index != -1)
            {
                // ���Կ� �߰�
                syncLists[index] = item;
            }
            else if (index == -1)
            {
                // ���� ������ ���� ���� (���� ����)
                return item;
            }
        }

        // ���������� �߰��� null�� ��ȯ
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

        // ���� �����ϰ� ����
        int cacheAmount = item.Amount;
        int dropAmount = Mathf.Clamp(((item.Amount - amount) > 0 ? amount : item.Amount), 0, item.MaxAmount);

        item.SetAmount(item.Amount - amount);

        // �������� ��������� ���ο� �������� �������� �ʰ� �� �������� �״�� ���� ������ Ŭ������ �־��ش�.
        if (item.IsEmpty)
        {
            syncLists[index] = null;
            item.SetAmount(cacheAmount);
            Managers.Game.CreateWorldItem(item, gameObject);
        }
        // ���� ������� ������ �迭�� �������ְ� ���ο� �������� ������ ���� ������ Ŭ������ �־��ش�.
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
        // ���� ������ ���
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
