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
                // �κ��丮�� ���������� �߰��� �Ǹ� null�� ��ȯ�Ѵ�.
                return (inventory.Add(this) == null) ? true : false;
            }
            else if (creature is Monster monster)
            {
                // �ӽ�
                // �׳� ���� �����Ѵ�.
                return true;
            }
        }

        return false;
    }
}
