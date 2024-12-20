using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPatrolState : BaseStateMonster
{
    RaycastHit hit;
    Vector3 patrolPos;
    public MonsterPatrolState(Define.State _state, Monster _monster, MonsterController _monsterController) : base(_state, _monster, _monsterController)
    {

    }

    public override bool CheckCondition()
    {
        // ��Ʈ�� ��ġ ���� // ���� ������ true ���� ���н� false
        float patrolDistance = MonsterGS.GetMonsterStat.PatrolDistance;
        patrolPos.Set(MonsterControllerGS.SpawnPos.x + Random.Range(-patrolDistance, patrolDistance), MonsterControllerGS.SpawnPos.y, MonsterControllerGS.SpawnPos.z + Random.Range(-patrolDistance, patrolDistance));
        bool result = false;
        if(MonsterGS.GetSetPhysics.Raycast(patrolPos + Vector3.up * 10, Vector3.down, out hit, float.PositiveInfinity, 1 << 7, QueryTriggerInteraction.Ignore))
        {
            result = MonsterGS.GetNav.CalculatePath(patrolPos, MonsterGS.GetNav.path);
        }
          
        return result;
    }

    public override void EnterState()
    {
        patrolPos = hit.point;

        // �̵� �ִϸ��̼� ����
        MonsterGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 1.0f);
        // Nav �ӵ� ����
        MonsterGS.GetNav.speed = MonsterGS.GetMonsterStat.MoveSpeed;
        // ������ ����
        MonsterGS.GetNav.SetDestination(patrolPos);

        // Nav ���� Ȱ��ȭ
        MonsterGS.GetNav.isStopped = false;
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
        // Nav �ӵ� ����
        MonsterGS.GetNav.speed = MonsterGS.GetMonsterStat.MoveSpeed;

        if (MonsterGS.GetNav.remainingDistance < 0.1f)
        {
            MonsterGS.transform.position = patrolPos;
            MonsterGS.StateMachine.ChangeState(Define.State.Idle);
        }
        // ���߿� �÷�� �ִ��� Ȯ�� (�̱���)
        else
        {

        }
    }

    public override void UpdateState()
    {
      
    }
}
