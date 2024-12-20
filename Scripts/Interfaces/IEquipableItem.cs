using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEquipableItem
{
    // 아이템 사용여부 bool 변수로 리턴
    bool Equip(GameObject target);

    bool UnEquip(GameObject target);

}
