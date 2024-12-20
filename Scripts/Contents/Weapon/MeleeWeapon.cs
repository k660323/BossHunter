using Mirror;
using System.Collections.Generic;
using UnityEngine;

public abstract class MeleeWeapon : Weapon
{
    // �ߺ� ���� ó�� ���� �ؽü�
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
        // ���� ���� ����
        isAttack = false;
        // ��Ʈ ������Ʈ �ʱ�ȭ
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

    // ���� ���� ���⼭
    [ServerCallback]

    public void FixedUpdate()
    {
        // ���� flag�� ���������� ���� ���� ����
        if (IsAttack && CurHitCount < MaxHitCount)
        {
            Vector3 half = AttackSize * 0.5f;

            Stat stat = creature.GetStat;

            // ���� �˻�  // PhysicsŬ������ ���� �迭�� �������� �ʾ� ���� �迭 ���
            if (creature.GetSetPhysics.OverlapBox(creature.normalAtkPos.position, half, cols, creature.transform.rotation, stat.EnemyLayer) > 0)
            {
                // ��Ʈ�� ������Ʈ �˻�
                for (int i = 0; i < cols.Length; i++)
                {
                    if (cols[i] == null)
                        break;
                    // �ߺ� ��Ʈ ó�� ����
                    if (hitObjects.Contains(cols[i].gameObject))
                        continue;

                    if (cols[i].TryGetComponent(out IHitable hitable))
                    {
                        // ������Ʈ�� ��������� ��Ͽ� �߰�
                        if (hitable.IsAlive)
                        {
                            // ����
                            hitable.OnAttacked(stat, stat.TotalHitStatePriority);
                            // ���� HashSet �߰�
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
