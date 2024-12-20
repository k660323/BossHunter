using Data;
using Mirror;
using System.Collections;
using UnityEngine;

public abstract class Weapon : NetworkBehaviour
{
    [SerializeField]
    protected Define.WeaponType weaponType;

    [HideInInspector]
    public Creature creature;

    public Coroutine checkAttackReInputCor;

    public bool IsAttack { get { return isAttack; } set { isAttack = value; } }
    public Vector3 AttackSize { get { return attackSize; } protected set { attackSize = value; } }
    public bool IsCoolTime { get { return isCoolTime; } set { isCoolTime = value; } }
    public float AttackCoolTime { get { return attackCoolTime; } protected set { attackCoolTime = value; } }
    public int ComboCount 
    { 
        get { return comboCount; } 
        protected set 
        { 
            if (value > MaxComboCount) 
                value = 1; 
            comboCount = value; 

            if(isServer)
            {
                if(connectionToClient == null)
                {
                    // ���� ��ü�� �������� ����
                    creature.GetNetAnim.animator.SetInteger(Managers.AnimHash.IAttackCombo, value);
                }
                else
                {
                    // �÷��̾� ��ü�� RPC
                    OnComboCountChanged(connectionToClient, value);
                }
            }
        } 
    }
    public int MaxComboCount { get { return maxComboCount; } }
    public int CurHitCount { get { return curHitCount; } protected set { curHitCount = value; } }
    public int MaxHitCount { get { return maxHitCount; } protected set { maxHitCount = value; } }

    #region ���� ����
    // �ǰ� ���� ������ �����ΰ�
    [SyncVar]
    protected bool isAttack;
    // ���� ��Ÿ�� �ΰ�?
    [SyncVar]
    protected bool isCoolTime;
    // �޺� ���Է� �ð�
    public const float CanReInputTime = 1.0f;

    // �޺� ���� �޺� ī���Ϳ� �޺� ��Ÿ�� ���
    [SyncVar]
    protected int comboCount = 0;
    [SerializeField, SyncVar]
    protected int maxComboCount = 1;

    // �ǰ� �ݶ��̴� ũ��
    [SyncVar]
    protected Vector3 attackSize;
    // ��Ÿ��
    [SerializeField, SyncVar]
    protected float attackCoolTime;
    // ���� Ÿ�� ī��Ʈ
    protected int curHitCount = 0;
    // �ִ� Ÿ�� ī��Ʈ
    [SyncVar]
    protected int maxHitCount = 1;
    // �ѹ��� �浹 ��ü�� �����ü� �ִ� �ݶ��̴� ��
    protected Collider[] cols = new Collider[5];

    // �ǰ� �ݶ��̴� ũ��
    [SerializeField]
    protected Vector3 d_attackSize;
    // ��Ÿ��
    [SerializeField]
    protected float d_attackCoolTime;
    // �ִ� Ÿ�� ī��Ʈ
    [SerializeField]
    protected int d_maxHitCount = 1;
    #endregion

   
    [TargetRpc]
    public void OnComboCountChanged(NetworkConnection conn, int value)
    {
        creature.GetNetAnim.animator.SetInteger(Managers.AnimHash.IAttackCombo, value);
    }

    private void Awake()
    {
        creature = GetComponentInParent<Creature>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Init();
        WeaponStatInit();
    }

    public abstract void Init();

    public virtual void WeaponStatInit()
    {
        AttackSize = d_attackSize;
        AttackCoolTime = d_attackCoolTime;
        MaxHitCount = d_maxHitCount;
    }

    public virtual void WeaponStatSet(WeaponItemStat weaponItemStat)
    {
        AttackSize = weaponItemStat._attackSize;
        AttackCoolTime = weaponItemStat._attackCoolTime;
        MaxHitCount = weaponItemStat._maxHitCount;
    }

    // �⺻ ����
    public abstract void CTS_StartAttack(Define.State state);
    // �⺻ ���� ��
    public abstract void CTS_EndAttack(Define.State state);
    // �뽬 ����
    public abstract void CTS_DashAttack(Define.State state);
    // ���� ����
    public abstract void CTS_ChargingAttack(Define.State state);
    // ��ų
    public abstract void CTS_Skill(Define.State state);
    // �ñر�
    public abstract void CTS_UltimateSkill(Define.State state);


    public void CheckAttackReInput(float reInputTime)
    {
        if (checkAttackReInputCor != null)
            StopCoroutine(checkAttackReInputCor);
        checkAttackReInputCor = StartCoroutine(CheckAttackReInputCoroutine(reInputTime));
    }

    // ���� �ð� ���� �Է��� ������ ��Ÿ�� ����
    IEnumerator CheckAttackReInputCoroutine(float reInputTime)
    {
        float currentTime = 0.0f;
        while (true)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= reInputTime)
                break;
            yield return null;
        }

        CheckCoolTime(AttackCoolTime);
    }

    public void CheckCoolTime(float coolTime)
    {
        StartCoroutine(CoolTimeStart(coolTime));
    }

    public IEnumerator CoolTimeStart(float time)
    {
        IsCoolTime = true;
        ComboCount = 0;
        float currentTime = 0.0f;
        while (true)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= time)
                break;
            yield return null;
        }
        IsCoolTime = false;
        creature.GetNetAnim.animator.SetInteger(Managers.AnimHash.IAttackCombo, 0);
    }

    public virtual bool IsAttackable()
    {
        return (!IsCoolTime) && IsComboable();
    }

    public bool IsComboable()
    {
        return ComboCount < MaxComboCount;
    }

    public bool IsLastComboToFirstCombo()
    {
        return (attackCoolTime == 0) && (ComboCount == MaxComboCount);
    }
}
