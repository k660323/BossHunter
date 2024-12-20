using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.IO;

public class SceneManagerEx
{
    public string CurSceneName { get; private set; }
    // This is set true after server loads all subscene instances
    public bool subscenesLoaded;

    Dictionary<string, Queue<Scene>> sceneDicQ = new Dictionary<string, Queue<Scene>>();

    // This is managed in LoadAdditive, UnloadAdditive, and checked in OnClientSceneChanged
    public bool isInTransition;

    private BaseScene[] cacheScenes = new BaseScene[(int)Define.Scene.MAX];

    private Dictionary<string, Dictionary<int, BaseScene>> sceneDic = new Dictionary<string, Dictionary<int, BaseScene>>();

    // 오브젝트가 있는 씬에서 원하는 Object를 서치한다.
    // 호스트 서버는 각각 오브젝트가 해당 씬에 들어가지만 클라는 Online씬에 오브젝트가 추가된다.
    public T GetCurrentSceneObject<T>(ref GameObject go) where T : Object
    {
        if (go == null)
            return null;

        Scene currentScene = go.scene;

        GameObject[] allObjects = currentScene.GetRootGameObjects();
        foreach(GameObject obj in allObjects)
        {
            if(obj.TryGetComponent(out T component))
            {
                return component;
            }
        }

        return null;
    }

    // FindObjectType(O(N)) -> 배열(O(1))에 저장하여 시간복잡도를 줄임
    // 해당 Enum값을 넣으면 baseScene을 반환한다
    // 하지만 인스턴스 씬일경우 해당 게임 오브젝트를 받아 서치한다.

    public BaseScene GetCachedBaseScene(Define.Scene scene)
    {
        return cacheScenes[(int)scene];
    }

    public void SetCachedBaseScene(Define.Scene scene, BaseScene baseScene)
    {
        cacheScenes[(int)scene] = baseScene;
    }

    public BaseScene GetBaseScene(string sceneName, int index = 0)
    {
        if (sceneDic.ContainsKey(sceneName))
        {
            if (System.Enum.TryParse(typeof(Define.Scene), sceneName, out System.Object result))
            {
                index = 0;
            }

            Dictionary<int, BaseScene> dic = sceneDic[sceneName];
            for (int i = 0; i < dic.Count; i++)
            {
                if(index == i)
                {
                    return dic[i];
                }
            }
        }

        Debug.LogWarning($"Define Scene Not Define {sceneName}");
        return null;
    }

    public BaseScene GetBaseScene(Scene scene)
    {
        if (scene == null)
            return null;

        string name = scene.name;
        if(sceneDic.ContainsKey(name))
        {
            Dictionary<int, BaseScene> dic = sceneDic[name];
            if(dic.ContainsKey(scene.handle))
                return dic[scene.handle];
        }

        return null;
    }

    public void InsertBaseScene(Scene scene, BaseScene baseScene)
    {
        string name = scene.name;
        if(sceneDic.ContainsKey(name))
        {
            Dictionary<int, BaseScene> dic  = sceneDic[name];
            dic.Add(scene.handle, baseScene);
        }
        else
        {
            Dictionary<int, BaseScene> dic = new Dictionary<int, BaseScene>
            {
                { scene.handle, baseScene }
            };
            sceneDic.Add(name, dic);
        }
    }

    public void RemoveBaseScene(Scene scene)
    {
        string name = scene.name;
        if (sceneDic.ContainsKey(name))
        {
            Dictionary<int, BaseScene> dic = sceneDic[name];
            dic.Remove(scene.handle);

            if(dic.Count == 0)
            {
                sceneDic[name] = null;
                sceneDic.Remove(name);
            }
        }
    }
    
    public bool SceneValid(Scene scene)
    {
        string name = scene.name;
        if (sceneDic.ContainsKey(name))
        {
            Dictionary<int, BaseScene> dic = sceneDic[name];
            return dic.ContainsKey(scene.handle);
        }

        return false;
    }

