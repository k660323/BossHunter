using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEquipableItem
{
    // ������ ��뿩�� bool ������ ����
    bool Equip(GameObject target);

    bool UnEquip(GameObject target);

}
