using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunState : BaseStatePlayer
{
    public float addtiveMoveSpeed { get; protected set; }

    public RunState(Define.State _state, Player _player, PlayerController _playerController, float _addtiveMoveSpeed = 1.5f) : base(_state, _player, _playerController)
    {
        addtiveMoveSpeed = _addtiveMoveSpeed;
    }

    public override bool CheckCondition()
    {
        return PlayerControllerGS.inputDirection != Vector3.zero && PlayerControllerGS.IsRun;
    }

    public override void EnterState()
    {
        PlayerGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 1.0f);
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {
        PlayerGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 0.0f);
    }

    public override void FixedUpdateState()
    {
        // ���� ����ȭ�� �ȵɰ�찡 �־ FixedUpdate���� üũ
        if (PlayerGS.GetNetAnim.animator.GetFloat(Managers.AnimHash.FmoveSpeed) != 1.0f)
            PlayerGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 1.0f);

        // �÷��̾��� ���ǵ带 �����ϰ� ��ǲ�������� �ٶ󺸰� �������ش�.
        float currentMoveSpeed = PlayerGS.GetStat.MoveSpeed * addtiveMoveSpeed;
        PlayerControllerGS.LookAt(PlayerControllerGS.inputDirection);
        PlayerGS.GetRigidBody.MovePosition(PlayerGS.transform.position + PlayerControllerGS.calculatedDirection * currentMoveSpeed * Time.fixedDeltaTime);
    }

    public override void UpdateState()
    {
        if (PlayerGS.StateMachine.ChangeState(Define.State.Idle))
            return;
        else if (PlayerGS.StateMachine.ChangeState(Define.State.Jumping))
            return;
        else if (PlayerGS.StateMachine.ChangeState(Define.State.Dash))
            return;
        else if (PlayerGS.StateMachine.ChangeState(Define.State.Moving))
            return;
        else if (PlayerGS.StateMachine.ChangeState(Define.State.NormalAttack))
            return;
    }
}
