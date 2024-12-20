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

    // 연결된 스텟 UI
    [SerializeField]
    private UI_Stat uI_Stat;

    #region 클라이언트 콜백 UI 업데이트

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

    // 경험치 설정 만약 코루틴이 작동중에 경험치가 들어오면 curGainExp에 값만 변경한다.
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
        // 레벨 업
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
        // 50%만 회복시킨다.
        _hp = (int)((_maxHp + ExtraMaxHp) * 0.5f);
        _mp = (int)((_maxMp + ExtraMaxMp) * 0.5f);
    }

    void PlayLevelUpSound(int level)
    {
        if (Util.IsSameScene(gameObject))
            Managers.Sound.Play3D("MLevelUP", transform);
    }
}
