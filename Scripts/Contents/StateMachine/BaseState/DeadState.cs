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

        // ���� ���� ��Ȱ��ȭ
        creatureGS.GetRigidBody.isKinematic = true;

        // �߷� ��Ȱ��ȭ
        creatureGS.GetRigidBody.useGravity = false;
        
        // �ݶ��̴� ��Ȱ��ȭ
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

            // ���� ���� ��Ȱ��ȭ
            creatureGS.GetRigidBody.isKinematic = true;

            // �߷� �� Ȱ��ȭ
            creatureGS.GetRigidBody.useGravity = false;
        }
        else
        {
            // ���� ���� Ȱ��ȭ
            creatureGS.GetRigidBody.isKinematic = false;

            // �߷� Ȱ��ȭ
            creatureGS.GetRigidBody.useGravity = true;
        }

        // �ݶ��̴� Ȱ��ȭ
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
