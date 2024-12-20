using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    public Dictionary<int, Data.Stat> StatDict { get; private set; } = new Dictionary<int, Data.Stat>();

    public Dictionary<Define.PlayerType, Data.PlayerStatList> PlayerStatDict { get; private set; } = new Dictionary<Define.PlayerType, Data.PlayerStatList>();
    
    public Dictionary<Define.MonsterType, Data.MonsterStat> MonsterStatDict { get; private set; } = new Dictionary<Define.MonsterType, Data.MonsterStat>();
    
    public Dictionary<Define.PlayerType, Data.PlayerCharacterInfo> PlayerCharacterInfoDict { get; private set;} = new Dictionary<Define.PlayerType, Data.PlayerCharacterInfo>();

    public Dictionary<int, Data.Lighting> SceneLightingDict { get; private set; } = new Dictionary<int, Data.Lighting>();

    public Dictionary<int, Data.Lighting> InstanceSceneLightingDict { get; private set; } = new Dictionary<int, Data.Lighting>();
    
    public Dictionary<int, Data.SceneInfo> SceneInfoDict { get; private set; } = new Dictionary<int, Data.SceneInfo>();
    
    public Dictionary<int, Data.SceneInfo> InstanceSceneInfoDict { get; private set; } = new Dictionary<int, Data.SceneInfo>();
    
    public Dictionary<int, Data.PortionItemData> PortionItemDict { get; private set; } = new Dictionary<int, Data.PortionItemData>();

    public Dictionary<int, Data.ArmorItemData> HelmetItemDict { get; private set; } = new Dictionary<int, Data.ArmorItemData>();

    public Dictionary<int, Data.ArmorItemData> ShoulderItemDict { get; private set; } = new Dictionary<int, Data.ArmorItemData>();

    public Dictionary<int, Data.ArmorItemData> TopItemDict { get; private set; } = new Dictionary<int, Data.ArmorItemData>();

    public Dictionary<int, Data.ArmorItemData> PantsItemDict { get; private set; } = new Dictionary<int, Data.ArmorItemData>();

    public Dictionary<int, Data.ArmorItemData> BeltItemDict { get; private set; } = new Dictionary<int, Data.ArmorItemData>();

    public Dictionary<int, Data.ArmorItemData> ShoesItemDict { get; private set; } = new Dictionary<int, Data.ArmorItemData>();

    public Dictionary<int, Data.ArmorItemData> RingItemDict { get; private set; } = new Dictionary<int, Data.ArmorItemData>();

    public Dictionary<int, Data.ArmorItemData> NecklaceItemDict { get; private set; } = new Dictionary<int, Data.ArmorItemData>();

    public Dictionary<int, Data.ArmorItemData> BraceletItemDict { get; private set; } = new Dictionary<int, Data.ArmorItemData>();

    public Dictionary<int, Data.WeaponItemData> MeleeWeaponItemDict { get; private set; } = new Dictionary<int, Data.WeaponItemData>();

    public Dictionary<int, Data.CoinItemData> CoinItemDict { get; private set; } = new Dictionary<int, Data.CoinItemData>(); 
    
    public void Init()
    {

#if !UNITY_SERVER
        SceneLightingDict = LoadJson<Data.LightingData, int, Data.Lighting>("SceneLightingData.json").MakeDict();
        InstanceSceneLightingDict = LoadJson<Data.LightingData, int, Data.Lighting>("InstanceSceneLightingData.json").MakeDict();
#endif
        SceneInfoDict = LoadJson<Data.SceneInfoData, int, Data.SceneInfo>("SceneInfoData.json").MakeDict();
        InstanceSceneInfoDict = LoadJson<Data.SceneInfoData, int, Data.SceneInfo>("InstanceSceneInfoData.json").MakeDict();

        StatDict = LoadJson<Data.StatData, int, Data.Stat>("StatData.json").MakeDict();
        PlayerStatDict = LoadJson<Data.PlayerStatListData, Define.PlayerType, Data.PlayerStatList>("PlayerStatDataList.json").MakeDict();
        MonsterStatDict = LoadJson<Data.MonsterStatData, Define.MonsterType, Data.MonsterStat>("MonsterStatData.json").MakeDict();
        PlayerCharacterInfoDict = LoadJson<Data.PlayerCharacterInfoData, Define.PlayerType, Data.PlayerCharacterInfo>("PlayerCharacterInfoData.json").MakeDict();
        
        PortionItemDict = LoadJson<Data.PortionItemDatas, int, Data.PortionItemData>("PortionItemData.json").MakeDict();
        
        HelmetItemDict = LoadJson<Data.ArmorItemDatas, int, Data.ArmorItemData>("HelmetItemData.json").MakeDict();
        ShoulderItemDict = LoadJson<Data.ArmorItemDatas, int, Data.ArmorItemData>("ShoulderItemData.json").MakeDict();
        TopItemDict = LoadJson<Data.ArmorItemDatas, int, Data.ArmorItemData>("TopItemData.json").MakeDict();
        PantsItemDict = LoadJson<Data.ArmorItemDatas, int, Data.ArmorItemData>("PantsItemData.json").MakeDict();
        BeltItemDict = LoadJson<Data.ArmorItemDatas, int, Data.ArmorItemData>("BeltItemData.json").MakeDict();
        ShoesItemDict = LoadJson<Data.ArmorItemDatas, int, Data.ArmorItemData>("ShoesItemData.json").MakeDict();
        RingItemDict = LoadJson<Data.ArmorItemDatas, int, Data.ArmorItemData>("RingItemData.json").MakeDict();
        NecklaceItemDict = LoadJson<Data.ArmorItemDatas, int, Data.ArmorItemData>("NecklaceItemData.json").MakeDict();
        BraceletItemDict = LoadJson<Data.ArmorItemDatas, int, Data.ArmorItemData>("BraceletItemData.json").MakeDict();
       
        MeleeWeaponItemDict = LoadJson<Data.WeaponItemDatas, int, Data.WeaponItemData>("MeleeWeaponItemData.json").MakeDict();

        CoinItemDict = LoadJson<Data.CoinItemDatas, int, Data.CoinItemData>("CoinItemData.json").MakeDict();
    }

    Loader LoadJson<Loader, Key, Value>(string path) where Loader :ILoader<Key,Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>(path);
        return JsonUtility.FromJson<Loader>(textAsset.text);
    }
}
