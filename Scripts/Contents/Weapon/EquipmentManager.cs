using Data;
using Mirror;
using System;
using UnityEngine;

public class EquipmentManager : NetworkBehaviour
{
    // 플레이어 Stat
    private Stat stat;
    public Stat GetStat {  get { return stat; } }

    protected Func<EquipmentItem, bool>[] equipAction;
    protected Func<Define.ItemSubType, bool>[] unEquipAction;

    // 곧 삭제됨
    [SerializeField]
    Transform weaponPos;

    private Weapon equipWeapon;
    public Weapon EquipWeapon { get { return equipWeapon; } private set { equipWeapon = value; } }

    // 연결된 인벤토리 UI
    [HideInInspector]
    private UI_EquipmentManager uI_Equipment;

    // 장비
    SyncList<EquipmentItem> equipItems = new SyncList<EquipmentItem>();

    // 무기 가져오기
    private void Awake()
    {
        // Stat 컴포넌트 캐싱
        TryGetComponent(out stat);
        // 루트 오브젝트 밑에 배치
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

    // 클라이언트 콜백 이벤트
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

            // UI 인벤토리 연결될때
            // Process initial SyncList payload
            equipItems.Callback -= OnEquipmentUpdated;
            equipItems.Callback += OnEquipmentUpdated;

            for (int index = 0; index < equipItems.Count; index++)
                OnEquipmentUpdated(SyncList<EquipmentItem>.Operation.OP_SET, index, new EquipmentItem(), equipItems[index]);
        }
    }

    // 일단 무기 데이터가 들어오면 알아서 동기화 되도록 구현 목표
    // 인벤토리에서 우클릭(클라) -> 해당 아이템이 장착아이템이면 인벤토리에서 장비매니저로 접근(서버) -> EquipItem 호출(서버)
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

    #region 바인딩 함수


    bool ArmorItemEquip(EquipmentItem item)
    {
        if (item == null)
            return false;

        int index = (int)item.Data._itemSubType;

        // 장비 레벨과 플레이어 레벨 비교 추가 예정

        // 현재 장비중인 아이템이 없다.
        if (equipItems[index] == null)
        {
            // 바로 장착
            equipItems[index] = item;
            // UI 업데이트 (인벤템은 알아서 사라짐)
            ArmorStatApply(item, true);
            return true;
        }

        return false;
    }

    bool ArmorItemUnEquip(Define.ItemSubType itemSubType)
    {
        int index = (int)itemSubType;

        // 탈착 실패
        if (equipItems[index] == null)
            return false;
      
        EquipmentItem item = equipItems[index];

        // 인벤토리에 이 아이템을 추가
        if (item is IEquipableItem eItem)
        {
            bool success = eItem.UnEquip(gameObject);
            if (success)
            {
                // 장비 탈착 기존의 장비 능력치만큼 플레이어 능력치 차감
                ArmorStatApply(equipItems[index], false);

                // 탈착 성공시 장비창의 아이템을 null로 밀어낸다.
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

        // 장비 레벨과 플레이어 레벨 비교 추가 예정

        // 현재 장비중인 아이템이 없다.
        if (equipItems[index] == null)
        {
            // 바로 장착
            equipItems[index] = item as WeaponItem;
            // UI 업데이트 (인벤템은 알아서 사라짐)
            WeaponStatApply(equipItems[index], true);
            return true;
        }
        // 현재 장비중인 아이템이 있다.
        else
        {
            // 장비창에 있는 아이템은 인벤으로
            // 인벤창에 있는 아이템은 장비로

            // UI 업데이트

        }

        return false;
    }

    bool WeaponItemUnEquip(Define.ItemSubType itemSubType)
    {
        int index = (int)itemSubType;

        // 탈착 실패
        if (equipItems[index] == null)
            return false;

        EquipmentItem item = equipItems[index];

        // 인벤토리에 이 아이템을 추가
        if (item is IEquipableItem eItem)
        {
            bool succeeded = eItem.UnEquip(gameObject);
            if (succeeded)
            {
                // 장비 탈착 기존의 장비 능력치만큼 플레이어 능력치 차감
                WeaponStatApply(equipItems[index], false);

                // 탈착 성공시 장비창의 아이템을 null로 밀어낸다.
                equipItems[index] = null;
            }
        }

        return true;
    }

    #endregion

    #region 아이템 스텟 적용 함수
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

    // 해당 인덱스 슬롯 상태 및 UI 갱신
    [ClientCallback]
    void UpdateSlot(int index)
    {
        EquipmentItem eItem = equipItems[index];

        // 1. 아이템이 존재하는 경우
        if (eItem != null)
        {         
            uI_Equipment.SetItemIcon(eItem.Data._itemSubType, eItem.Data._iconSpritePath);
        }
        else
        {
            // 아이템 제거
            uI_Equipment.RemoveItem(index);
        }
    }

    // 해당 슬롯의 아이템 정보 반환
    public ItemData GetItemData(Define.ItemSubType itemSubType)
    {
        if (equipItems[(int)itemSubType] == null)
            return null;

        return equipItems[(int)itemSubType].Data;
    }
}