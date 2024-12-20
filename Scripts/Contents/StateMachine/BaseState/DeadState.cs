using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadState : BaseState
{
    public DeadState(Define.State _state, Creature _creature, BaseController _controller) : base(_state, _creature, _controller)
    {
    }

    public override void EnterState()
    {
        creatureGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BDead, true);
        
        if (creatureGS.GetNav != null)
            creatureGS.GetNav.enabled = false;

        // 물리 연산 비활성화
        creatureGS.GetRigidBody.isKinematic = true;

        // 중력 비활성화
        creatureGS.GetRigidBody.useGravity = false;
        
        // 콜라이더 비활성화
        creatureGS.GetCollider.enabled = false;

        if (creatureGS is Player)
            creatureGS.CTS_Collider(false);
        else
            creatureGS.RPC_Collider(false);
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {
        creatureGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BDead, false);

        if (creatureGS.GetNav != null)
        {
            creatureGS.GetNav.enabled = true;

            // 물리 연산 비활성화
            creatureGS.GetRigidBody.isKinematic = true;

            // 중력 비 활성화
            creatureGS.GetRigidBody.useGravity = false;
        }
        else
        {
            // 물리 연산 활성화
            creatureGS.GetRigidBody.isKinematic = false;

            // 중력 활성화
            creatureGS.GetRigidBody.useGravity = true;
        }

        // 콜라이더 활성화
        creatureGS.GetCollider.enabled = true;

        if (creatureGS is Player)
            creatureGS.CTS_Collider(true);
        else
            creatureGS.RPC_Collider(true);
    }

    public override void FixedUpdateState()
    {

    }

    public override void UpdateState()
    {
       
    }
}
