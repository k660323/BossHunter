using Mirror.Examples.CCU;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterReturnState : BaseStateMonster
{
    public MonsterReturnState(Define.State _state, Monster _monster, MonsterController _monsterController) : base(_state, _monster, _monsterController)
    {
    }

    public override void EnterState()
    {
        // 이동 애니메이션 설정
        MonsterGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 1.0f);

        // Nav 속도 설정
        MonsterGS.GetNav.speed = MonsterGS.GetMonsterStat.MoveSpeed;

        // 목적지 갱신
        MonsterGS.GetNav.SetDestination(MonsterControllerGS.SpawnPos);

        // Nav 추적 활성화
        MonsterGS.GetNav.isStopped = false;

        // 추적 대상 초기화
        MonsterControllerGS.Target = null;
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {
        // 이동 애니메이션 설정
        MonsterGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 0.0f);

        // Nav 추적 비활성화
        MonsterGS.GetNav.isStopped = true;

        // 추적 종료후 풀피로 세팅
        MonsterGS.GetMonsterStat.Hp = MonsterGS.GetMonsterStat.MaxHp;
    }

    public override void FixedUpdateState()
    {
        // Nav 속도 설정
        MonsterGS.GetNav.speed = MonsterGS.GetMonsterStat.MoveSpeed;

        // 스폰 시작 위치와 현재 위치의 차이가 0.1f 이하면 아이들로
        if (MonsterGS.GetNav.remainingDistance < 0.1f)
        {
            // 오차범위 제거하기위해 스폰 위치 설정
            MonsterGS.transform.position = MonsterControllerGS.SpawnPos;
            MonsterGS.StateMachine.ChangeState(Define.State.Idle);
        }
    }

    public override void UpdateState()
    {
       
    }
}
