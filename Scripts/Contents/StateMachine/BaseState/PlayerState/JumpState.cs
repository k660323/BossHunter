using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : BaseStatePlayer
{
    // 점프 애니메이션을 어느정도 실행한 후에 벽을 탈 수 있게 한다.
    public bool isOnWallable;

    public JumpState(Define.State _state, Player _player, PlayerController _playerController) : base(_state, _player, _playerController)
    {

    }

    public override bool CheckCondition()
    {
        return PlayerControllerGS.IsJump;
    }
    public override void EnterState()
    {
        PlayerGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BJumpIn, true);
        PlayerGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BJumpOut, false);
        PlayerGS.GetRigidBody.velocity = new Vector3(PlayerGS.GetRigidBody.velocity.x, 0.0f, PlayerGS.GetRigidBody.velocity.z);
        Vector3 moveDir = PlayerControllerGS.inputDirection;
        PlayerGS.GetRigidBody.AddForce(moveDir + Vector3.up * 5.0f, ForceMode.Impulse); // 점프 세기 수정 예정
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {
        PlayerGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BJumpIn, false);
        PlayerGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BJumpOut, true);
        PlayerGS.GetRigidBody.velocity = Vector3.zero;
        isOnWallable = false;
    }

    public override void FixedUpdateState()
    {
        if (isOnWallable && PlayerGS.StateMachine.ChangeState(Define.State.OnWall))
            return;

        if (PlayerControllerGS.IsOnGround)
        {
            if (PlayerGS.StateMachine.ChangeState(Define.State.Dash))
                return;

            if (PlayerGS.StateMachine.ChangeState(Define.State.Idle))
                return;
            else if (PlayerGS.StateMachine.ChangeState(Define.State.Moving))
                return;
            else if (PlayerGS.StateMachine.ChangeState(Define.State.Run))
                return;
              
        }
        else
        {
            float currentMoveSpeed = PlayerGS.GetStat.MoveSpeed;
            PlayerControllerGS.LookAt(PlayerControllerGS.inputDirection);
            PlayerGS.GetRigidBody.MovePosition(PlayerGS.transform.position + PlayerControllerGS.calculatedDirection * currentMoveSpeed * Time.fixedDeltaTime);
            // PlayerGS.gameObject.transform.position += PlayerController.calculatedDirection * currentMoveSpeed * Time.fixedDeltaTime; //+ new Vector3(0, Player.GetRigidBody.velocity.y, 0);
        }
    }

    public override void UpdateState()
    {
        
    }
}
