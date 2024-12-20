using Mirror;
using UnityEngine;

public class PlayerAnimEvt : AnimationEvent, IAttackEvent, IEffect, ISound
{
    protected Player player;
    protected PlayerController controller;
    protected Coroutine dashCoolTimeCoroutine;

    // 크리쳐 사운드
    [SerializeField]
    protected string moveSound;
    [SerializeField]
    protected string jumpSound;
    [SerializeField]
    protected string dashSound;
    [SerializeField]
    protected string hitSound;
    [SerializeField]
    protected string deadSound;

    public override void Init()
    {
        player = GetComponentInParent<Player>();
        creature = player;   
        controller = GetComponentInParent<PlayerController>();
    }

    #region 대쉬 이벤트 함수
    [ClientCallback]
    public virtual void OnCanAddInputDushBuffer()
    {
        if (isOwned == false)
            return;

        if (player.StateMachine.State == Define.State.Dash)
        {
            (player.StateMachine.BaseState as DashState).CanAddInputBuffer = true;
        }
    }

    [ClientCallback]
    public override void OnDashEnd()
    {
        if (isOwned == false)
            return;

        if (player.StateMachine.State == Define.State.Dash)
        {
            DashState dashState = player.StateMachine.BaseState as DashState;
            dashState.isFinishedDash = true;

            // 버퍼에 선입력으로 넣었던게 남아있다면, 다시 Dash로 상태 전환
            if (dashState.inputDirectionBuffer.Count > 0)
            {
                if (player.StateMachine.ChangeState(Define.State.Dash))
                {
                    return;
                }
            }
        }

        if (player.StateMachine.ChangeState(Define.State.Idle))
            return;
        else if (player.StateMachine.ChangeState(Define.State.Moving))
            return;
        else if (player.StateMachine.ChangeState(Define.State.Run))
            return;
    }

    [ClientCallback]
    public override void OnCanDashAttack()
    {
        if (isOwned == false)
            return;

        if (creature.StateMachine.State == Define.State.Dash)
        {
            (creature.StateMachine.BaseState as DashState).CanDashAttack = true;
        }
    }
    #endregion

    #region 공격 인터페이스 함수

    [ClientCallback]
    public virtual void OnCanAddInputAtkBuffer()
    {
        if (isOwned == false)
            return;

        if (creature.StateMachine.State == Define.State.NormalAttack)
        {
            if (creature.StateMachine.BaseState is IPreInput)
            {
                IPreInput preInput = creature.StateMachine.BaseState as IPreInput;
                preInput.CanAddInputBuffer = true;
            }
        }
    }

    [ServerCallback]
    public virtual void OnProjectileAttack()
    {
        if (creature.StateMachine.State == Define.State.NormalAttack)
        {
            if (creature.GetEquipment.EquipWeapon is RangedWeapon)
            {
                RangedWeapon rangedWeapon = creature.GetEquipment.EquipWeapon as RangedWeapon;
                rangedWeapon.ProjectileFire(creature.normalAtkPos.position);
            }
        }
    }

    [ServerCallback]
    public virtual void OnHitJudgmentStart()
    {
        if (creature.StateMachine.State == Define.State.NormalAttack)
        {
            creature.GetEquipment.EquipWeapon.IsAttack = true;
        }
    }

    [ServerCallback]
    public virtual void OnHitJudgmentEnd()
    {
        if (creature.StateMachine.State == Define.State.NormalAttack)
        {
            creature.GetEquipment.EquipWeapon.IsAttack = false;
        }
    }


    [ClientCallback]
    public override void OnAttackEnd()
    {
        if (isOwned == false)
            return;

        if (creature.StateMachine.State == Define.State.NormalAttack)
        {
            if (creature.StateMachine.BaseState is IPreInput)
            {
                IPreInput preInput = creature.StateMachine.BaseState as IPreInput;
                if (preInput.PreInput)
                    if (creature.StateMachine.ChangeState(Define.State.NormalAttack))
                        return;
            }
        }

        if (creature.StateMachine.ChangeState(Define.State.Idle))
            return;
        else if(creature.StateMachine.ChangeState(Define.State.Moving))
            return;
        else if (creature.StateMachine.ChangeState(Define.State.Run))
            return;
    }

    #endregion

    #region 벽타기, 벽에서 점프
    [ClientCallback]
    public virtual void OnWallAble()
    {
        if (isOwned == false)
            return;

        if (player.StateMachine.BaseState is JumpState)
        {
            JumpState jumpState = player.StateMachine.BaseState as JumpState;
            jumpState.isOnWallable = true;
        }
    }

    [ClientCallback]
    public virtual void OnWallJumpAble()
    {
        if (isOwned == false)
            return;

        if (player.StateMachine.BaseState is OnWallState)
        {
            OnWallState onWallState = player.StateMachine.BaseState as OnWallState;
            onWallState.isJumpable = true;
        }
    }

    #endregion

    [ServerCallback]
    public override void OnDeadEnd()
    {
        // 플레이어 사망
        player.S_PlayerDead();
    }

    [ClientCallback]
    public override void OnHitEnd()
    {
        if (isOwned == false)
            return;

        if (creature.StateMachine.State == Define.State.Hit)
        {
            creature.StateMachine.ChangeState(Define.State.Idle, false);
        }
    }


    #region 이펙트
    [ClientCallback]
    public void OnAttackEffect()
    {
        if (Util.IsSameScene(gameObject) == false)
            return;

        if (creature.StateMachine.State != Define.State.NormalAttack)
            return;

        if (creature.GetEquipment.EquipWeapon == null)
            return;

        Weapon weapon = creature.GetEquipment.EquipWeapon;
        if (weapon is IWeaponEffect)
        {
            IWeaponEffect effect = weapon as IWeaponEffect;

            effect.PlayComboAttackEffects();
        }
    }
    #endregion

    #region 사운드
    [ClientCallback]
    public void OnAttackSound()
    {
        if (Util.IsSameScene(gameObject) == false)
            return;

        if (creature.StateMachine.State != Define.State.NormalAttack)
            return;

        if (creature.GetEquipment.EquipWeapon == null)
            return;

        Weapon weapon = creature.GetEquipment.EquipWeapon;
        if (weapon is IWeaponSound)
        {
            IWeaponSound sound = weapon as IWeaponSound;

            sound.PlayComboAttackSound();
        }
    }

    [ClientCallback]
    public void OnMoveSound()
    {
        if (Util.IsSameScene(gameObject))
            Managers.Sound.Play3D(moveSound, transform.root, false, Define.Sound3D.Effect3D, 1, 1, 0, 10f);
    }

    [ClientCallback]
    public void OnJumpSound()
    {
        if (Util.IsSameScene(gameObject))
            Managers.Sound.Play3D(jumpSound, transform.root, false, Define.Sound3D.Effect3D, 1, 1, 0, 10f);
    }

    [ClientCallback]
    public void OnDashSound()
    {
        if (Util.IsSameScene(gameObject))
            Managers.Sound.Play3D(dashSound, transform.root, false, Define.Sound3D.Effect3D, 1, 1, 0, 10f);
    }

    [ClientCallback]
    public void OnHitSound()
    {
        if (Util.IsSameScene(gameObject))
            Managers.Sound.Play3D(hitSound, transform.root, false, Define.Sound3D.Effect3D, 0.5f, 1, 7.5f, 10f);
    }

    [ClientCallback]
    public void OnDeadSound()
    {
        if (Util.IsSameScene(gameObject))
            Managers.Sound.Play3D(deadSound, transform.root, false, Define.Sound3D.Effect3D, 0.5f, 1, 7.5f, 10f);
    }

    #endregion
}
