using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeWeaponItem : WeaponItem
{
    public RangeWeaponItemData RangeWeaponItemData { get; private set; }
    public RangeWeaponItem() : base()
    {
    }

    public RangeWeaponItem(RangeWeaponItemData data) : base(data)
    {
        RangeWeaponItemData = data;
    }
}
