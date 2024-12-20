using Data;
using Mirror;
using UnityEngine;

public abstract class RangedWeapon : Weapon
{
    [SyncVar]
    protected string spawnProjectile;
    [SyncVar]
    protected float _projectileMoveSpeed;
    [SyncVar]
    protected float _lifeTime;

    // �ǰ� �ݶ��̴� ũ��
    [SerializeField]
    protected string d_spawnProjectile;
    // ��Ÿ��
    [SerializeField]
    protected float d_moveSpeed;
    // �ִ� Ÿ�� ī��Ʈ
    [SerializeField]
    protected float d_lifeTime;

    public override void Init()
    {
        weaponType = Define.WeaponType.Range;
    }

    public override void WeaponStatInit()
    {
        base.WeaponStatInit();
        spawnProjectile = d_spawnProjectile;
        _projectileMoveSpeed = d_moveSpeed;
        _lifeTime = d_lifeTime;
    }

    public virtual void RangeWeaponStatSet(RangeWeaponItemStat rangedWeaponItemStat)
    {
        spawnProjectile = rangedWeaponItemStat._spawnProjectile;
        _projectileMoveSpeed = rangedWeaponItemStat._projectileMoveSpeed;
        _lifeTime = rangedWeaponItemStat._lifeTime;
    }

    [Command(requiresAuthority = false)]
    public override void CTS_StartAttack(Define.State state)
    {
        ComboCount++;

        if (checkAttackReInputCor != null)
            StopCoroutine(checkAttackReInputCor);
    }

    [Command(requiresAuthority = false)]
    public override void CTS_EndAttack(Define.State state)
    {
        isAttack = false;

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

    [Command(requiresAuthority = false)]
    public virtual void ProjectileFire(Vector3 spawnPos)
    {
        GameObject fireObj = Managers.Resource.NetInstantiate(spawnProjectile, gameObject.scene, true);
        if(fireObj.TryGetComponent(out Projectile projectile) == false)
        {
            Debug.LogWarning("�ش� ������Ʈ�� ����ü ������Ʈ�� �����ϴ�. �߰����ּ���.");
            return;
        }

        projectile.SetProjectileInfo(spawnPos, creature.transform.forward, _projectileMoveSpeed, creature.GetStat.TotalAttack, _lifeTime, creature.GetStat.TotalHitStatePriority, creature.GetStat.EnemyLayer, 1, creature);
    }
}
