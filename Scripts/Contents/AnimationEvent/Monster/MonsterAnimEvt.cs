using Mirror;
using UnityEngine;

public class MonsterAnimEvt : AnimationEvent, IAttackEvent, IEffect, ISound
{
    protected Monster monster;
    protected MonsterController controller;

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
        monster = GetComponentInParent<Monster>();
        creature = monster;
        controller = GetComponentInParent<MonsterController>();
    }

    [ServerCallback]
    public override void OnCanDashAttack()
    {
      
    }
    #region 공격 인터페이스 함수

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
    #endregion

    [ServerCallback]
    public override void OnAttackEnd()
    {
        // 아이들 상태로
        if (creature.StateMachine.State == Define.State.NormalAttack)
        {
            if (creature.StateMachine.ChangeState(Define.State.NormalAttack))
            {
                return;
            }
        }

        if (creature.StateMachine.ChangeState(Define.State.Idle))
            return;
    }

    [ServerCallback]
    public override void OnDashEnd()
    {
      
    }

    [ServerCallback]
    public override void OnHitEnd()
    {
        if (creature.StateMachine.State == Define.State.Hit)
        {
            creature.StateMachine.ChangeState(Define.State.Idle, false);
        }
    }

    [ServerCallback]
    public override void OnDeadEnd()
    {
        Invoke("DestoryObjectDelay", 3.0f);
    }

    void DestoryObjectDelay()
    {
        if (NetworkServer.active)
            NetworkServer.UnSpawn(creature.gameObject);
    }

    #region 사운드
    [ClientCallback]
    public void OnAttackSound()
    {
        if (Util.IsSameScene(gameObject) == false)
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
        
    }

    [ClientCallback]
    public void OnJumpSound()
    {
        
    }

    [ClientCallback]
    public void OnDashSound()
    {
        
    }

    [ClientCallback]
    public void OnHitSound()
    {
        if(Util.IsSameScene(gameObject))
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
