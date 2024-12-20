using Data;
using Mirror;
using System;
using UnityEngine;

public class EquipmentManager : NetworkBehaviour
{
    // �÷��̾� Stat
    private Stat stat;
    public Stat GetStat {  get { return stat; } }

    protected Func<EquipmentItem, bool>[] equipAction;
    protected Func<Define.ItemSubType, bool>[] unEquipAction;

    // �� ������
    [SerializeField]
    Transform weaponPos;

    private Weapon equipWeapon;
    public Weapon EquipWeapon { get { return equipWeapon; } private set { equipWeapon = value; } }

    // ����� �κ��丮 UI
    [HideInInspector]
    private UI_EquipmentManager uI_Equipment;

    // ���
    SyncList<EquipmentItem> equipItems = new SyncList<EquipmentItem>();

    // ���� ��������
    private void Awake()
    {
        // Stat ������Ʈ ĳ��
        TryGetComponent(out stat);
        // ��Ʈ ������Ʈ �ؿ� ��ġ
        equipWeapon = weaponPos.GetComponentInChildren<Weapon>();

#if UNITY_SERVER
        for (int i = 0; i < 13; i++)
            equipItems.Add(null);

        equipAction = new Func<EquipmentItem, bool>[13];
        unEquipAction = new Func<Define.ItemSubType, bool>[13];

        for (int i = 1; i < 4; i++)
        {
            equipAction[i] = WeaponItemEquip;
            unEquipAction[i] = WeaponItemUnEquip;
        }
        for(int i = 4; i < 13; i++)
        {
            equipAction[i] = ArmorItemEquip;
            unEquipAction[i] = ArmorItemUnEquip;
        }
#else
        if (Managers.Instance.mode == NetworkManagerMode.Host)
        {
            for (int i = 0; i < 13; i++)
                equipItems.Add(null);

            equipAction = new Func<EquipmentItem, bool>[13];
            unEquipAction = new Func<Define.ItemSubType, bool>[13];

            for (int i = 1; i < 4; i++)
            {
                equipAction[i] = WeaponItemEquip;
                unEquipAction[i] = WeaponItemUnEquip;
            }

            for (int i = 4; i < 13; i++)
            {
                equipAction[i] = ArmorItemEquip;
                unEquipAction[i] = ArmorItemUnEquip;
            }
        }
#endif
    }

    // Ŭ���̾�Ʈ �ݹ� �̺�Ʈ
    public void OnEquipmentUpdated<T>(SyncList<EquipmentItem>.Operation op, int index, T oldItem, T newItem) where T : EquipmentItem
    {
        switch (op)
        {
            case SyncList<EquipmentItem>.Operation.OP_SET:
                UpdateSlot(index);
                break;
        }
    }

    public override void OnStartLocalPlayer()
    {
        if (Managers.UI.SceneUI is IPlayerUI playerUI)
        {
            uI_Equipment = playerUI.GetPlayerUI.UI_EquipmentManager;
            uI_Equipment.equipmentManager = this;

            // UI �κ��丮 ����ɶ�
            // Process initial SyncList payload
            equipItems.Callback -= OnEquipmentUpdated;
            equipItems.Callback += OnEquipmentUpdated;

            for (int index = 0; index < equipItems.Count; index++)
                OnEquipmentUpdated(SyncList<EquipmentItem>.Operation.OP_SET, index, new EquipmentItem(), equipItems[index]);
        }
    }

    // �ϴ� ���� �����Ͱ� ������ �˾Ƽ� ����ȭ �ǵ��� ���� ��ǥ
    // �κ��丮���� ��Ŭ��(Ŭ��) -> �ش� �������� �����������̸� �κ��丮���� ���Ŵ����� ����(����) -> EquipItem ȣ��(����)
    public bool EquipItem(EquipmentItem item)
    {
        return equipAction[(int)item.Data._itemSubType].Invoke(item);
    }

