using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStat : Stat
{
    [SerializeField]
    protected Define.MonsterType type;

    public Define.MonsterType MonsterType { get { return type; } }

    [SerializeField, SyncVar]
    protected float _expGained;
    public float ExpGained { get { return _expGained; } set { _expGained = value; } }
    
    [SerializeField, SyncVar]
    protected float _detectRange;
    public float DetectRange { get { return _detectRange; } set { _detectRange = value; } }

    [SerializeField, SyncVar]
    protected float _chaseDistance;
    public float ChaseDistance { get { return _chaseDistance;  } }

    [SerializeField, SyncVar]
    protected float _patrolDistance;
    public float PatrolDistance { get { return _patrolDistance; } }

    #region 클라이언트 콜백 UI 업데이트

    Action<int> OnExpGainedAction;
    Action<float> OnDetectRangeAction;
    Action<float> OnChaseDistanceAction;
    Action<float> OnPatrolDistanceAction;

    public void OnChangedExpGained(int _old, int _new)
    {
        OnExpGainedAction?.Invoke(_new);
    }

    public void OnChangedDetectRange(float _old, float _new)
    {
        OnDetectRangeAction?.Invoke(_new);
    }

    public void OnChangedChaseDistance(float _old, float _new)
    {
        OnChaseDistanceAction?.Invoke(_new);
    }

    public void OnChangedPatrolDistance(float _old, float _new)
    {
        OnPatrolDistanceAction?.Invoke(_new);
    }

    #endregion

    public override void Init()
    {
        // 하드코딩 (추후 데이터 불러와서 세팅 예정)
        // 플레이어를 적대시 한다
        SetStat(type);
    }

    public void SetStat(Define.MonsterType _type)
    {
        Dictionary<Define.MonsterType, Data.MonsterStat> dict = Managers.Data.MonsterStatDict;
        Data.MonsterStat stat = dict[_type];
        if (stat == null)
            return;

        _creatureName = stat._creatureName;
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
        _enemyLayer = stat._enemyLayer;
        _expGained = stat._expGained;
        _detectRange = stat._detectRange;
        _chaseDistance = stat._chaseDistance;
        _patrolDistance = stat._patrolDstance;
        _dropItems = stat._dropItemList;
    }

    public override void RespawnStatInit()
    {
        _hp = _maxHp + ExtraMaxHp;
        _mp = _maxMp + ExtraMaxMp;
    }

    public void DropItems()
    {
        for (int i = 0; i < _dropItems.Count; i++)
        {
            Data.DropItem dropItem = _dropItems[i];
            float percent = dropItem._percent;
            float randomValue = UnityEngine.Random.Range(0.0f, 1.0f);

            // 스폰
            if (randomValue <= percent)
            {
                Item item = null;
                Define.ItemType itemType = dropItem._itemType;
                Define.ItemSubType itemSubType = dropItem._itemSubType;
                int id = dropItem._id;
                int amount = UnityEngine.Random.Range(1, dropItem._amount);

                if (itemType == Define.ItemType.Countable)
                {
                    if (itemSubType == Define.ItemSubType.Consumption)
                    {
                        Data.PortionItemData itemData = Managers.Data.PortionItemDict[id];
                        item = itemData.CreateItem();
                        item.Amount = amount;
                    }

                }
                else if (itemType == Define.ItemType.Equip)
                {
                    if (itemSubType == Define.ItemSubType.MeleeWeapon)
                    {
                        Data.WeaponItemData itemData = Managers.Data.MeleeWeaponItemDict[id];
                        item = itemData.CreateItem();
                        item.Amount = amount;
                    }
                    else if (itemSubType == Define.ItemSubType.Helmet)
                    {
                        Data.ArmorItemData itemData = Managers.Data.HelmetItemDict[id];
                        item = itemData.CreateItem();
                        item.Amount = amount;
                    }
                    else if (itemSubType == Define.ItemSubType.Shoulder)
                    {
                        Data.ArmorItemData itemData = Managers.Data.ShoulderItemDict[id];
                        item = itemData.CreateItem();
                        item.Amount = amount;
                    }
                    else if (itemSubType == Define.ItemSubType.Top)
                    {
                        Data.ArmorItemData itemData = Managers.Data.TopItemDict[id];
                        item = itemData.CreateItem();
                        item.Amount = amount;
                    }
                    else if (itemSubType == Define.ItemSubType.Pants)
                    {
                        Data.ArmorItemData itemData = Managers.Data.PantsItemDict[id];
                        item = itemData.CreateItem();
                        item.Amount = amount;
                    }
                    else if (itemSubType == Define.ItemSubType.Belt)
                    {
                        Data.ArmorItemData itemData = Managers.Data.BeltItemDict[id];
                        item = itemData.CreateItem();
                        item.Amount = amount;
                    }
                    else if (itemSubType == Define.ItemSubType.Shoes)
                    {
                        Data.ArmorItemData itemData = Managers.Data.ShoesItemDict[id];
                        item = itemData.CreateItem();
                        item.Amount = amount;
                    }
                    else if (itemSubType == Define.ItemSubType.Ring)
                    {
                        Data.ArmorItemData itemData = Managers.Data.RingItemDict[id];
                        item = itemData.CreateItem();
                        item.Amount = amount;
                    }
                    else if (itemSubType == Define.ItemSubType.Necklace)
                    {
                        Data.ArmorItemData itemData = Managers.Data.NecklaceItemDict[id];
                        item = itemData.CreateItem();
                        item.Amount = amount;
                    }
                    else if (itemSubType == Define.ItemSubType.Bracelet)
                    {
                        Data.ArmorItemData itemData = Managers.Data.BraceletItemDict[id];
                        item = itemData.CreateItem();
                        item.Amount = amount;
                    }
                }
                else if(itemType == Define.ItemType.Coin)
                {
                    Data.CoinItemData itemData = Managers.Data.CoinItemDict[id];
                    item = itemData.CreateItem(amount);
                }

                Managers.Game.CreateWorldItem(item, gameObject);
            }
        }
    }
}
