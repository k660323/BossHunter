using System.Collections;
using UnityEngine;

public class MonsterDissemination : BaseStateMonster, IMonsterSkill
{
    float percentage;
    float mpRequirement;
    float skillDistance;
    float skillCoolTime;
    bool isCool;

    public float Percentage { get => percentage; set => percentage = value; }
    public float MpRequirement { get => mpRequirement;  set => mpRequirement = value; }
    public float SkillDistance {  get => skillDistance; set => skillDistance = value; }
    public float SkillCoolTime { get => skillCoolTime; set => skillCoolTime = value; }
    public bool IsCool { get => IsCool; }

    Quaternion quaternion;
    float creationCycle = 0.5f;
    float curTime = 0.0f;

    public MonsterDissemination(Define.State _state, Monster _monster, MonsterController _monsterController,float _mpRequirement, float _percentage, float _skillDistance, float _skillCoolTime) : base(_state, _monster, _monsterController)
    {
        mpRequirement = _mpRequirement;
        percentage = _percentage;
        skillDistance = _skillDistance;
        skillCoolTime = _skillCoolTime;
    }

    public override bool CheckCondition()
    {
        // 타겟 없으면 스킵
        if (MonsterControllerGS.Target == null)
            return false;

        // 스킬 시전 범위 밖이면 스킵
        float distance = Vector3.Distance(creatureGS.transform.position, MonsterControllerGS.Target.transform.position);
        if (distance > SkillDistance)
            return false;

        // 마나 소비량
        if (creatureGS.GetStat.Mp - mpRequirement < 0)
            return false;

        // 스킬 쿨타임
        if(isCool)
            return false;

        // 스킬 발동 확률
        float per = Random.Range(0.0f, 1.0f);
        return per <= Percentage;
    }

    public override void EnterState()
    {
        // 마나 소모
        creatureGS.GetStat.Mp -= (int)mpRequirement;

        // 스킬 애니메이션 설정
        MonsterGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BDissemination, true);
        // Nav 추적 비활성화
        MonsterGS.GetNav.isStopped = true;
        // 현재 방향
        quaternion = MonsterGS.transform.rotation;
        // 생성 쿨
        curTime = creationCycle;
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {
        // 스킬 애니메이션 설정
        MonsterGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BDissemination, false);
        MonsterGS.StartCoroutine(CoolTime());

    }

    public override void FixedUpdateState()
    {
        // 여기서 씨앗 생성
        curTime -= Time.fixedDeltaTime;
        if (curTime <= 0.0f)
        {
            GameObject go = Managers.Resource.NetInstantiate("BoomSeeds", MonsterGS.gameObject.scene, true);
            if (go.TryGetComponent(out BombSeeds bombSeeds))
            {
                Vector3 spawnPos = MonsterGS.transform.position;
                if (MonsterGS is IColliderInfo info)
                    spawnPos.Set(spawnPos.x, spawnPos.y + info.GetHeight(), spawnPos.z);

                bombSeeds.SetBoomInfo(spawnPos, quaternion, Vector3.zero, 5.0f, 8.0f, 0.0f, 10, 15.0f, 5, Managers.LayerManager.Player, 1, MonsterGS);
            }

            // 생성쿨 초기화
            curTime = creationCycle;
            // 30 도씩 회전
            quaternion *= Quaternion.Euler(0, 45, 0);
        }
    }

    public override void UpdateState()
    {
       
    }

   IEnumerator CoolTime()
    {
        isCool = true;
        yield return new WaitForSeconds(skillCoolTime);
        isCool = false;
    }
}
