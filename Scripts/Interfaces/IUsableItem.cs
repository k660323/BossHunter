using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUsableItem
{
    // ������ ��뿩�� bool ������ ����
    bool Use(GameObject target = null);
}
