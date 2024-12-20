using UnityEngine;


public class MonsterChaseState : BaseStateMonster
{
    Collider[] col = new Collider[10];

    // 감지된 오브젝트중 제일 가까운 오브젝트를 따라가게 만들 최소거리 변수
    float minDistance = float.MaxValue;

    Define.State skillEstate;

    public MonsterChaseState(Define.State _state, Monster _monster, MonsterController _monsterController) : base(_state, _monster, _monsterController)
    {
        
    }

    // 적대 오브젝트 감지
    public override bool CheckCondition()
    {
        MonsterStat stat = MonsterGS.GetMonsterStat;

        // 콜라이더 정리
        col.Initialize();

        // 적대 오브젝트 감지
        if (MonsterGS.GetSetPhysics.OverlapSphere(MonsterGS.transform.position, stat.DetectRange, col, stat.EnemyLayer, QueryTriggerInteraction.Collide) > 0)
        {
            GameObject tempTarget = null;
            // 최소 거리 초기화
            minDistance = float.MaxValue;

            // 충돌된 오브젝트와 그 오브젝트와의 최소거리를 구한다.
            for (int i = 0; i < col.Length; i++)
            {
                if (col[i] == null)
                    break;

                // 자기자신 위치와 충돌체 위치의 거리를 계산하여 최단거리 오브젝트 캐싱
                float distance = Vector3.SqrMagnitude(MonsterGS.transform.position - col[i].transform.position);
                if (minDistance > distance)
                {
                    minDistance = distance;
                    tempTarget = col[i].transform.gameObject;
                }
            }

            // 임시 타겟 존재
            if(tempTarget != null)
            {
                // 몬스터 타겟 존재
                if (MonsterControllerGS.Target != null)
                {
                    // 두 오브젝트가 같은 오브젝트가 아닐시 검사
                    if (tempTarget != MonsterControllerGS.Target)
                    {
                        // 현재 타겟의 거리
                        float distance = Vector3.SqrMagnitude(MonsterGS.transform.position - MonsterControllerGS.Target.transform.position);

                        // 기존의 타겟 거리보다 짧으면 교체
                        if (minDistance < distance)
                        {
                            // 타게 재배치
                            MonsterControllerGS.Target = tempTarget;
                        }
                    }
                }// 타겟 재배치
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
        // 이동 애니메이션 설정
        MonsterGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 1.0f);
        // 추적 시작 포지션 설정
        MonsterControllerGS.ChaseStartPos = MonsterGS.transform.position;

        // Nav 속도 설정
        MonsterGS.GetNav.speed = MonsterGS.GetMonsterStat.MoveSpeed;

        // Nav 추적 활성화
        MonsterGS.GetNav.isStopped = false;

        // 추적 시작할때 사용할 수 있는 스킬 정하기
        skillEstate = MonsterGS.MonsterStateMachine.RandomSelectSkill(MonsterGS.StateMachine.UseableSkill());
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
        // 타겟 검사
        if (CheckCondition())
        {
            MonsterStat stat = MonsterGS.GetMonsterStat;
            Vector3 curPos = MonsterGS.transform.position;

            // 스폰 위치와 현재 위치의 차이가 최대 추적범위를 넘지 않으면 계속 추적한다.
            if (Vector3.Distance(MonsterControllerGS.SpawnPos, curPos) <= stat.ChaseDistance)
            {
                // 사용할 수 있는 스킬중 특정 확률로 스킬을 가져온다. (없을 수도 있음)
                // 공격 시도 실패시
                if (MonsterGS.StateMachine.ChangeState(skillEstate) == false && MonsterGS.StateMachine.ChangeState(Define.State.NormalAttack) == false)
                {
                    Weapon weapon = creatureGS.GetEquipment.EquipWeapon;
                    Vector3 targetPos = MonsterControllerGS.Target.transform.position;
                    float distance = Vector3.Distance(curPos, targetPos);

                    // 실패 이유가 공격 쿨타임일 경우와 공격 사거리 안이면
                    if (weapon != null && weapon.IsAttackable() == false && distance <= creatureGS.GetStat.AtkDistance)
                    {
                        MonsterGS.StateMachine.ChangeState(Define.State.Idle);
                    }
                    else
                    {
                        // Nav 속도 설정
                        MonsterGS.GetNav.speed = stat.MoveSpeed;

                        // 목적지 갱신
                        MonsterGS.GetNav.SetDestination(MonsterControllerGS.Target.transform.position);

                        // Nav 추적 활성화
                        MonsterGS.GetNav.isStopped = false;
                    }
                }
                return;
            }
        }

        // 타겟이 없거나 추적 범위를 벗어나면 원래 자리로 돌아간다.
        MonsterGS.StateMachine.ChangeState(Define.State.Return);
    }

    public override void UpdateState()
    {
       
    }
}
