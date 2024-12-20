using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : BaseStatePlayer
{
    public IdleState(Define.State _state, Player _player, PlayerController _playerController) : base(_state, _player, _playerController)
    {
    }

    public override bool CheckCondition()
    {
        return PlayerControllerGS.inputDirection == Vector3.zero;
    }

    public override void EnterState()
    {
        PlayerGS.GetRigidBody.velocity = new Vector3(0, PlayerGS.GetRigidBody.velocity.y, 0);
        PlayerGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 0.0f);
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {

    }

    public override void FixedUpdateState()
    {
        if (PlayerGS.GetNetAnim.animator.GetFloat(Managers.AnimHash.FmoveSpeed) != 0.0f)
            PlayerGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 0.0f);

        PlayerGS.GetRigidBody.velocity = new Vector3(0, PlayerGS.GetRigidBody.velocity.y, 0);
    }

    public override void UpdateState()
    {
        if (PlayerGS.StateMachine.ChangeState(Define.State.Moving))
            return;
        else if (PlayerGS.StateMachine.ChangeState(Define.State.Run))
            return;
        else if (PlayerGS.StateMachine.ChangeState(Define.State.Jumping))
            return;
        else if(PlayerGS.StateMachine.ChangeState(Define.State.Dash))
            return;
        else if(PlayerGS.StateMachine.ChangeState(Define.State.NormalAttack))
            return;
    }
}
