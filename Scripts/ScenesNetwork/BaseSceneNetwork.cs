using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Rendering;

public abstract class BaseSceneNetwork : NetworkBehaviour, IChatable
{
    [SerializeField]
    protected string BGM_Path;

    protected BaseScene baseScene;

    // 정적 리스트 터레인 리스트 (모든 BaseSceneNetwork는 하나의 리스트를 사용한다)
    static List<Terrain> terrainList = new List<Terrain>();

    public static void DisableTerrainList()
    {
        if (Managers.Instance.mode == NetworkManagerMode.Host)
        {
            for (int i = 0; i < terrainList.Count; i++)
            {
                if (terrainList[i] != null)
                {
                    terrainList[i].enabled = false;
                }
            }

            terrainList.Clear();
        }
    }

    // 일단 현재 씬에 있는 터레인 배열
    [SerializeField]
    Terrain[] terrains;

    [SerializeField]
    protected VolumeProfile sharedProfile;

    void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        TryGetComponent(out baseScene);
    }

    // 서버나 호스트 시작시 터레인 비활성화
    public override void OnStartServer()
    {
        base.OnStartServer();

        if (terrains != null)
        {
            for (int i = 0; i < terrains.Length; i++)
            {
                terrains[i].enabled = false;
            }
        }
    }

    // 네트워크 오브젝트 생성시 모든 클라이언트가 실행함
    public override void OnStartClient()
    {
        base.OnStartClient();
        InitTerrainHost();
        SetSceneLightingAndShader();
        ShowSceneUI();
        Managers.Sound.Play2D(BGM_Path, Define.Sound2D.Bgm);
    }

    [ServerCallback]
    protected virtual void InitTerrainHost()
    {
        // 호스트만 관리한다.
        if (Managers.Instance.mode == NetworkManagerMode.Host)
        {
            if (terrains != null)
            {
                for (int i = 0; i < terrains.Length; i++)
                {
                    terrains[i].enabled = true;
                    terrainList.Add(terrains[i]);
                }
            }

        }
    }

    // 씬 환경 설정
    protected virtual void SetSceneLightingAndShader()
    {
        SceneManager.SetActiveScene(gameObject.scene);
        if (Managers.Data.SceneLightingDict.TryGetValue((int)baseScene.SceneType, out Data.Lighting data))
        {
            // 빛
            OnlineScene onlineScene = Managers.Scene.GetCachedBaseScene(Define.Scene.Online) as OnlineScene;
           
            Light light = onlineScene.directionLight;
            light.transform.rotation = Quaternion.Euler(data._directionLight._rot);
            light.type = data._directionLight._lightType;
            light.color = data._directionLight._lightColor;
            light.intensity = data._directionLight._intensity;
            light.renderMode = data._directionLight._lightRenderMode;
            light.cullingMask = data._directionLight._cullingMask;
            light.shadowStrength = data._directionLight._shadowStrength;
            light.shadowNearPlane = data._directionLight._shadowNearPlane;
            RenderSettings.sun = light;

            Volume urpPP = onlineScene.urpPostProcessing;
            urpPP.sharedProfile = sharedProfile;
        }
    }

    protected abstract void ShowSceneUI();

    [Command(requiresAuthority = false)]
    public void CTS_ChatRPC(string message)
    {
        ChatRPC(message);
    }

    [ClientRpc]
    public void ChatRPC(string message)
    {
        if (Managers.UI.SceneUI is IChatable_UI)
        {
            IChatable_UI chatable = Managers.UI.SceneUI as IChatable_UI;
            chatable.UpdateChat(message);
        }
    }

    [ClientRpc]
    public void RPC_Play2D(string path, Define.Sound2D type, float pitch)
    {
#if !UNITY_SERVER
        if (Util.IsSameScene(gameObject) == false)
            return;

        Managers.Sound.Play2D(path, type, pitch);
#endif
    }

    [ClientRpc]
    public void RPC_Play3D(string path, Vector3 pos, Define.Sound3D type, float volume, float pitch, float minDistance, float maxDistance)
    {
#if !UNITY_SERVER
        if (Util.IsSameScene(gameObject) == false)
            return;

        Managers.Sound.Play3D(path, pos, type, volume, pitch, minDistance, maxDistance);
#endif
    }

    [ClientRpc]
    public void RPC_Play3D(string path, GameObject gameObject, bool isAttach, Define.Sound3D type, float volume, float pitch, float minDistance, float maxDistance)
    {
#if !UNITY_SERVER
        if (Util.IsSameScene(gameObject) == false)
            return;

        if (gameObject != null)
            Managers.Sound.Play3D(path, gameObject.transform, isAttach, type, volume, pitch, minDistance, maxDistance);
#endif
    }

    [ClientRpc]
    public void RPC_Effect(string key, Vector3 spawnPos, Quaternion qut)
    {
#if !UNITY_SERVER
        if (Util.IsSameScene(gameObject) == false)
            return;

        GameObject effectObj = Managers.Resource.Instantiate(key, null, true);
        effectObj.transform.parent = null;
        effectObj.transform.position = spawnPos;
        effectObj.transform.rotation = qut;

        if (effectObj.TryGetComponent(out IEffectPlay effectPlay))
        {
            effectPlay.EffectPlay();
        }
#endif
    }


    [Command(requiresAuthority = false)]
    public void CTS_TestCreateItemRPC(GameObject target, Define.ItemType itemType, Define.ItemSubType itemSubType, int id)
    {
        Player player = target.GetComponent<Player>();
        Inventory inventory = player.Inventory;

        if (itemType == Define.ItemType.Countable)
        {
            if (itemSubType == Define.ItemSubType.Consumption)
            {
                Data.PortionItemData itemData = Managers.Data.PortionItemDict[id];
                inventory.Add(itemData);
            }

        }
        else if (itemType == Define.ItemType.Equip)
        {
            if (itemSubType == Define.ItemSubType.MeleeWeapon)
            {
                Data.WeaponItemData itemData = Managers.Data.MeleeWeaponItemDict[id];
                inventory.Add(itemData);
            }
            else if (itemSubType == Define.ItemSubType.Helmet)
            {
                Data.ArmorItemData itemData = Managers.Data.HelmetItemDict[id];
                inventory.Add(itemData);
            }
            else if (itemSubType == Define.ItemSubType.Shoulder)
            {
                Data.ArmorItemData itemData = Managers.Data.ShoulderItemDict[id];
                inventory.Add(itemData);
            }
            else if (itemSubType == Define.ItemSubType.Top)
            {
                Data.ArmorItemData itemData = Managers.Data.TopItemDict[id];
                inventory.Add(itemData);
            }
            else if (itemSubType == Define.ItemSubType.Pants)
            {
                Data.ArmorItemData itemData = Managers.Data.PantsItemDict[id];
                inventory.Add(itemData);
            }
            else if (itemSubType == Define.ItemSubType.Belt)
            {
                Data.ArmorItemData itemData = Managers.Data.BeltItemDict[id];
                inventory.Add(itemData);
            }
            else if (itemSubType == Define.ItemSubType.Shoes)
            {
                Data.ArmorItemData itemData = Managers.Data.ShoesItemDict[id];
                inventory.Add(itemData);
            }
            else if (itemSubType == Define.ItemSubType.Ring)
            {
                Data.ArmorItemData itemData = Managers.Data.RingItemDict[id];
                inventory.Add(itemData);
            }
            else if (itemSubType == Define.ItemSubType.Necklace)
            {
                Data.ArmorItemData itemData = Managers.Data.NecklaceItemDict[id];
                inventory.Add(itemData);
            }
            else if (itemSubType == Define.ItemSubType.Bracelet)
            {
                Data.ArmorItemData itemData = Managers.Data.BraceletItemDict[id];
                inventory.Add(itemData);
            }
        }
    }
}