    #region 로컬 로드

    public IEnumerator LoadScene(Define.Scene type, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (mode == LoadSceneMode.Single)
            yield return Managers.Instance.FadeInOut.FadeIn();

        Managers.Clear();

        SceneManager.LoadScene(Util.GetEnumToString(type), mode);

        if (mode == LoadSceneMode.Single)
            yield return Managers.Instance.FadeInOut.FadeOut();
    }

    // (비동기 로딩)[비동기] 씬 초기화, 씬 추가
    public IEnumerator AsyncLoadScene(Define.Scene type, LoadSceneMode mode = LoadSceneMode.Single, bool isCompletedAndActvie = false)
    {
        if(mode == LoadSceneMode.Single)
            yield return Managers.Instance.FadeInOut.FadeIn();

        Managers.loadingSingleSceneAsync = SceneManager.LoadSceneAsync(Util.GetEnumToString(type), mode);
        Managers.Clear();

        while (Managers.loadingSingleSceneAsync != null && !Managers.loadingSingleSceneAsync.isDone)
            yield return null;

        Managers.loadingSingleSceneAsync.allowSceneActivation = isCompletedAndActvie;

        if (mode == LoadSceneMode.Single)
            yield return Managers.Instance.FadeInOut.FadeOut();
    }

    public IEnumerator AsyncUnLoadScene(Define.Scene type)
    {
        Managers.loadingSingleSceneAsync = SceneManager.UnloadSceneAsync(Util.GetEnumToString(type));
        Managers.Clear();

        while (Managers.loadingSingleSceneAsync != null && !Managers.loadingSingleSceneAsync.isDone)
            yield return null;

        Managers.loadingSingleSceneAsync.allowSceneActivation = true;
    }

    #endregion

    #region 네트워크 로드
    public IEnumerator ServerLoadSubScenes()
    {
        foreach (string additiveScene in Managers.Instance.additiveScenes)
            yield return SceneManager.LoadSceneAsync(additiveScene, new LoadSceneParameters
            {
                loadSceneMode = LoadSceneMode.Additive,
                localPhysicsMode = LocalPhysicsMode.Physics3D // change this to .Physics2D for a 2D game
            });


        subscenesLoaded = true;

        sceneDicQ.Clear();
        SceneManager.sceneLoaded -= InstanceSceneLoaded;
        SceneManager.sceneLoaded += InstanceSceneLoaded;
    }

  
    public void InstanceSceneLoaded(Scene scene , LoadSceneMode mode)
    {
        if(sceneDicQ.ContainsKey(scene.name))
        {
            sceneDicQ[scene.name].Enqueue(scene);
        }
        else
        {
            Queue<Scene> queue = new Queue<Scene>();
            queue.Enqueue(scene);
            sceneDicQ.Add(scene.name, queue);
        }
    }

    public IEnumerator LoadAdditive(string sceneName)
    {
        isInTransition = true;
        // This will return immediately if already faded in
        // e.g. by UnloadAdditive or by default startup state
        Managers.Clear();
        // 파일 경로와 확장자를 제거하고 파일명만 가져온다.
        string[] path = Path.GetFileNameWithoutExtension(sceneName).Split('.');
        CurSceneName = path[0];

        yield return Managers.Instance.FadeInOut.FadeIn();

        // host client is on server...don't load the additive scene again
        if (Managers.Instance.mode == NetworkManagerMode.ClientOnly)
        {
            // Start loading the additive subscene
            Managers.loadingSceneAsync = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (Managers.loadingSceneAsync != null && !Managers.loadingSceneAsync.isDone)
                yield return null;
        }

        // Reset these to false when ready to proceed
        NetworkClient.isLoadingScene = false;
        isInTransition = false;
        
        Managers.Instance.OnClientSceneChanged();

        // Reveal the new scene content.
        yield return Managers.Instance.FadeInOut.FadeOut();
    }

