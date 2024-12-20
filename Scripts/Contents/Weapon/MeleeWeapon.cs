using Mirror;
using System.Collections.Generic;
using UnityEngine;

public abstract class MeleeWeapon : Weapon
{
    // 중복 공격 처리 방지 해시셋
    HashSet<GameObject> hitObjects = new HashSet<GameObject>();

    public override void Init()
    {
        weaponType = Define.WeaponType.Melee;
    }

    [Command(requiresAuthority = false)]
    public override void CTS_StartAttack(Define.State state)
    {
        ComboCount++;
        CurHitCount = 0;
        if (checkAttackReInputCor != null)
            StopCoroutine(checkAttackReInputCor);
    }

    [Command(requiresAuthority = false)]
    public override void CTS_EndAttack(Define.State state)
    {
        // 공격 판정 중지
        isAttack = false;
        // 히트 오브젝트 초기화
        hitObjects.Clear();
        cols.Initialize();
        if (IsComboable())
        {
            CheckAttackReInput(CanReInputTime);
        }
        else
        {
            if (checkAttackReInputCor != null)
                StopCoroutine(checkAttackReInputCor);
            CheckCoolTime(AttackCoolTime);
        }
    }

    [Command(requiresAuthority = false)]
    public override void CTS_ChargingAttack(Define.State state)
    {

    }

    [Command(requiresAuthority = false)]
    public override void CTS_DashAttack(Define.State state)
    {

    }

    [Command(requiresAuthority = false)]
    public override void CTS_Skill(Define.State state)
    {

    }

    [Command(requiresAuthority = false)]
    public override void CTS_UltimateSkill(Define.State state)
    {

    }

    // 공격 판정 여기서
    [ServerCallback]

    public void FixedUpdate()
    {
        // 공격 flag가 켜져있으면 공격 판정 시작
        if (IsAttack && CurHitCount < MaxHitCount)
        {
            Vector3 half = AttackSize * 0.5f;

            Stat stat = creature.GetStat;

            // 물리 검사  // Physics클래스가 동적 배열을 지원하지 않아 정적 배열 사용
            if (creature.GetSetPhysics.OverlapBox(creature.normalAtkPos.position, half, cols, creature.transform.rotation, stat.EnemyLayer) > 0)
            {
                // 히트한 오브젝트 검사
                for (int i = 0; i < cols.Length; i++)
                {
                    if (cols[i] == null)
                        break;
                    // 중복 히트 처리 방지
                    if (hitObjects.Contains(cols[i].gameObject))
                        continue;

                    if (cols[i].TryGetComponent(out IHitable hitable))
                    {
                        // 오브젝트가 살아있으면 목록에 추가
                        if (hitable.IsAlive)
                        {
                            // 공격
                            hitable.OnAttacked(stat, stat.TotalHitStatePriority);
                            // 공격 HashSet 추가
                            hitObjects.Add(cols[i].gameObject);

                            CurHitCount++;
                            if (CurHitCount == MaxHitCount)
                                return;
                        }
                    }
                }
            }
        }
    }
}
