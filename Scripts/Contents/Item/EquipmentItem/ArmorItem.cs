using Data;
using System;

[Serializable]
public class ArmorItem : EquipmentItem
{
    public ArmorItem() : base()
    {

    }

    public ArmorItem(ArmorItemData data) : base(data)
    {

    }
}
