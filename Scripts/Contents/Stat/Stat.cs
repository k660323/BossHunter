using UnityEngine;
using Mirror;
using System;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;

public class Stat : NetworkBehaviour
{
    static WaitForSeconds recoveryWait = new WaitForSeconds(10.0f);

    [SerializeField, SyncVar(hook = nameof(OnChangedCreatureName))]
    protected string _creatureName;
    [SerializeField, SyncVar(hook = nameof(OnChangedLevel))]
    protected int _level;
    [SerializeField, SyncVar(hook = nameof(OnChangedHp))]
    protected int _hp;
    [SerializeField, SyncVar(hook = nameof(OnChangedHp))]
    protected int _maxHp;
    [SerializeField, SyncVar(hook = nameof(OnChangedHp))]
    protected int _extraMaxHp;

    [SerializeField, SyncVar(hook = nameof(OnChangedHpRecovery))]
    protected int _hpRecoveryAmount;
    [SerializeField, SyncVar(hook = nameof(OnChangedHpRecovery))]
    protected int _extraHpRecoveryAmount;

    [SerializeField, SyncVar(hook = nameof(OnChangedMp))]
    protected int _mp;
    [SerializeField, SyncVar(hook = nameof(OnChangedMp))]
    protected int _maxMp;
    [SerializeField, SyncVar(hook = nameof(OnChangedMp))]
    protected int _extraMaxMp;

    [SerializeField, SyncVar(hook = nameof(OnChangedMpRecovery))]
    protected int _mpRecoveryAmount;
    [SerializeField, SyncVar(hook = nameof(OnChangedMpRecovery))]
    protected int _extraMpRecoveryAmount;

    [SerializeField, SyncVar(hook = nameof(OnChangedAttack))]
    protected int _attack;
    [SerializeField, SyncVar(hook = nameof(OnChangedAttack))]
    protected int _extraAttack;

    [SerializeField, SyncVar(hook = nameof(OnChangedAttackSpeed))]
    protected float _atkSpeed;
    [SerializeField, SyncVar(hook = nameof(OnChangedAttackSpeed))]
    protected float _extraAtkSpeed;

    [SerializeField, SyncVar(hook = nameof(OnChangedDefense))]
    protected int _defense;
    [SerializeField, SyncVar(hook = nameof(OnChangedDefense))]
    protected int _extraDefense;

    [SerializeField, SyncVar(hook = nameof(OnChangedMoveSpeed))]
    protected float _moveSpeed;
    [SerializeField, SyncVar(hook = nameof(OnChangedMoveSpeed))]
    protected float _extraMoveSpeed;

    [SerializeField, SyncVar(hook = nameof(OnChangedHSPriority))]
    protected int _hitStatePriority;
    [SerializeField, SyncVar(hook = nameof(OnChangedHSPriority))]
    protected int _extraHitStatePriority;

    [SerializeField, SyncVar(hook = nameof(OnChangedAtkDistance))]
    protected float _atkDistance;
    [SerializeField, SyncVar(hook = nameof(OnChangedAtkDistance))]
    protected float _extraAtkDistance;

    [SerializeField, SyncVar(hook = nameof(OnChangedEnemyLayer))]
    protected int _enemyLayer;

    public string CreatureName { get { return _creatureName; } set { _creatureName = value; } }
    public int Level { get { return _level; } set { _level = value; } }

    public int Hp { get { return _hp; } 
        set 
        { 
            _hp = Mathf.Clamp(value, 0, MaxHp + _extraMaxHp); 
        } 
    }
    public int MaxHp { get { return _maxHp; } set { _maxHp = value; } }
    public int ExtraMaxHp { get { return _extraMaxHp; } 
        set 
        { 
            _extraMaxHp = value;
            if(value < 0)
            {
                Hp = Hp;
            }
        } 
    }
    public int TotalMaxHp { get { return _maxHp + _extraMaxHp; } }

    public int HpRecoveryAmount { get { return _hpRecoveryAmount; } set { _hpRecoveryAmount = value; } }
    public int ExtraHpRecoveryAmount { get { return _extraHpRecoveryAmount; } set { _extraHpRecoveryAmount = value; } }

    public int TotalHpRecoveryAmount { get { return _hpRecoveryAmount + _extraHpRecoveryAmount; } }

    public int Mp { get { return _mp; } 
        set 
        {
            _mp = Mathf.Clamp(value, 0, MaxMp + _extraMaxMp); 
        } 
    }
    public int MaxMp { get { return _maxMp; } set { _maxMp = value; } }
    public int ExtraMaxMp { get { return _extraMaxMp; } 
        set 
        { 
            _extraMaxMp = value;
            if(value < 0)
            {
                Mp = Mp;
            }
        } 
    }
    public int TotalMaxMp {  get {  return _maxMp + _extraMaxMp; } }

    public int MpRecoveryAmount { get { return _mpRecoveryAmount; } set { _mpRecoveryAmount = value; } }
    public int ExtraMpRecoveryAmount { get { return _extraMpRecoveryAmount; } set { _extraMpRecoveryAmount = value; } }