    public IEnumerator UnloadAdditive(string sceneName)
    {
        isInTransition = true;
        // This will return immediately if already faded in
        // e.g. by LoadAdditive above or by default startup state.
        yield return Managers.Instance.FadeInOut.FadeIn();

        // host client is on server...don't unload the additive scene here.
        if (Managers.Instance.mode == NetworkManagerMode.ClientOnly)
        {
            yield return SceneManager.UnloadSceneAsync(sceneName);
            yield return Resources.UnloadUnusedAssets();
        }

        // Reset these to false when ready to proceed
        NetworkClient.isLoadingScene = false;
        isInTransition = false;

        Managers.Instance.OnClientSceneChanged();
        // There is no call to FadeOut here on purpose.
        // Expectation is that a LoadAdditive or full scene change
        // will follow that will call FadeOut after that scene loads.
    }

    #region 프리 로드 씬 이동
    [ServerCallback]
    public IEnumerator SwapPlayerAndSendScene(GameObject oldPlayer, GameObject newPlayer, string currentScene, string destinationScene, Vector3 startPos)
    {
        if (oldPlayer.TryGetComponent(out NetworkIdentity identity))
        {
            NetworkConnectionToClient conn = identity.connectionToClient;
            if (conn == null) yield break;
         
            // Tell client to unload previous subscene with custom handling (see NetworkManager::OnClientChangeScene).
            conn.Send(new SceneMessage { sceneName = currentScene, sceneOperation = SceneOperation.UnloadAdditive, customHandling = true });

            // wait for fader to complete
            yield return Managers.Instance.FadeInOut.GetWaitSeconds();

            // 터레인 수동으로 비활성화
            if (identity.isOwned)
                BaseSceneNetwork.DisableTerrainList();

            // Remove player after fader has completed
            NetworkServer.RemovePlayerForConnection(conn, false);
            Managers.Resource.NetworkDestory(oldPlayer, true);

            // 최상위 루트로 이동
            newPlayer.transform.parent = null;

            // reposition player on server and client
            newPlayer.transform.position = startPos;

            // Rotate player to face center of scene
            // Player is 2m tall with pivot at 0,1,0 so we need to look at
            // 1m height to not tilt the player down to look at origin
            newPlayer.transform.LookAt(Vector3.forward);

            // Move player to new subscene.
            MoveToScene(newPlayer, SceneManager.GetSceneByName(destinationScene));
            
            // Tell client to load the new subscene with custom handling (see NetworkManager::OnClientChangeScene).
            conn.Send(new SceneMessage { sceneName = destinationScene, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });

            // wait for fader to complete
            yield return Managers.Instance.FadeInOut.GetWaitSeconds();

            // Player will be spawned after destination scene is loaded
            NetworkServer.AddPlayerForConnection(conn, newPlayer);

            // host client playerController would have been disabled by OnTriggerEnter above
            // Remote client players are respawned with playerController already enabled
            //if (NetworkClient.localPlayer != null && NetworkClient.localPlayer.TryGetComponent(out PlayerController playerController))
            //    playerController.enabled = true;
        }
    }

