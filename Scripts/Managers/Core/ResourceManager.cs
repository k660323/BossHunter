using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using Object = UnityEngine.Object;
using Mirror;
using UnityEngine.SceneManagement;

public struct StartSpawnPlayer : NetworkMessage
{
    public string nickName;
    public Define.PlayerType playerType;
}

public struct CreateAndSwapPlayer : NetworkMessage
{
    public Define.PlayerType newType;
}

public struct SwapPlayer : NetworkMessage
{
    public GameObject target;
}

public struct MoveToScene : NetworkMessage
{
    public GameObject player;
    public Define.Scene sceneType;
    public string destinationScene;
}

public class ResourceManager
{
    Dictionary<string, UnityEngine.Object> _resources = new Dictionary<string, UnityEngine.Object>();

    public T Load<T>(string key, bool legacy = false) where T : Object
    {
        if (_resources.TryGetValue(key, out Object resource))
            return resource as T;
        if(legacy)
        {
            T obj = Resources.Load<T>(key);
            _resources.Add(key, obj);
            return obj;
        }

        return null;
    }

    public GameObject Instantiate(string key, Transform parent = null, bool pooling = false, bool legacy = false)
    {
        GameObject prefab = Load<GameObject>($"{key}", legacy);
        if (prefab == null)
        {
            Debug.Log($"Failed to load prefab : {key}");
            return null;
        }

        // Pooling
        if (pooling)
            return Managers.Pool.Pop(prefab);

        GameObject go = Object.Instantiate(prefab, parent);
        go.name = prefab.name;
        return go;
    }

    public GameObject NetInstantiate(string key, Scene scene, bool pooling = false, bool spawn = false)
    {
        GameObject prefab = Load<GameObject>($"{key}");
        if (prefab == null)
        {
            Debug.Log($"Failed to load prefab : {key}");
            return null;
        }

        if(prefab.TryGetComponent(out NetworkIdentity networkIdentity) == false)
        {
            Debug.Log($"Failed to Find NetworkIdentity : {key}");
            return null;
        }

        GameObject go = null;
        // Pooling
        if (pooling)
        {
            go = Managers.Pool.NetPop(prefab);
            go.transform.SetParent(null);
        }
        else
        {
            go = Object.Instantiate(prefab);
            go.name = prefab.name;
        }

        // ���� ������Ʈ�� �ش� scene���� �̵���Ų��.
        Managers.Scene.MoveToScene(go, scene);

        // ������ true�� �ٷ� ��� Ŭ���̾�Ʈ���� ���� ��Ų��.
        if (spawn)
            NetworkServer.Spawn(go);
     
        return go;
    }

    public void Destroy(GameObject go, float time = 0.0f)
    {
        if (go == null)
            return;

        if (Managers.Pool.Push(go))
            return;

        Object.Destroy(go, time);
    }

    public void NetworkDestory(GameObject go, bool isDisconnected = false)
    {
        if (go == null)
            return;

        // Ǯ�� ������ Ŭ��� ������ ���� Ǯ�� �ִ´�.
        if (Managers.Pool.NetPush(go, isDisconnected))
            return;

        // Ǯ ������Ʈ�� �ƴϸ�
        // �����ϰ� ������Ʈ ����
        if (isDisconnected)
            Destroy(go);
        else
            NetworkServer.Destroy(go);
    }

    #region ���� �ε� �� �̵�
    // ó�� �÷��̾� ĳ���� ������ Town���� ����
    public void OnStartSpawnPlayer(NetworkConnectionToClient conn, StartSpawnPlayer message)
    {
        if (conn == null || conn.identity == null)
            return;

        // ���� ������� ���� ������Ʈ
        GameObject oldPlayer = conn.identity.gameObject;
        string currentScene = oldPlayer.scene.name;

        // ���ο� ���� ������Ʈ
        string spawnObjName = Enum.GetName(typeof(Define.PlayerType), message.playerType);
        GameObject newPlayer = Managers.Resource.NetInstantiate(spawnObjName, oldPlayer.scene);
        NetworkClientInfo clientInfo = newPlayer.GetComponentInChildren<NetworkClientInfo>();
        clientInfo.CTS_SetNickName(message.nickName);

        // Town ������ �̵� ��Ų��.
        Vector3 startPos = Managers.Data.SceneInfoDict[(int)Define.Scene.Town]._startPosition;
        Managers.Instance.StartCoroutine(Managers.Scene.SwapPlayerAndSendScene(oldPlayer, newPlayer, currentScene, Enum.GetName(typeof(Define.Scene), Define.Scene.Town), startPos));
    }

