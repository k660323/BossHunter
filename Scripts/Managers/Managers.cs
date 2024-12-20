using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using UnityEngine.SceneManagement;

public class Managers : NetworkManager
{
    static Managers instacne;
    public static Managers Instance
    {
        get
        {
            Init();
            return instacne;
        }
    }

    #region Contents
    AnimatorHashManager _animHash = new AnimatorHashManager();
    GameManager _game = new GameManager();
    LayerManager _layerManager = new LayerManager();
    ObjectManager _object = new ObjectManager();
    OptionManager _option = new OptionManager();

    public static AnimatorHashManager AnimHash { get { return Instance?._animHash; } }
    public static LayerManager LayerManager { get { return Instance?._layerManager; } }
    public static GameManager Game { get { return Instance?._game; } }
    public static OptionManager Option { get { return Instance?._option; } }
    public static ObjectManager Object { get { return Instance?._object; } }
    #endregion

    #region Core
    DataManager _data = new DataManager();
    InputManager _input = new InputManager();
    PoolManager _pool = new PoolManager();
    ResourceManager _resource = new ResourceManager();
    SceneManagerEx _scene = new SceneManagerEx();
    SoundManager _sound = new SoundManager();
    UIManager _ui = new UIManager();


    public static DataManager Data { get { return Instance?._data; } }
    public static InputManager Input { get { return Instance?._input; } }
    public static PoolManager Pool { get { return Instance?._pool; } }
    public static ResourceManager Resource { get { return Instance?._resource; } }
    public static SceneManagerEx Scene { get { return Instance?._scene; } }
    public static SoundManager Sound { get { return Instance?._sound; } }
    public static UIManager UI { get { return Instance?._ui; } }
    #endregion

    public static UnityEngine.AsyncOperation loadingSingleSceneAsync;

    [Header("추가 씬들 - 시작씬에서 처음")]
    [Scene, Tooltip("추가씬\n첫번째 목록들은 플레이어 시작 씬에서 로드될것이다.")]
    public string[] additiveScenes;

    [Header("페이드 조작 - See child FadeCanvas")]
    [Tooltip("원본 FadeInOut 스크립트는 FadeCanvas의 자식은 안에 있다")]
    private FadeInOut fadeInOut;

    public FadeInOut FadeInOut { 
        get 
        {
            if(fadeInOut == null)
            {
                fadeInOut = GetComponentInChildren<FadeInOut>();
                if (fadeInOut == null)
                    Resource.Instantiate("FadeCanvas", gameObject.transform).TryGetComponent(out fadeInOut);
            }

            return fadeInOut;
        }
    }

    static void Init()
    {
        if (instacne == null)
        {
            GameObject go = GameObject.FindWithTag("@Managers");
            if (go == null)
            {
                //go = new GameObject() { name = "@Managers" };
                //go.tag = "@Managers";
                //go.AddComponent<Managers>();
                return;
            }

            instacne = go.GetComponent<Managers>();
        }
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();

        if (instacne != this)
        {
            Destroy(gameObject);
            return;
        }

#if UNITY_SERVER
        maxConnections = 100;
#else
        maxConnections = 6;
#endif

        _input._playerInput = Util.GetOrAddComponent<PlayerInput>(gameObject);
    }

#if !UNITY_SERVER
    public override void Update()
    {
        base.Update();
        Input.Update();
    }
#endif

    public static void Clear()
    {
        Sound.Clear();
        UI.Clear();
    }
    
    #region 씬 매니저

    /// <summary>
    /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
    /// </summary>
    /// <param name="sceneName">The name of the new scene.</param>
    public override void OnServerSceneChanged(string sceneName)
    {
        // This fires after server fully changes scenes, e.g. offline to online
        // If server has just loaded the Container (online) scene, load the subscenes on server
        if (sceneName == onlineScene)
            StartCoroutine(Scene.ServerLoadSubScenes());
    }

