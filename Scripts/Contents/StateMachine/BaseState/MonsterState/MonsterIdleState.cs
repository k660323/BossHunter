using UnityEngine;

public class MonsterIdleState : BaseStateMonster
{
    // ��Ʈ�� ��� �ð�
    float patrolWaitTime;

    // ��� ��ų ENUM
    Define.State skillEstate;

    public MonsterIdleState(Define.State _state, Monster _monster, MonsterController _monsterController) : base(_state, _monster, _monsterController)
    {
        
    }

    public override void EnterState()
    {
        MonsterGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 0.0f);

        // ��Ʈ�� ��� �ð��� �������� �����Ѵ�.
        patrolWaitTime = Random.Range(3.0f, 10.0f);

        // ���̵� ���� ���Խ� ����� �� �ִ� ��ų �ϳ� ���ϱ�
        skillEstate = MonsterGS.MonsterStateMachine.RandomSelectSkill(MonsterGS.StateMachine.UseableSkill());
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {
       
    }

    public override void FixedUpdateState()
    {
        // ���� ����� �ִ� ���
        if (MonsterControllerGS.Target != null)
        {
            // ����� �ٶ󺸱�
            MonsterControllerGS.LookAt(MonsterControllerGS.Target.transform);

            // ��ų ��� �����ϸ� ����
            if (MonsterGS.StateMachine.ChangeState(skillEstate))
                return;

            Vector3 curPos = MonsterGS.transform.position;
            Vector3 targetPos = MonsterControllerGS.Target.transform.position;
            float distance = Vector3.Distance(curPos, targetPos);

            // �Ÿ� �Ǻ�
            // ���� ��Ÿ� ���̸�
            if (distance <= creatureGS.GetStat.AtkDistance)
            {
                Weapon weapon = creatureGS.GetEquipment.EquipWeapon;
                // ���� ���̸� ����
                if (weapon.IsAttackable() == false)
                    return;

                // ���� �����ϸ� ����
                if (MonsterGS.StateMachine.ChangeState(Define.State.NormalAttack))
                    return;
            }
        }

        // ���� ��Ÿ� ���̸� Ÿ�� �� ���� �� ���� ����
        // ���� ���� Raycast
        if (MonsterGS.StateMachine.ChangeState(Define.State.Chase) == false)
        {
            // ���� �ð��� ��Ʈ��
            patrolWaitTime -= Time.fixedDeltaTime;
            if (patrolWaitTime <= 0.0f)
            {
                // ��Ʈ�� ��ȯ ���н� �ٽ� �ð� ����
                if (MonsterGS.StateMachine.ChangeState(Define.State.Patrol) == false)
                {
                    // ��Ʈ�� ��� �ð��� �������� �����Ѵ�.
                    patrolWaitTime = Random.Range(5.0f, 10.0f);
                }
            }
        }
    }

    public override void UpdateState()
    {
        
    }
}