    // ���ο� �÷��̾ �����ϰ� �����ϴ� �Լ�
    public void OnCreateAndSwapPlayer(NetworkConnectionToClient conn, CreateAndSwapPlayer message)
    {
        if (conn == null || conn.identity == null)
            return;

        string playerType = Enum.GetName(typeof(Define.Scene), message.newType);

        GameObject oldPlayer = conn.identity.gameObject;
        GameObject newPlayer = Managers.Resource.NetInstantiate(playerType, oldPlayer.scene);

        // ������ �̽�
        UI_NetworkClientInfo oldClientInfo = oldPlayer.GetComponentInChildren<UI_NetworkClientInfo>();
        if (oldClientInfo)
        {
            UI_NetworkClientInfo newClientInfo = newPlayer.GetComponentInChildren<UI_NetworkClientInfo>();
            if (newClientInfo)
                Util.CopyComponent(oldClientInfo, newClientInfo);
        }

        // ���θ��� ���ӿ�����Ʈ�� �������ش�. (���� NetworkServer.Spawn()���� ������ ������Ʈ�� �ƴϸ� �ڵ����� ȣ�����ش�)
        NetworkServer.ReplacePlayerForConnection(conn, newPlayer, true);

        NetworkDestory(oldPlayer, false);  // ��Ʈ��ũ �󿡼� ���� + Ǯ�� ������Ʈ���� �����ϴ� �Լ� �Ѵ� ����
    }

    // �÷��̾� ��ü
    public void OnSwapPlayer(NetworkConnectionToClient conn, SwapPlayer message)
    {
        if (conn == null || conn.identity == null)
            return;

        GameObject oldPlayer = conn.identity.gameObject;
        GameObject target = message.target;

        // ������ �̽�
        UI_NetworkClientInfo oldClientInfo = oldPlayer.GetComponentInChildren<UI_NetworkClientInfo>();
        if (oldClientInfo)
        {
            UI_NetworkClientInfo newClientInfo = target.GetComponentInChildren<UI_NetworkClientInfo>();
            if (newClientInfo)
                Util.CopyComponent(oldClientInfo, newClientInfo);
        }

        // ���� ��ü
        // ���� ������Ʈ�� ������ ��ϵǰ� OnStopServer���� NetworkDestory�� ���ǵǾ� ������ ���� �ƴϸ� ����
        NetworkServer.ReplacePlayerForConnection(conn, target, true);

        NetworkDestory(oldPlayer, false);  // ��Ʈ��ũ �󿡼� ���� + Ǯ�� ������Ʈ���� �����ϴ� �Լ� �Ѵ� ����
    }

    // �� �̵�
    public void OnMoveToScene(NetworkConnectionToClient conn, MoveToScene message)
    {
        GameObject player = message.player;
        if (player == null)
            return;

        string currentScene = player.gameObject.scene.name;
        string destinationScene = message.destinationScene;
        Vector3 startPos = Managers.Data.SceneInfoDict[(int)message.sceneType]._startPosition;
        Managers.Instance.StartCoroutine(Managers.Scene.SendPlayerToNewScene(player, currentScene, destinationScene, startPos));
    }
    #endregion

    #region ��巹����
    public void LoadAsync<T>(string key, Action<T> callback = null) where T : UnityEngine.Object
    {
        // ĳ�� Ȯ��.
        if (_resources.TryGetValue(key, out Object resource))
        {
            callback?.Invoke(resource as T);
            return;
        }

        string loadKey = key;
        if (key.Contains(".sprite"))
            loadKey = $"{key}[{key.Replace(".sprite", "")}]";

        // ���ҽ� �񵿱� �ε� ����.
        var asyncOperation = Addressables.LoadAssetAsync<T>(loadKey);
        asyncOperation.Completed += (op) =>
        {
            if (_resources.ContainsKey(key) == false)
                _resources.Add(key, op.Result);
            callback?.Invoke(op.Result);
        };
    }

    public void LoadAllAsync<T>(string label, Action<string, int, int> callback) where T : UnityEngine.Object
    {
        var opHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
        opHandle.Completed += (op) =>
        {
            int loadCount = 0;
            int totalCount = op.Result.Count;

            foreach (var result in op.Result)
            {
                LoadAsync<T>(result.PrimaryKey, (obj) =>
                {
                    loadCount++;
                    callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                });
            }
        };
    }

    #endregion

    public void Clear()
    {
        Resources.UnloadUnusedAssets();
    }
}
