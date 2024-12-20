using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadStatePhysics : BaseState
{
    const float physicsPower = 10.0f;
    public DeadStatePhysics(Define.State _state, Creature _creature, BaseController _controller) : base(_state, _creature, _controller)
    {
    }

    public override void EnterState()
    {
        creatureGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BDead, true);

        if (creatureGS.GetNav != null)
            creatureGS.GetNav.enabled = false;

        // 리지드 바디 초기화
        creatureGS.GetRigidBody.isKinematic = false;
        creatureGS.GetRigidBody.useGravity = true;
        creatureGS.GetRigidBody.constraints = RigidbodyConstraints.None;

        // 뒤로 날아가는 방향
        Vector3 dir = -1 * (creatureGS.transform.forward + creatureGS.transform.up);
        float randomPower = Random.Range(0.0f, physicsPower);
        creatureGS.GetRigidBody.AddForce(dir * randomPower, ForceMode.Impulse);
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {
        creatureGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BDead, false);

        if (creatureGS.GetNav != null)
            creatureGS.GetNav.enabled = true;

        // 리지드 바디 초기화
        creatureGS.GetRigidBody.isKinematic = true;
        creatureGS.GetRigidBody.useGravity = false;
        creatureGS.GetRigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    public override void FixedUpdateState()
    {
      
    }

    public override void UpdateState()
    {
      
    }
}