    [Command]
    public void Use(Define.ItemSubType itemSubType)
    {
        if (unEquipAction[(int)itemSubType] == null)
            return;

        UnEquipItem(itemSubType);
    }

    public void UnEquipItem(Define.ItemSubType itemSubType)
    {
        unEquipAction[(int)itemSubType].Invoke(itemSubType);
    }

    #region ���ε� �Լ�


    bool ArmorItemEquip(EquipmentItem item)
    {
        if (item == null)
            return false;

        int index = (int)item.Data._itemSubType;

        // ��� ������ �÷��̾� ���� �� �߰� ����

        // ���� ������� �������� ����.
        if (equipItems[index] == null)
        {
            // �ٷ� ����
            equipItems[index] = item;
            // UI ������Ʈ (�κ����� �˾Ƽ� �����)
            ArmorStatApply(item, true);
            return true;
        }

        return false;
    }

    bool ArmorItemUnEquip(Define.ItemSubType itemSubType)
    {
        int index = (int)itemSubType;

        // Ż�� ����
        if (equipItems[index] == null)
            return false;
      
        EquipmentItem item = equipItems[index];

        // �κ��丮�� �� �������� �߰�
        if (item is IEquipableItem eItem)
        {
            bool success = eItem.UnEquip(gameObject);
            if (success)
            {
                // ��� Ż�� ������ ��� �ɷ�ġ��ŭ �÷��̾� �ɷ�ġ ����
                ArmorStatApply(equipItems[index], false);

                // Ż�� ������ ���â�� �������� null�� �о��.
                equipItems[index] = null;
            }
        }

        return true;
    }

    bool WeaponItemEquip(EquipmentItem item)
    {
        if (item == null)
            return false;

        int index = (int)item.Data._itemSubType;

        // ��� ������ �÷��̾� ���� �� �߰� ����

        // ���� ������� �������� ����.
        if (equipItems[index] == null)
        {
            // �ٷ� ����
            equipItems[index] = item as WeaponItem;
            // UI ������Ʈ (�κ����� �˾Ƽ� �����)
            WeaponStatApply(equipItems[index], true);
            return true;
        }
        // ���� ������� �������� �ִ�.
        else
        {
            // ���â�� �ִ� �������� �κ�����
            // �κ�â�� �ִ� �������� ����

            // UI ������Ʈ

        }

        return false;
    }

    bool WeaponItemUnEquip(Define.ItemSubType itemSubType)
    {
        int index = (int)itemSubType;

        // Ż�� ����
        if (equipItems[index] == null)
            return false;

        EquipmentItem item = equipItems[index];

        // �κ��丮�� �� �������� �߰�
        if (item is IEquipableItem eItem)
        {
            bool succeeded = eItem.UnEquip(gameObject);
            if (succeeded)
            {
                // ��� Ż�� ������ ��� �ɷ�ġ��ŭ �÷��̾� �ɷ�ġ ����
                WeaponStatApply(equipItems[index], false);

                // Ż�� ������ ���â�� �������� null�� �о��.
                equipItems[index] = null;
            }
        }

        return true;
    }

    #endregion

    #region ������ ���� ���� �Լ�
    void ArmorStatApply(EquipmentItem item, bool isEquip)
    {
        if (isEquip)
        {
            ArmorItem armorItem = item as ArmorItem;
            EquipmentItemStat itemStat = armorItem.EquipmentItemData._equipmentStat;
            stat.ExtraMaxHp += itemStat._maxHp;
            stat.ExtraHpRecoveryAmount += itemStat._hpRecoveryAmount;
            stat.ExtraMaxMp += itemStat._maxMp;
            stat.ExtraMpRecoveryAmount += itemStat._mpRecoveryAmount;
            stat.ExtraAttack += itemStat._attack;
            stat.ExtraDefense += itemStat._defense;
            stat.ExtraMoveSpeed += itemStat._moveSpeed;
            stat.ExtraAtkSpeed += itemStat._attackSpeed;
            stat.ExtraHitStatePriority += itemStat._hitStatePriority;
        }
        else
        {
            ArmorItem armorItem = item as ArmorItem;
            EquipmentItemStat itemStat = armorItem.EquipmentItemData._equipmentStat;
            stat.ExtraMaxHp -= itemStat._maxHp;
            stat.ExtraHpRecoveryAmount -= itemStat._hpRecoveryAmount;
            stat.ExtraMaxMp -= itemStat._maxMp;
            stat.ExtraMpRecoveryAmount -= itemStat._mpRecoveryAmount;
            stat.ExtraAttack -= itemStat._attack;
            stat.ExtraDefense -= itemStat._defense;
            stat.ExtraMoveSpeed -= itemStat._moveSpeed;
            stat.ExtraAtkSpeed -= itemStat._attackSpeed;
            stat.ExtraHitStatePriority -= itemStat._hitStatePriority;
        }
    }