    /// <summary>
    /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
    /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
    /// </summary>
    /// <param name="sceneName">Name of the scene that's about to be loaded</param>
    /// <param name="sceneOperation">Scene operation that's about to happen</param>
    /// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
    public override void OnClientChangeScene(string sceneName, SceneOperation sceneOperation, bool customHandling)
    {
        //Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} OnClientChangeScene {sceneName} {sceneOperation}");
        if (sceneOperation == SceneOperation.UnloadAdditive)
            StartCoroutine(Scene.UnloadAdditive(sceneName));

        if (sceneOperation == SceneOperation.LoadAdditive)
            StartCoroutine(Scene.LoadAdditive(sceneName));
    }

    /// <summary>
    /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
    /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
    /// </summary>
    /// <param name="conn">The network connection that the scene change message arrived on.</param>
    public override void OnClientSceneChanged()
    {
        // Only call the base method if not in a transition.
        // This will be called from LoadAdditive / UnloadAdditive after setting isInTransition to false
        // but will also be called first by Mirror when the scene loading finishes.
        if (!Managers.Scene.isInTransition)
            base.OnClientSceneChanged();
    }

    #endregion

    #region 서버 시스템 콜백

    /// <summary>
    /// Called on the server when a client is ready.
    /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        // This fires from a Ready message client sends to server after loading the online scene
        base.OnServerReady(conn);

        if (conn.identity == null)
            StartCoroutine(AddPlayerDelayed(conn));
    }

    // This delay is mostly for the host player that loads too fast for the
    // server to have subscenes async loaded from OnServerSceneChanged ahead of it.
    IEnumerator AddPlayerDelayed(NetworkConnectionToClient conn)
    {
        // Wait for server to async load all subscenes for game instances
        while (!Scene.subscenesLoaded)
            yield return null;

        // Send Scene msg to client telling it to load the first additive scene
        conn.Send(new SceneMessage { sceneName = additiveScenes[0], sceneOperation = SceneOperation.LoadAdditive, customHandling = true });

        // We have Network Start Positions in first additive scene...pick one
        Transform start = GetStartPosition();

        // Instantiate player as child of start position - this will place it in the additive scene
        // This also lets player object "inherit" pos and rot from start position transform
        
        GameObject player = Resource.NetInstantiate(playerPrefab.name, start.gameObject.scene, true);
        
        // now set parent null to get it out from under the Start Position object
        //player.transform.SetParent(null);
        // Wait for end of frame before adding the player to ensure Scene Message goes first
        yield return new WaitForEndOfFrame();

        // Finally spawn the player object for this connection
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    #endregion

    #region 커스텀 메시지
    public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkServer.RegisterHandler<StartSpawnPlayer>(Resource.OnStartSpawnPlayer);
        NetworkServer.RegisterHandler<CreateAndSwapPlayer>(Resource.OnCreateAndSwapPlayer);
        NetworkServer.RegisterHandler<SwapPlayer>(Resource.OnSwapPlayer);
        NetworkServer.RegisterHandler<MoveToScene>(Resource.OnMoveToScene);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        NetworkServer.UnregisterHandler<StartSpawnPlayer>();
        NetworkServer.UnregisterHandler<CreateAndSwapPlayer>();
        NetworkServer.UnregisterHandler<SwapPlayer>();
        NetworkServer.UnregisterHandler<MoveToScene>();
    }

    #endregion

    // 새로운 클라이언트가 접속하면 콜백함수로 호출된다.
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        Debug.Log("Client Connect");
    }

    // 접속된 클라이언트가 연결을 해제하면 콜백함수로 호출된다.
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log("Client DisConnected");

    }

    public override void OnServerError(NetworkConnectionToClient conn, TransportError error, string reason)
    {
        base.OnServerError(conn, error, reason);
        Debug.Log("에러 발생 : " + error.ToString() + "Reseon : " + reason);
    }

    public override void OnClientError(TransportError error, string reason)
    {
        base.OnClientError(error, reason);

        Debug.Log("에러 발생 : " + reason);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        StartCoroutine(Managers.Instance.fadeInOut.FadeOut());
    }
     
    // 만약 씬이 로딩되지 않았을때 서버에서 요청이 들어오면 해당 클라이언트에서 호출되는 함수
    public override void OnClientNotReady()
    {
        base.OnClientNotReady();
        Debug.Log("클라 준비되지 않음");
    }
}