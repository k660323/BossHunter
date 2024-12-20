using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : Player, IColliderInfo
{
    #region 콜라이더 정보
    public float GetHeight()
    {
        CapsuleCollider capsule = col as CapsuleCollider;
        return capsule.height;
    }
    public float GetRadius()
    {
        CapsuleCollider capsule = col as CapsuleCollider;
        return capsule.radius;
    }
    #endregion

    protected override void Init(Define.ObjectType objectType = Define.ObjectType.Player)
    {
        base.Init(objectType);
        playerType = Define.PlayerType.Archer;
    }

    protected override void InitStateMachine()
    {
        IdleState idleState = new IdleState(Define.State.Idle, this, controller);
        stateMachine.DefaultState(Define.State.Idle, idleState);

        MoveState moveState = new MoveState(Define.State.Moving, this, controller);
        stateMachine.RegisterState(Define.State.Moving, moveState);

        JumpState jumpState = new JumpState(Define.State.Jumping, this, controller);
        stateMachine.RegisterState(Define.State.Jumping, jumpState);

        DashState dashState = new DashState(Define.State.Dash, this, controller);
        stateMachine.RegisterState(Define.State.Dash, dashState);

        RunState runState = new RunState(Define.State.Run, this, controller);
        stateMachine.RegisterState(Define.State.Run, runState);

        ProjectileAttackState attackState = new ProjectileAttackState(Define.State.NormalAttack, this, controller);
        stateMachine.RegisterState(Define.State.NormalAttack, attackState);

        HitState hitState = new HitState(Define.State.Hit, this, controller);
        stateMachine.RegisterState(Define.State.Hit, hitState);

        DeadState deshState = new DeadState(Define.State.Dead, this, controller);
        stateMachine.RegisterState(Define.State.Dead, deshState);

        OnWallState onWallState = new OnWallState(Define.State.OnWall, this, controller);
        stateMachine.RegisterState(Define.State.OnWall, onWallState);
    }

  
}
