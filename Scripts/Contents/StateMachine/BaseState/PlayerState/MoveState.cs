using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : BaseStatePlayer
{
    public MoveState(Define.State _state, Player _player, PlayerController _playerController) : base(_state, _player, _playerController)
    {
    }

    public override bool CheckCondition()
    {
        return PlayerControllerGS.inputDirection != Vector3.zero && PlayerControllerGS.IsRun == false;
    }

    public override void EnterState()
    {
        PlayerGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 0.5f);
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {
        PlayerGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 0.0f);
    }

    public override void FixedUpdateState()
    {
        if(PlayerGS.GetNetAnim.animator.GetFloat(Managers.AnimHash.FmoveSpeed) != 0.5f)
            PlayerGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 0.5f);

        float currentMoveSpeed = PlayerGS.GetStat.MoveSpeed;
        PlayerControllerGS.LookAt(PlayerControllerGS.inputDirection);
        PlayerGS.GetRigidBody.MovePosition(PlayerGS.transform.position + PlayerControllerGS.calculatedDirection * currentMoveSpeed * Time.fixedDeltaTime);
    }

    public override void UpdateState()
    {
        if (PlayerGS.StateMachine.ChangeState(Define.State.Idle))
            return;
        else if (PlayerGS.StateMachine.ChangeState(Define.State.Jumping))
           return;
        else if(PlayerGS.StateMachine.ChangeState(Define.State.Dash))
            return;
        else if(PlayerGS.StateMachine.ChangeState(Define.State.Run))
            return;
        else if(PlayerGS.StateMachine.ChangeState(Define.State.NormalAttack))
            return;
    }
}