    void WeaponStatApply(EquipmentItem item, bool isEquip)
    {
        if (isEquip)
        {
            WeaponItem weaponItem = item as WeaponItem;
            EquipmentItemStat itemStat = weaponItem.EquipmentItemData._equipmentStat;
            stat.ExtraMaxHp += itemStat._maxHp;
            stat.ExtraHpRecoveryAmount += itemStat._hpRecoveryAmount;
            stat.ExtraMaxMp += itemStat._maxMp;
            stat.ExtraMpRecoveryAmount += itemStat._mpRecoveryAmount;
            stat.ExtraAttack += itemStat._attack;
            stat.ExtraDefense += itemStat._defense;
            stat.ExtraMoveSpeed += itemStat._moveSpeed;
            stat.ExtraAtkSpeed += itemStat._attackSpeed;
            stat.ExtraHitStatePriority += itemStat._hitStatePriority;
            WeaponItemStat weaponStat = weaponItem.WeaponItemData._weaponStat;
            equipWeapon.WeaponStatSet(weaponStat);

            if(equipWeapon is RangedWeapon rangedWeapon)
            {
                if(item is RangeWeaponItem rangedWeaponItem)
                {
                    RangeWeaponItemStat rangedWeaponStat = rangedWeaponItem.RangeWeaponItemData._rangeWeaponStat;
                    rangedWeapon.RangeWeaponStatSet(rangedWeaponStat);
                }
            }
        }
        else
        {
            WeaponItem weaponItem = item as WeaponItem;
            EquipmentItemStat itemStat = weaponItem.EquipmentItemData._equipmentStat;
            stat.ExtraMaxHp -= itemStat._maxHp;
            stat.ExtraHpRecoveryAmount -= itemStat._hpRecoveryAmount;
            stat.ExtraMaxMp -= itemStat._maxMp;
            stat.ExtraMpRecoveryAmount -= itemStat._mpRecoveryAmount;
            stat.ExtraAttack -= itemStat._attack;
            stat.ExtraDefense -= itemStat._defense;
            stat.ExtraMoveSpeed -= itemStat._moveSpeed;
            stat.ExtraAtkSpeed -= itemStat._attackSpeed;
            stat.ExtraHitStatePriority -= itemStat._hitStatePriority;
            equipWeapon.WeaponStatInit();
        }
    }
    #endregion

    // �ش� �ε��� ���� ���� �� UI ����
    [ClientCallback]
    void UpdateSlot(int index)
    {
        EquipmentItem eItem = equipItems[index];

        // 1. �������� �����ϴ� ���
        if (eItem != null)
        {         
            uI_Equipment.SetItemIcon(eItem.Data._itemSubType, eItem.Data._iconSpritePath);
        }
        else
        {
            // ������ ����
            uI_Equipment.RemoveItem(index);
        }
    }

    // �ش� ������ ������ ���� ��ȯ
    public ItemData GetItemData(Define.ItemSubType itemSubType)
    {
        if (equipItems[(int)itemSubType] == null)
            return null;

        return equipItems[(int)itemSubType].Data;
    }
}