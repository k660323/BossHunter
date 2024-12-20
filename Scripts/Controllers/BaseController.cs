using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class BaseController : NetworkBehaviour
{
    public Creature creature { get; protected set; }

    private void Awake()
    {
        Init();
    }

    public abstract void Init();

    public void LookAt(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Quaternion targetAngle = Quaternion.LookRotation(direction);
            creature.GetRigidBody.MoveRotation(targetAngle);
        }
    }

    public void LookAt(Transform target, bool isZeroY = true)
    {
        if (target != null)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            if (isZeroY)
                dir.Set(dir.x, 0, dir.z);
            LookAt(dir);
        }
    }
}
