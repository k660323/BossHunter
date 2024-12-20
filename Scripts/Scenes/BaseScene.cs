using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
    [SerializeField]
    protected Define.Scene sceneType = Define.Scene.Unknown;
    public virtual Define.Scene SceneType { get { return sceneType; } protected set { sceneType = value; Managers.Scene.SetCachedBaseScene(sceneType, this); } }

    protected BaseSceneNetwork baseSceneNetwork;

    protected Dictionary<uint, GameObject> networkPlayerObjects = new Dictionary<uint, GameObject>();
    protected Dictionary<uint, GameObject> networkMonsterObjects = new Dictionary<uint, GameObject>();
    protected Dictionary<uint, GameObject> networkInAnimateObjects = new Dictionary<uint, GameObject>();

    void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        Managers.Scene.InsertBaseScene(gameObject.scene, this);
        gameObject.transform.SetAsFirstSibling();
        TryGetComponent(out baseSceneNetwork);
    }

    protected virtual void StartLoaded()
    {
        Object obj = GameObject.FindObjectOfType(typeof(EventSystem));
        if (obj == null)
            Managers.Resource.Instantiate("EventSystem.prefab").name = "@EventSystem";

    }

    public virtual T GetSceneNetwork<T>() where T : BaseSceneNetwork
    {
        return baseSceneNetwork as T;
    }

    public virtual void InsertPlayer(uint id, GameObject gameObject)
    {
        networkPlayerObjects.Add(id, gameObject);
    }

    public virtual void ReplacePlayer(uint id, GameObject gameObject)
    {
        if (networkPlayerObjects.ContainsKey(id))
        {
            networkPlayerObjects.Remove(id);
        }

        networkPlayerObjects.Add(id, gameObject);
    }

    public virtual void RemovePlayer(uint id)
    {
        if (networkPlayerObjects.ContainsKey(id))
        {
            networkPlayerObjects.Remove(id);
        }
    }

    public GameObject GetPlayerObject(uint id)
    {
        if (networkPlayerObjects.ContainsKey(id))
        {
            return networkPlayerObjects[id];
        }

        return null;
    }

    public void InsertMonster(uint id, GameObject gameObject)
    {
        networkMonsterObjects.Add(id, gameObject);
    }

    public void ReplaceMonster(uint id, GameObject gameObject)
    {
        if (networkMonsterObjects.ContainsKey(id))
        {
            networkMonsterObjects.Remove(id);
        }

        networkMonsterObjects.Add(id, gameObject);
    }

    public void RemoveMonster(uint id)
    {
        if (networkMonsterObjects.ContainsKey(id))
        {
            networkMonsterObjects.Remove(id);
        }
    }

    public GameObject GetMonsterObject(uint id)
    {
        if (networkMonsterObjects.ContainsKey(id))
        {
            return networkMonsterObjects[id];
        }

        return null;
    }

    public void InsertInAnimate(uint id, GameObject gameObject)
    {
        networkInAnimateObjects.Add(id, gameObject);
    }

    public void ReplaceInAnimate(uint id, GameObject gameObject)
    {
        if (networkInAnimateObjects.ContainsKey(id))
        {
            networkInAnimateObjects.Remove(id);
        }

        networkInAnimateObjects.Add(id, gameObject);
    }

    public void RemoveInAnimate(uint id)
    {
        if (networkInAnimateObjects.ContainsKey(id))
        {
            networkInAnimateObjects.Remove(id);
        }
    }

    public GameObject GetItemObject(uint id)
    {
        if (networkInAnimateObjects.ContainsKey(id))
        {
            return networkInAnimateObjects[id];
        }

        return null;
    }

    protected virtual void OnDestroy()
    {
        networkPlayerObjects.Clear();
        networkMonsterObjects.Clear();
        networkInAnimateObjects.Clear();
        Managers.Scene.RemoveBaseScene(gameObject.scene);
    }
}
