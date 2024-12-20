using Data;
using System;

[Serializable]
public class WeaponItem : EquipmentItem
{
    public WeaponItemData WeaponItemData { get; private set; }
    public WeaponItem() : base()
    {
    }

    public WeaponItem(WeaponItemData data) : base(data)
    {
        WeaponItemData = data;
    }
}
