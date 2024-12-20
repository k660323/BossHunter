using Data;
using System;
using UnityEngine;

[Serializable]
public class EquipmentItem : Item, IEquipableItem
{
    public EquipmentItemData EquipmentItemData { get; private set; }

    public int Durability
    {
        get => _durability;
        set
        {
            if (value < 0)
                value = 0;
            if(value > EquipmentItemData.MaxDurability)
                value = EquipmentItemData.MaxDurability;

            _durability = value;
        }
    }

    public int _durability;

    public EquipmentItem() : base()
    {

    }

    protected EquipmentItem(EquipmentItemData data) : base(data)
    {
        EquipmentItemData = data;
        Durability = data.MaxDurability;
    }

    public bool Equip(GameObject target)
    {
        if (target.TryGetComponent(out Creature creature))
        {
            EquipmentManager equipmentManager = creature.GetEquipment;
            return equipmentManager.EquipItem(this);
        }

        return false;
    }

    public bool UnEquip(GameObject target = null)
    {
        if (target.TryGetComponent(out Creature creature))
        {
            if (creature is Player player)
            {
                Inventory inventory = player.Inventory;
                // 인벤토리에 성공적으로 추가가 되면 null을 반환한다.
                return (inventory.Add(this) == null) ? true : false;
            }
            else if (creature is Monster monster)
            {
                // 임시
                // 그냥 템을 제거한다.
                return true;
            }
        }

        return false;
    }
}
