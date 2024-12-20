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
                    // 서버 객체면 서버에서 갱신
                    creature.GetNetAnim.animator.SetInteger(Managers.AnimHash.IAttackCombo, value);
                }
                else
                {
                    // 플레이어 객체면 RPC
                    OnComboCountChanged(connectionToClient, value);
                }
            }
        } 
    }
    public int MaxComboCount { get { return maxComboCount; } }
    public int CurHitCount { get { return curHitCount; } protected set { curHitCount = value; } }
    public int MaxHitCount { get { return maxHitCount; } protected set { maxHitCount = value; } }

    #region 무기 정보
    // 피격 판정 가능한 상태인가
    [SyncVar]
    protected bool isAttack;
    // 현재 쿨타임 인가?
    [SyncVar]
    protected bool isCoolTime;
    // 콤보 재입력 시간
    public const float CanReInputTime = 1.0f;

    // 콤보 사용시 콤보 카운터와 콤보 쿨타임 사용
    [SyncVar]
    protected int comboCount = 0;
    [SerializeField, SyncVar]
    protected int maxComboCount = 1;

    // 피격 콜라이더 크기
    [SyncVar]
    protected Vector3 attackSize;
    // 쿨타임
    [SerializeField, SyncVar]
    protected float attackCoolTime;
    // 현재 타격 카운트
    protected int curHitCount = 0;
    // 최대 타격 카운트
    [SyncVar]
    protected int maxHitCount = 1;
    // 한번에 충돌 객체를 가져올수 있는 콜라이더 수
    protected Collider[] cols = new Collider[5];

    // 피격 콜라이더 크기
    [SerializeField]
    protected Vector3 d_attackSize;
    // 쿨타임
    [SerializeField]
    protected float d_attackCoolTime;
    // 최대 타격 카운트
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

    // 기본 공격
    public abstract void CTS_StartAttack(Define.State state);
    // 기본 공격 끝
    public abstract void CTS_EndAttack(Define.State state);
    // 대쉬 공격
    public abstract void CTS_DashAttack(Define.State state);
    // 차지 공격
    public abstract void CTS_ChargingAttack(Define.State state);
    // 스킬
    public abstract void CTS_Skill(Define.State state);
    // 궁극기
    public abstract void CTS_UltimateSkill(Define.State state);


    public void CheckAttackReInput(float reInputTime)
    {
        if (checkAttackReInputCor != null)
            StopCoroutine(checkAttackReInputCor);
        checkAttackReInputCor = StartCoroutine(CheckAttackReInputCoroutine(reInputTime));
    }

    // 일정 시간 동안 입력이 없으면 쿨타입 적용
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
