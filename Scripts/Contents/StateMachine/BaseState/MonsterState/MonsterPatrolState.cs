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
        // 패트롤 위치 설정 // 설정 성공시 true 설정 실패시 false
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

        // 이동 애니메이션 설정
        MonsterGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 1.0f);
        // Nav 속도 설정
        MonsterGS.GetNav.speed = MonsterGS.GetMonsterStat.MoveSpeed;
        // 목적지 설정
        MonsterGS.GetNav.SetDestination(patrolPos);

        // Nav 추적 활성화
        MonsterGS.GetNav.isStopped = false;
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {
        // 이동 애니메이션 설정
        MonsterGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 0.0f);
        // Nav 추적 비활성화
        MonsterGS.GetNav.isStopped = true;
    }

    public override void FixedUpdateState()
    {
        // Nav 속도 설정
        MonsterGS.GetNav.speed = MonsterGS.GetMonsterStat.MoveSpeed;

        if (MonsterGS.GetNav.remainingDistance < 0.1f)
        {
            MonsterGS.transform.position = patrolPos;
            MonsterGS.StateMachine.ChangeState(Define.State.Idle);
        }
        // 도중에 플레어가 있는지 확인 (미구현)
        else
        {

        }
    }

    public override void UpdateState()
    {
      
    }
}