    public int TotalMpRecoveryAmount { get { return _mpRecoveryAmount + _extraMpRecoveryAmount; } }

    public int Attack { get { return _attack; } set { _attack = value; } }
    public int ExtraAttack { get { return _extraAttack; } set { _extraAttack = value; } }
    public int TotalAttack { get { return _attack + _extraAttack; } }

    public float AtkSpeed { get { return _atkSpeed; } set { _atkSpeed = value; } }
    public float ExtraAtkSpeed { get { return _extraAtkSpeed; } set { _extraAtkSpeed = value; } }
    public float TotalAtkSpeed { get { return _atkSpeed + _extraAtkSpeed; } }

    public int Defense { get { return _defense; } set { _defense = value; } }
    public int ExtraDefense { get { return _extraDefense; } set { _extraDefense = value; } }
    public int TotalDefense { get {  return _defense + _extraDefense; } }

    public float MoveSpeed { get { return _moveSpeed; } set { _moveSpeed = value; } }
    public float ExtraMoveSpeed { get { return _extraMoveSpeed; } set { _extraMoveSpeed = value; } }
    public float TotalMoveSpeed { get {  return _moveSpeed + _extraMoveSpeed; } }

    public int HitStatePriority { get { return _hitStatePriority; } set { _hitStatePriority = value; } }
    public int ExtraHitStatePriority {  get { return _extraHitStatePriority; } set { _extraHitStatePriority = value; } }
    public int TotalHitStatePriority {  get { return _hitStatePriority + ExtraHitStatePriority; } }

    public float AtkDistance { get { return _atkDistance; } set { _atkDistance = value; } }
    public float ExtraAtkDistance { get { return _extraAtkDistance; } set { _extraAtkDistance = value; } }
    public float TotalAtkDistance { get { return AtkDistance + ExtraAtkDistance; } }

    public int EnemyLayer { get { return _enemyLayer; } }

    protected List<Data.DropItem> _dropItems;

    #region 클라이언트 콜백 UI 업데이트
    public Action<string> OnNameAction;
    public Action<int> OnLevelAction;
    public Action<int, int> OnHpAction;
    public Action<int> OnHpRecoveryAction;
    public Action<int, int> OnMpAction;
    public Action<int> OnMpRecoveryAction;
    public Action<int> OnAttackAction;
    public Action<float> OnAttackSpeedAction;
    public Action<int> OnDefenseAction;
    public Action<float> OnMoveSpeedAction;
    public Action<int> OnHSPriorityAction;
    public Action<float> OnAtkDistanceAction;
    public Action<int> OnEnemyLayerAction;

    void OnChangedCreatureName(string _old, string _new)
    {
        OnNameAction?.Invoke(_new);
    }

    void OnChangedLevel(int _old, int _new)
    {
        OnLevelAction?.Invoke(_new);
    }
    
    void OnChangedHp(int _old, int _new)
    {
        OnHpAction?.Invoke(_hp, TotalMaxHp);
    }

    void OnChangedHpRecovery(int _old, int _new)
    {
        OnHpRecoveryAction?.Invoke(TotalHpRecoveryAmount);
    }

    void OnChangedMp(int _old, int _new)
    {
        OnMpAction?.Invoke(_mp, TotalMaxMp);
    }

    void OnChangedMpRecovery(int _old, int _new)
    {
        OnMpRecoveryAction?.Invoke(TotalMpRecoveryAmount);
    }

    void OnChangedAttack(int _old, int _new)
    {
        OnAttackAction?.Invoke(TotalAttack);
    }

    void OnChangedAttackSpeed(float _old, float _new)
    {
        OnAttackSpeedAction?.Invoke(TotalAtkSpeed);
    }

    void OnChangedDefense(int _old, int _new)
    {
        OnDefenseAction?.Invoke(TotalDefense);
    }

    void OnChangedMoveSpeed(float _old, float _new)
    {
        OnMoveSpeedAction?.Invoke(TotalMoveSpeed);
    }

    void OnChangedHSPriority(int _old, int _new)
    {
        OnHSPriorityAction?.Invoke(TotalHitStatePriority);
    }

    void OnChangedAtkDistance(float _old, float _new)
    {
        OnAtkDistanceAction?.Invoke(TotalAtkDistance);
    }

    void OnChangedEnemyLayer(int _old, int _new)
    {
        OnEnemyLayerAction?.Invoke(_new);
    }

    #endregion

    [ServerCallback]
    public void Awake()
    {
        Init();
    }

    public virtual void Init()
    {

    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(RecoveryCor());
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        StopCoroutine(RecoveryCor());
    }

    // 자동 회복 코루틴
    IEnumerator RecoveryCor()
    {
        while (true)
        {
            yield return recoveryWait;
            if (Hp > 0)
                Hp += TotalHpRecoveryAmount;
            Mp += TotalMpRecoveryAmount;
        }
    }

    // 리스폰 스텟 초기화
    public virtual void RespawnStatInit() { }
}
