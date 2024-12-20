using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerDryad : Monster, IHitable, IColliderInfo, IDropable
{
    #region ��� ó��
    public void DropItem()
    {
        // ������ ����
        monsterStat.DropItems();
    }
    #endregion

    #region �ǰ� ó��
    public bool IsAlive { get { return stat.Hp > 0; } }

    public void OnAttacked(Stat attacker, int _hitpriority = 0)
    {
        int damage = Mathf.Max(0, attacker.TotalAttack - stat.TotalDefense);
        stat.Hp -= damage;

        // ����
        if (stat.Hp > 0)
        {
            // ���� �ǰ� ����� �ǰݻ��� �켱���� ���� ���ų� ���� ��� hit ���·�
            if (stat.TotalHitStatePriority <= _hitpriority)
                stateMachine.ChangeState(Define.State.Hit);

            // Ÿ���� ������ ��Ʈ ������ ��� Ÿ���� �����Ѵ�.
            if (GetController.Target == null)
                GetController.Target = attacker.gameObject;
        }
        // ���
        else
        {
            stateMachine.ChangeState(Define.State.Dead);

            // �÷��̾ ���� ��� ����ġ ����
            if (attacker is PlayerStat)
            {
                PlayerStat playerStat = attacker as PlayerStat;
                playerStat.SetExp(monsterStat.ExpGained);
            }

            DropItem();
        }
    }

    public void OnAttacked(int _damage, int _hitpriority = 0)
    {
        int damage = _damage - stat.Defense;
        stat.Hp -= damage;

        if (stat.Hp > 0)
        {
            // ���� �ǰ� ����� �ǰݻ��� �켱���� ���� ���ų� ���� ��� hit ���·�
            if (stat.HitStatePriority <= _hitpriority)
                stateMachine.ChangeState(Define.State.Hit);
        }
        // ���
        else
        {
            stateMachine.ChangeState(Define.State.Dead);

            DropItem();
        }

    }

    public void OnAttacked(Stat attackerOwner, int _damage, int _hitpriority)
    {
        int damage = _damage - stat.Defense;
        stat.Hp -= damage;

        if (stat.Hp > 0)
        {
            // ���� �ǰ� ����� �ǰݻ��� �켱���� ���� ���ų� ���� ��� hit ���·�
            if (stat.HitStatePriority <= _hitpriority)
                stateMachine.ChangeState(Define.State.Hit);

            // Ÿ���� ������ ��Ʈ ������ ��� Ÿ���� �����Ѵ�.
            if (GetController.Target == null)
                GetController.Target = attackerOwner.gameObject;
        }
        // ���
        else
        {
            stateMachine.ChangeState(Define.State.Dead);

            // �÷��̾ ���� ��� ����ġ ����
            if (attackerOwner is PlayerStat)
            {
                PlayerStat playerStat = attackerOwner as PlayerStat;
                playerStat.SetExp(monsterStat.ExpGained);
            }

            DropItem();
        }
    }
    #endregion

    #region �ݶ��̴� ����
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

    [ServerCallback]
    protected override void InitStateMachine()
    {
        MonsterIdleState m_idleState = new MonsterIdleState(Define.State.Idle, this, controller);
        stateMachine.DefaultState(Define.State.Idle, m_idleState);

        MonsterChaseState m_chaseState = new MonsterChaseState(Define.State.Chase, this, controller);
        stateMachine.RegisterState(Define.State.Chase, m_chaseState);

        MonsterReturnState m_returnState = new MonsterReturnState(Define.State.Return, this, controller);
        stateMachine.RegisterState(Define.State.Return, m_returnState);

        MonsterPatrolState m_patrolState = new MonsterPatrolState(Define.State.Patrol, this, controller);
        stateMachine.RegisterState(Define.State.Patrol, m_patrolState);

        MonsterAttackState m_attackState = new MonsterAttackState(Define.State.NormalAttack, this, controller);
        stateMachine.RegisterState(Define.State.NormalAttack, m_attackState);

        // Skill
        MonsterDissemination m_dissemination = new MonsterDissemination(Define.State.Skill, this, controller, 50, 0.1f, 5.0f, 20.0f);
        stateMachine.RegisterState(Define.State.Skill, m_dissemination);

        HitState m_hitState = new HitState(Define.State.Hit, this, controller);
        stateMachine.RegisterState(Define.State.Hit, m_hitState);

        DeadState m_deadState = new DeadState(Define.State.Dead, this, controller);
        stateMachine.RegisterState(Define.State.Dead, m_deadState);
    }
}
