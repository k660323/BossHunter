using Data;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : Stat
{
    [SerializeField]
    Define.PlayerType playerType;
    public Define.PlayerType PlayerType { get { return playerType; } set { playerType = value; } }

    [SerializeField, SyncVar(hook = nameof(OnChangedExp))]
    protected float _exp;
    [SerializeField, SyncVar(hook = nameof(OnChangedMaxExp))]
    protected float _maxExp;

    public float Exp { get { return _exp; } set { _exp = Mathf.Max(0, value); } }
    public float MaxExp { get { return _maxExp; } set { _maxExp = value; } }

    // ����� ���� UI
    [SerializeField]
    private UI_Stat uI_Stat;

    #region Ŭ���̾�Ʈ �ݹ� UI ������Ʈ

    public Action<float, float> OnExpAction;

    IEnumerator ExpUpCor;
    float curGainExp;

    public void OnChangedExp(float _old, float _new)
    {
        OnExpAction?.Invoke(_new, _maxExp);
    }

    public void OnChangedMaxExp(float _old, float _new)
    {
        OnExpAction?.Invoke(_exp, _new);
    }

    #endregion

    public override void Init()
    {
        SetStat(1, playerType);
        _enemyLayer = Managers.LayerManager.Monster;
        
    }

    public void SetStat(int level, Define.PlayerType _type)
    {
        Dictionary<Define.PlayerType, Data.PlayerStatList> dict = Managers.Data.PlayerStatDict;
        Data.PlayerStatList statList = dict[_type];
        Data.PlayerStat stat = statList._playerStatList[level - 1];
        if (stat == null)
            return;

        _creatureName = statList._creatureName;
        _level = stat._level;
        _maxHp = stat._maxHp;
        _hp = stat._maxHp + ExtraMaxHp;
        _hpRecoveryAmount = stat._hpRecoveryAmount;
        _maxMp = stat._maxMp;
        _mp = stat._maxMp + ExtraMaxMp;
        _mpRecoveryAmount = stat._mpRecoveryAmount;
        _attack = stat._attack;
        _atkSpeed = stat._atkSpeed;
        _defense = stat._defense;
        _moveSpeed = stat._moveSpeed;
        _hitStatePriority = stat._hitStatePriority;
        _atkDistance = stat._atkDistance;
        _exp = 0;
        _maxExp = stat._maxExp;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        OnLevelAction -= PlayLevelUpSound;
        OnLevelAction += PlayLevelUpSound;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        OnLevelAction -= PlayLevelUpSound;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if (Managers.UI.SceneUI is IPlayerUI playerUI)
        {
            uI_Stat = playerUI.GetPlayerUI.UI_Stat;
            uI_Stat.PlayerUIStatInit(this);
        }
    }

    // ����ġ ���� ���� �ڷ�ƾ�� �۵��߿� ����ġ�� ������ curGainExp�� ���� �����Ѵ�.
    public void SetExp(float _gainExp)
    {
        curGainExp += _gainExp;
        if (ExpUpCor == null)
        {
            ExpUpCor = LevelUpCor();
            StartCoroutine(ExpUpCor);
        }
    }

    IEnumerator LevelUpCor()
    {
        // ���� ��
        while (MaxExp <= Exp + curGainExp)
        {
            curGainExp = Mathf.Max(0, Exp + curGainExp - MaxExp);
            SetStat(Level + 1, playerType);
            yield return null;
        }

        Exp += curGainExp;
        curGainExp = 0;
        ExpUpCor = null;
    }

    public override void RespawnStatInit()
    {
        // 50%�� ȸ����Ų��.
        _hp = (int)((_maxHp + ExtraMaxHp) * 0.5f);
        _mp = (int)((_maxMp + ExtraMaxMp) * 0.5f);
    }

    void PlayLevelUpSound(int level)
    {
        if (Util.IsSameScene(gameObject))
            Managers.Sound.Play3D("MLevelUP", transform);
    }
}
