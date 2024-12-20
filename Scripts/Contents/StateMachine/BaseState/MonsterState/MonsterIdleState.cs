using UnityEngine;

public class MonsterIdleState : BaseStateMonster
{
    // 패트롤 대기 시간
    float patrolWaitTime;

    // 대상 스킬 ENUM
    Define.State skillEstate;

    public MonsterIdleState(Define.State _state, Monster _monster, MonsterController _monsterController) : base(_state, _monster, _monsterController)
    {
        
    }

    public override void EnterState()
    {
        MonsterGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 0.0f);

        // 패트롤 대기 시간을 랜덤으로 설정한다.
        patrolWaitTime = Random.Range(3.0f, 10.0f);

        // 아이들 상태 진입시 사용할 수 있는 스킬 하나 정하기
        skillEstate = MonsterGS.MonsterStateMachine.RandomSelectSkill(MonsterGS.StateMachine.UseableSkill());
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {
       
    }

    public override void FixedUpdateState()
    {
        // 공격 대상이 있는 경우
        if (MonsterControllerGS.Target != null)
        {
            // 대상을 바라보기
            MonsterControllerGS.LookAt(MonsterControllerGS.Target.transform);

            // 스킬 사용 가능하면 리턴
            if (MonsterGS.StateMachine.ChangeState(skillEstate))
                return;

            Vector3 curPos = MonsterGS.transform.position;
            Vector3 targetPos = MonsterControllerGS.Target.transform.position;
            float distance = Vector3.Distance(curPos, targetPos);

            // 거리 판별
            // 공격 사거리 안이면
            if (distance <= creatureGS.GetStat.AtkDistance)
            {
                Weapon weapon = creatureGS.GetEquipment.EquipWeapon;
                // 공격 쿨이면 리턴
                if (weapon.IsAttackable() == false)
                    return;

                // 공격 가능하면 리턴
                if (MonsterGS.StateMachine.ChangeState(Define.State.NormalAttack))
                    return;
            }
        }

        // 공격 사거리 밖이면 타겟 재 감지 및 추적 시작
        // 적대 감지 Raycast
        if (MonsterGS.StateMachine.ChangeState(Define.State.Chase) == false)
        {
            // 일정 시간후 패트롤
            patrolWaitTime -= Time.fixedDeltaTime;
            if (patrolWaitTime <= 0.0f)
            {
                // 패트롤 전환 실패시 다시 시간 설정
                if (MonsterGS.StateMachine.ChangeState(Define.State.Patrol) == false)
                {
                    // 패트롤 대기 시간을 랜덤으로 설정한다.
                    patrolWaitTime = Random.Range(5.0f, 10.0f);
                }
            }
        }
    }

    public override void UpdateState()
    {
        
    }
}
