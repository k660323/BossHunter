using UnityEngine;


public class MonsterChaseState : BaseStateMonster
{
    Collider[] col = new Collider[10];

    // ������ ������Ʈ�� ���� ����� ������Ʈ�� ���󰡰� ���� �ּҰŸ� ����
    float minDistance = float.MaxValue;

    Define.State skillEstate;

    public MonsterChaseState(Define.State _state, Monster _monster, MonsterController _monsterController) : base(_state, _monster, _monsterController)
    {
        
    }

    // ���� ������Ʈ ����
    public override bool CheckCondition()
    {
        MonsterStat stat = MonsterGS.GetMonsterStat;

        // �ݶ��̴� ����
        col.Initialize();

        // ���� ������Ʈ ����
        if (MonsterGS.GetSetPhysics.OverlapSphere(MonsterGS.transform.position, stat.DetectRange, col, stat.EnemyLayer, QueryTriggerInteraction.Collide) > 0)
        {
            GameObject tempTarget = null;
            // �ּ� �Ÿ� �ʱ�ȭ
            minDistance = float.MaxValue;

            // �浹�� ������Ʈ�� �� ������Ʈ���� �ּҰŸ��� ���Ѵ�.
            for (int i = 0; i < col.Length; i++)
            {
                if (col[i] == null)
                    break;

                // �ڱ��ڽ� ��ġ�� �浹ü ��ġ�� �Ÿ��� ����Ͽ� �ִܰŸ� ������Ʈ ĳ��
                float distance = Vector3.SqrMagnitude(MonsterGS.transform.position - col[i].transform.position);
                if (minDistance > distance)
                {
                    minDistance = distance;
                    tempTarget = col[i].transform.gameObject;
                }
            }

            // �ӽ� Ÿ�� ����
            if(tempTarget != null)
            {
                // ���� Ÿ�� ����
                if (MonsterControllerGS.Target != null)
                {
                    // �� ������Ʈ�� ���� ������Ʈ�� �ƴҽ� �˻�
                    if (tempTarget != MonsterControllerGS.Target)
                    {
                        // ���� Ÿ���� �Ÿ�
                        float distance = Vector3.SqrMagnitude(MonsterGS.transform.position - MonsterControllerGS.Target.transform.position);

                        // ������ Ÿ�� �Ÿ����� ª���� ��ü
                        if (minDistance < distance)
                        {
                            // Ÿ�� ���ġ
                            MonsterControllerGS.Target = tempTarget;
                        }
                    }
                }// Ÿ�� ���ġ
                else if (MonsterControllerGS.Target == null)
                {
                    MonsterControllerGS.Target = tempTarget;
                }
            }
        }

        return MonsterControllerGS.Target;
    }

    public override void EnterState()
    {
        // �̵� �ִϸ��̼� ����
        MonsterGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 1.0f);
        // ���� ���� ������ ����
        MonsterControllerGS.ChaseStartPos = MonsterGS.transform.position;

        // Nav �ӵ� ����
        MonsterGS.GetNav.speed = MonsterGS.GetMonsterStat.MoveSpeed;

        // Nav ���� Ȱ��ȭ
        MonsterGS.GetNav.isStopped = false;

        // ���� �����Ҷ� ����� �� �ִ� ��ų ���ϱ�
        skillEstate = MonsterGS.MonsterStateMachine.RandomSelectSkill(MonsterGS.StateMachine.UseableSkill());
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {
        // �̵� �ִϸ��̼� ����
        MonsterGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 0.0f);

        // Nav ���� ��Ȱ��ȭ
        MonsterGS.GetNav.isStopped = true;
    }

    public override void FixedUpdateState()
    {
        // Ÿ�� �˻�
        if (CheckCondition())
        {
            MonsterStat stat = MonsterGS.GetMonsterStat;
            Vector3 curPos = MonsterGS.transform.position;

            // ���� ��ġ�� ���� ��ġ�� ���̰� �ִ� ���������� ���� ������ ��� �����Ѵ�.
            if (Vector3.Distance(MonsterControllerGS.SpawnPos, curPos) <= stat.ChaseDistance)
            {
                // ����� �� �ִ� ��ų�� Ư�� Ȯ���� ��ų�� �����´�. (���� ���� ����)
                // ���� �õ� ���н�
                if (MonsterGS.StateMachine.ChangeState(skillEstate) == false && MonsterGS.StateMachine.ChangeState(Define.State.NormalAttack) == false)
                {
                    Weapon weapon = creatureGS.GetEquipment.EquipWeapon;
                    Vector3 targetPos = MonsterControllerGS.Target.transform.position;
                    float distance = Vector3.Distance(curPos, targetPos);

                    // ���� ������ ���� ��Ÿ���� ���� ���� ��Ÿ� ���̸�
                    if (weapon != null && weapon.IsAttackable() == false && distance <= creatureGS.GetStat.AtkDistance)
                    {
                        MonsterGS.StateMachine.ChangeState(Define.State.Idle);
                    }
                    else
                    {
                        // Nav �ӵ� ����
                        MonsterGS.GetNav.speed = stat.MoveSpeed;

                        // ������ ����
                        MonsterGS.GetNav.SetDestination(MonsterControllerGS.Target.transform.position);

                        // Nav ���� Ȱ��ȭ
                        MonsterGS.GetNav.isStopped = false;
                    }
                }
                return;
            }
        }

        // Ÿ���� ���ų� ���� ������ ����� ���� �ڸ��� ���ư���.
        MonsterGS.StateMachine.ChangeState(Define.State.Return);
    }

    public override void UpdateState()
    {
       
    }
}
