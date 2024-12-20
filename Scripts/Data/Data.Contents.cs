using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    #region Stat

    [Serializable]
    public class Stat
    {
        public int _level;
        public int _maxHp;
        public int _hpRecoveryAmount;
        public int _maxMp;
        public int _mpRecoveryAmount;
        public int _attack;
        public float _atkSpeed = 1;
        public int _defense;
        public float _moveSpeed;
        public int _hitStatePriority;
        public float _atkDistance;
    }

    [Serializable]
    public class PlayerStatList
    {
        public Define.PlayerType _playerType;
        public string _creatureName;
        public List<PlayerStat> _playerStatList;
    }

    [Serializable]
    public class PlayerStat : Stat
    {
        public int _maxExp;
    }


    [Serializable]
    public class MonsterStat : Stat
    {
        public Define.MonsterType _monsterType;
        public string _creatureName;
        public int _enemyLayer;
        public float _expGained;
        public float _detectRange;
        public float _chaseDistance;
        public float _patrolDstance;
        public List<DropItem> _dropItemList;
    }

    [Serializable]
    public class DropItem
    {
        public Define.ItemType _itemType;
        public Define.ItemSubType _itemSubType;
        public int _id;
        public int _amount;
        public float _percent;
    }

    [Serializable]
    public class EquipmentItemStat
    {
        public int _requiredLevel;
        public int _maxHp;
        public int _hpRecoveryAmount;
        public int _maxMp;
        public int _mpRecoveryAmount;
        public int _attack;
        public float _attackSpeed;
        public int _defense;
        public float _moveSpeed;
        public int _hitStatePriority;
    }

    [Serializable]
    public class WeaponItemStat
    {
        public float _atkDistanceAI;
        public Vector3 _attackSize;
        public float _attackCoolTime = 1;
        public int _maxHitCount = 1;
    }

    [SerializeField]
    public class RangeWeaponItemStat
    {
        public string _spawnProjectile;
        public float _projectileMoveSpeed;
        public float _lifeTime;
    }

    [Serializable]
    public class StatData : ILoader<int, Stat>
    {
        public List<Stat> stats = new List<Stat>();

        public Dictionary<int, Stat> MakeDict()
        {
            Dictionary<int, Stat> dict = new Dictionary<int, Stat>();
            foreach (Stat stat in stats)
                dict.Add(stat._level, stat);
            return dict;
        }
    }

    [Serializable]
    public class PlayerStatListData : ILoader<Define.PlayerType, PlayerStatList>
    {
        public List<PlayerStatList> stats = new List<PlayerStatList>();

        public Dictionary<Define.PlayerType, PlayerStatList> MakeDict()
        {
            Dictionary<Define.PlayerType, PlayerStatList> dict = new Dictionary<Define.PlayerType, PlayerStatList>();
            foreach (PlayerStatList stat in stats)
            {
                dict.Add(stat._playerType, stat);
            }
            return dict;
        }
    }

    [Serializable]
    public class MonsterStatData : ILoader<Define.MonsterType, MonsterStat>
    {
        public List<MonsterStat> stats = new List<MonsterStat>();

        public Dictionary<Define.MonsterType, MonsterStat> MakeDict()
        {
            Dictionary<Define.MonsterType, MonsterStat> dict = new Dictionary<Define.MonsterType, MonsterStat>();
            foreach (MonsterStat stat in stats)
                dict.Add(stat._monsterType, stat);
            return dict;
        }
    }

    #endregion

    #region ItemData

    [Serializable]
    public class ItemData
    {
        public Define.ItemType _itemType;
        public Define.ItemSubType _itemSubType;
        public Define.ItemGrade _itemGrade;
        public Define.EffectType _effectType;
        public int _id;
        public string _name;
        public string _tooltip;
        public string _iconSpritePath;
        public long _itemPrice;
        // 타입에 맞는 아이템 생성
        public virtual Item CreateItem()
        {
            return new Item(this);
        }
    }

    [Serializable]
    public class CoinItemData : ItemData
    {
        public Item CreateItem(long value)
        {
            return new CoinItem(this, value);
        }
    }

    [Serializable]
    public class CoinItemDatas : ILoader<int, CoinItemData>
    {
        public List<CoinItemData> items = new List<CoinItemData>();

        public Dictionary<int, CoinItemData> MakeDict()
        {
            Dictionary<int, CoinItemData> dict = new Dictionary<int, CoinItemData>();
            foreach (CoinItemData item in items)
            {
                dict.Add(item._id, item);
            }

            return dict;
        }
    }

    [Serializable]
    public class CountableItemData : ItemData
    {
        public int _maxAmount = 99;

        public override Item CreateItem()
        {
            return new CountableItem(this);
        }
    }

    [Serializable]
    public class PortionItemData : CountableItemData
    {
        public float _value;

        public override Item CreateItem()
        {
            return new PortionItem(this);
        }
    }

    [Serializable]
    public class PortionItemDatas : ILoader<int, PortionItemData>
    {
        public List<PortionItemData> items = new List<PortionItemData>();

        public Dictionary<int, PortionItemData> MakeDict()
        {
            Dictionary<int, PortionItemData> dict = new Dictionary<int, PortionItemData>();
            foreach (PortionItemData item in items)
            {
                dict.Add(item._id, item);
            }

            return dict;
        }
    }

    [Serializable]
    public class EquipmentItemData : ItemData
    {
        public EquipmentItemStat _equipmentStat;

        public int MaxDurability => _maxDurability;

        public int _maxDurability = 100;
    }

    [Serializable]
    public class ArmorItemData : EquipmentItemData
    {
        public override Item CreateItem()
        {
            return new ArmorItem(this);
        }
    }

    [Serializable]
    public class ArmorItemDatas : ILoader<int, ArmorItemData>
    {
        public List<ArmorItemData> items = new List<ArmorItemData>();

        public Dictionary<int, ArmorItemData> MakeDict()
        {
            Dictionary<int, ArmorItemData> dict = new Dictionary<int, ArmorItemData>();
            foreach (ArmorItemData item in items)
                dict.Add(item._id, item);
            return dict;
        }
    }

    [Serializable]
    public class WeaponItemData : EquipmentItemData
    {
        public WeaponItemStat _weaponStat;

        public override Item CreateItem()
        {
            return new WeaponItem(this);
        }
    }

    [Serializable]
    public class RangeWeaponItemData : WeaponItemData
    {
        public RangeWeaponItemStat _rangeWeaponStat;
        public override Item CreateItem()
        {
            return new RangeWeaponItem(this);
        }
    }

    [Serializable]
    public class WeaponItemDatas : ILoader<int, WeaponItemData>
    {
        public List<WeaponItemData> items = new List<WeaponItemData>();

        public Dictionary<int, WeaponItemData> MakeDict()
        {
            Dictionary<int, WeaponItemData> dict = new Dictionary<int, WeaponItemData>();
            foreach (WeaponItemData item in items)
                dict.Add(item._id, item);
            return dict;
        }
    }

    [Serializable]
    public class RangeWeaponItemDatas : ILoader<int, RangeWeaponItemData>
    {
        public List<RangeWeaponItemData> items = new List<RangeWeaponItemData>();

        public Dictionary<int, RangeWeaponItemData> MakeDict()
        {
            Dictionary<int, RangeWeaponItemData> dict = new Dictionary<int, RangeWeaponItemData>();
            foreach (RangeWeaponItemData item in items)
                dict.Add(item._id, item);
            return dict;
        }
    }

    #endregion

    #region PlayerCharacterInfo

    [Serializable]
    public class PlayerCharacterInfo
    {
        public Define.PlayerType _type;
        public string _name;
        public string _mainWeapon;
        public int _difficulty;
        public int _offensePower;
        public int _defense;
        public int _mobility;
        public string _comment;
    }

    [Serializable]
    public class PlayerCharacterInfoData : ILoader<Define.PlayerType, PlayerCharacterInfo>
    {
        public List<PlayerCharacterInfo> playerCharacterInfos = new List<PlayerCharacterInfo>();

        public Dictionary<Define.PlayerType, PlayerCharacterInfo> MakeDict()
        {
            Dictionary<Define.PlayerType, PlayerCharacterInfo> dict = new Dictionary<Define.PlayerType, PlayerCharacterInfo>();
            foreach (PlayerCharacterInfo info in playerCharacterInfos)
                dict.Add(info._type, info);
            return dict;
        }
    }

    #endregion

    #region Lighting

    [Serializable]
    public class DirectionLight
    {
        public Vector3 _rot;
        public LightType _lightType;
        public Color _lightColor;
        public float _intensity;
        public LightRenderMode _lightRenderMode;
        public int _cullingMask;
        public float _shadowStrength;
        public float _shadowNearPlane;
    }

    [Serializable]
    public class Lighting
    {
        public int _scene;

        // Scene
        public string _lightingSettingsAsset;

        // Direction Light
        public DirectionLight _directionLight;
    }

    [Serializable]
    public class LightingData : ILoader<int, Lighting>
    {
        public List<Lighting> lights = new List<Lighting>();

        public Dictionary<int, Lighting> MakeDict()
        {
            Dictionary<int, Lighting> dict = new Dictionary<int, Lighting>();
            foreach (Lighting light in lights)
                dict.Add(light._scene, light);
            return dict;
        }
    }
    #endregion

    #region Scene
    [Serializable]
    public class SceneInfo
    {
        public int _scene;
        public string _sceneImage;
        public string _nickName;
        public string _recommendLevel;
        public string _comment;
        public Vector3 _startPosition;
    }

    [Serializable]
    public class SceneInfoData : ILoader<int, SceneInfo>
    {
        public List<SceneInfo> sceneInfos = new List<SceneInfo>();

        public Dictionary<int, SceneInfo> MakeDict()
        {
            Dictionary<int, SceneInfo> dict = new Dictionary<int, SceneInfo>();
            foreach (SceneInfo info in sceneInfos)
                dict.Add(info._scene, info);
            return dict;
        }
    }
    #endregion
}