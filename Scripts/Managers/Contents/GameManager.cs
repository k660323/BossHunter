using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using System;

public class GameManager
{
    public void CreateWorldItem(Item item, GameObject gameObject)
    {
        if (item == null)
            return;

        // 월드 아이템 스폰
        GameObject worldItemObject = Managers.Resource.NetInstantiate("WorldItem", gameObject.scene, true);
        if (worldItemObject.TryGetComponent(out WorldItem worldItem))
        {
            worldItem.SetItem(item);
            worldItem.transform.position = gameObject.transform.position;
            NetworkServer.Spawn(worldItemObject);
        }
    }
}