    [ServerCallback]
    public IEnumerator SendPlayerToNewScene(GameObject player, string currentScene, string destinationScene, Vector3 startPos)
    {
        if (player.TryGetComponent(out NetworkIdentity identity))
        {
            NetworkConnectionToClient conn = identity.connectionToClient;
            if (conn == null) yield break;

            // Tell client to unload previous subscene with custom handling (see NetworkManager::OnClientChangeScene).
            conn.Send(new SceneMessage { sceneName = currentScene, sceneOperation = SceneOperation.UnloadAdditive, customHandling = true });

            // wait for fader to complete
            yield return Managers.Instance.FadeInOut.GetWaitSeconds();

            // 터레인 수동으로 비활성화
            if (identity.isOwned)
                BaseSceneNetwork.DisableTerrainList();

            // Remove player after fader has completed
            NetworkServer.RemovePlayerForConnection(conn, false);

            // 최상위 루트로 이동
            player.transform.parent = null;
            // reposition player on server and client
            player.transform.position = startPos;

            // Rotate player to face center of scene
            // Player is 2m tall with pivot at 0,1,0 so we need to look at
            // 1m height to not tilt the player down to look at origin
            player.transform.LookAt(Vector3.forward);

            // Move player to new subscene.
            MoveToScene(player, SceneManager.GetSceneByName(destinationScene));

            // Tell client to load the new subscene with custom handling (see NetworkManager::OnClientChangeScene).
            conn.Send(new SceneMessage { sceneName = destinationScene, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });

            // Player will be spawned after destination scene is loaded
            NetworkServer.AddPlayerForConnection(conn, player);

            // host client playerController would have been disabled by OnTriggerEnter above
            // Remote client players are respawned with playerController already enabled
            //if (NetworkClient.localPlayer != null && NetworkClient.localPlayer.TryGetComponent(out PlayerController playerController))
            //    playerController.enabled = true;
        }
    }
    #endregion

    #region 인스턴스 씬 이동
    [ServerCallback]
    public IEnumerator DungeonSceneLoadAdditive(List<GameObject> players, Vector3 startPos, string currentScene, string destinationScene)
    {
        // 캐싱
        NetworkIdentity[] identityArr = new NetworkIdentity[players.Count];
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].TryGetComponent(out NetworkIdentity identity))
            {
                identityArr[i] = identity;

                NetworkConnectionToClient conn = identity.connectionToClient;
                if (conn == null) 
                    continue;

                // Tell client to unload previous subscene with custom handling (see NetworkManager::OnClientChangeScene).
                conn.Send(new SceneMessage { sceneName = currentScene, sceneOperation = SceneOperation.UnloadAdditive, customHandling = true });
            }
        }

        // wait for fader to complete
        yield return Managers.Instance.FadeInOut.GetWaitSeconds();

        // 서버에서 씬 동적 로드
        var asyncScene = SceneManager.LoadSceneAsync(destinationScene, new LoadSceneParameters
        {
            loadSceneMode = LoadSceneMode.Additive,
            localPhysicsMode = LocalPhysicsMode.Physics3D // change this to .Physics2D for a 2D game
        });

        while (asyncScene != null && !asyncScene.isDone)
            yield return null;

        Scene scene = sceneDicQ[destinationScene].Dequeue();

        for (int i = 0; i < players.Count; i++)
        {
            if (identityArr[i] != null)
            {
                NetworkConnectionToClient conn = identityArr[i].connectionToClient;

                // 터레인 수동으로 비활성화 (호스트만)
                if (identityArr[i].isOwned)
                    BaseSceneNetwork.DisableTerrainList();

                // Remove player after fader has completed
                NetworkServer.RemovePlayerForConnection(conn, false);

                // 최상위 루트로 이동
                players[i].transform.parent = null;
                // reposition player on server and client
                players[i].transform.position = startPos;

                players[i].transform.LookAt(Vector3.forward);

                // Move player to new subscene.
                MoveToScene(players[i], scene);

                // Tell client to load the new subscene with custom handling (see NetworkManager::OnClientChangeScene).
                conn.Send(new SceneMessage { sceneName = destinationScene, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });

                // Player will be spawned after destination scene is loaded
                NetworkServer.AddPlayerForConnection(conn, players[i]);
            }
        }
    }
    #endregion

    #endregion

    // 서버나 호스트에서 해당 씬의 물리 캐싱
    public void MoveToScene(GameObject newPlayer, Scene scene)
    {
        SceneManager.MoveGameObjectToScene(newPlayer, scene);

        if (newPlayer.TryGetComponent(out Creature creature))
            creature.GetSetPhysics = scene.GetPhysicsScene();
    }
}
