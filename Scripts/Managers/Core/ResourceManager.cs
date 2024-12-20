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

        // 게임 오브젝트를 해당 scene으로 이동시킨다.
        Managers.Scene.MoveToScene(go, scene);

        // 스폰이 true면 바로 모든 클라이언트에게 스폰 시킨다.
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

        // 풀에 있으면 클라와 연결을 끊고 풀에 넣는다.
        if (Managers.Pool.NetPush(go, isDisconnected))
            return;

        // 풀 오브젝트가 아니면
        // 언스폰하고 오브젝트 제거
        if (isDisconnected)
            Destroy(go);
        else
            NetworkServer.Destroy(go);
    }

    #region 프리 로드 씬 이동
    // 처음 플레이어 캐릭터 스폰후 Town으로 스폰
    public void OnStartSpawnPlayer(NetworkConnectionToClient conn, StartSpawnPlayer message)
    {
        if (conn == null || conn.identity == null)
            return;

        // 현재 사용중인 게임 오브젝트
        GameObject oldPlayer = conn.identity.gameObject;
        string currentScene = oldPlayer.scene.name;

        // 새로운 게임 오브젝트
        string spawnObjName = Enum.GetName(typeof(Define.PlayerType), message.playerType);
        GameObject newPlayer = Managers.Resource.NetInstantiate(spawnObjName, oldPlayer.scene);
        NetworkClientInfo clientInfo = newPlayer.GetComponentInChildren<NetworkClientInfo>();
        clientInfo.CTS_SetNickName(message.nickName);

        // Town 씬으로 이동 시킨다.
        Vector3 startPos = Managers.Data.SceneInfoDict[(int)Define.Scene.Town]._startPosition;
        Managers.Instance.StartCoroutine(Managers.Scene.SwapPlayerAndSendScene(oldPlayer, newPlayer, currentScene, Enum.GetName(typeof(Define.Scene), Define.Scene.Town), startPos));
    }

    // 새로운 플레이어를 생성하고 스왑하는 함수
    public void OnCreateAndSwapPlayer(NetworkConnectionToClient conn, CreateAndSwapPlayer message)
    {
        if (conn == null || conn.identity == null)
            return;

        string playerType = Enum.GetName(typeof(Define.Scene), message.newType);

        GameObject oldPlayer = conn.identity.gameObject;
        GameObject newPlayer = Managers.Resource.NetInstantiate(playerType, oldPlayer.scene);

        // 데이터 이식
        UI_NetworkClientInfo oldClientInfo = oldPlayer.GetComponentInChildren<UI_NetworkClientInfo>();
        if (oldClientInfo)
        {
            UI_NetworkClientInfo newClientInfo = newPlayer.GetComponentInChildren<UI_NetworkClientInfo>();
            if (newClientInfo)
                Util.CopyComponent(oldClientInfo, newClientInfo);
        }

        // 새로만든 게임오브젝트로 연결해준다. (만약 NetworkServer.Spawn()으로 생성된 오브젝트가 아니면 자동으로 호출해준다)
        NetworkServer.ReplacePlayerForConnection(conn, newPlayer, true);

        NetworkDestory(oldPlayer, false);  // 네트워크 상에서 제거 + 풀링 컴포넌트에서 실행하는 함수 둘다 실행
    }

    // 플레이어 교체
    public void OnSwapPlayer(NetworkConnectionToClient conn, SwapPlayer message)
    {
        if (conn == null || conn.identity == null)
            return;

        GameObject oldPlayer = conn.identity.gameObject;
        GameObject target = message.target;

        // 데이터 이식
        UI_NetworkClientInfo oldClientInfo = oldPlayer.GetComponentInChildren<UI_NetworkClientInfo>();
        if (oldClientInfo)
        {
            UI_NetworkClientInfo newClientInfo = target.GetComponentInChildren<UI_NetworkClientInfo>();
            if (newClientInfo)
                Util.CopyComponent(oldClientInfo, newClientInfo);
        }

        // 권한 교체
        // 이전 오브젝트는 폴링에 등록되고 OnStopServer에서 NetworkDestory가 정의되어 있으면 폴링 아니면 삭제
        NetworkServer.ReplacePlayerForConnection(conn, target, true);

        NetworkDestory(oldPlayer, false);  // 네트워크 상에서 제거 + 풀링 컴포넌트에서 실행하는 함수 둘다 실행
    }

    // 씬 이동
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

    #region 어드레서블
    public void LoadAsync<T>(string key, Action<T> callback = null) where T : UnityEngine.Object
    {
        // 캐시 확인.
        if (_resources.TryGetValue(key, out Object resource))
        {
            callback?.Invoke(resource as T);
            return;
        }

        string loadKey = key;
        if (key.Contains(".sprite"))
            loadKey = $"{key}[{key.Replace(".sprite", "")}]";

        // 리소스 비동기 로딩 시작.
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
