using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTransformCustom : NetworkBehaviour
{
    [SyncVar]
    Vector3 desPos;

    private void FixedUpdate()
    {
        if(Managers.Instance.mode == NetworkManagerMode.ServerOnly || Managers.Instance.mode == NetworkManagerMode.Host)
        {
            desPos = transform.position;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, desPos, 0.5f);
        }
    }
}
