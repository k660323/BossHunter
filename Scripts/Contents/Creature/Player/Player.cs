using Data;
using Mirror;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(PlayerStat))]
[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(Party))]
public abstract class Player : Creature, IHitable
{
    static Player instance;
    public static Player Instance { get { return instance; } }

    protected PlayerController controller;
    public PlayerController GetController { get { return controller; } }

    protected PlayerStat playerStat;
    public PlayerStat GetPlayerStat { get { return playerStat; } }

    protected Party party;
    public Party GetParty { get { return party; } }

    protected NetworkClientInfo clientInfo;
    public NetworkClientInfo GetClientInfo { get { return clientInfo; } }

    protected Define.PlayerType playerType;
    public Define.PlayerType PlayerType { get { return playerType; } }

    protected Inventory inventory;
    public Inventory Inventory { get { return inventory; } }

    protected PlayerStateMachine playerStateMachine;
    public PlayerStateMachine PlayerStateMachine { get { return playerStateMachine; } protected set { playerStateMachine = value; } }

    #region 피격 처리
    public bool IsAlive { get { return stat.Hp > 0; } }

    public void OnAttacked(Stat attacker, int _hitpriority = 0)
    {
        if (!IsAlive)
            return;

        int damage = Mathf.Max(0, attacker.TotalAttack - stat.TotalDefense);
        stat.Hp -= damage;

        // 생존
        if (stat.Hp > 0)
        {
            // 경직 피격 대상의 피격상태 우선순위 보다 같거나 높을 경우 hit 상태로
            if (stat.TotalHitStatePriority <= _hitpriority)
                if (connectionToClient != null)
                    OnHit(connectionToClient);
        }
        // 사망
        else
        {
            OnDead(connectionToClient);
            //stateMachine.ChangeState(Define.State.Dead);
            //NetworkServer.RemovePlayerForConnection(connectionToClient, false);
        }
    }
  

    public void OnAttacked(int _damage, int _hitpriority = 0)
    {
        if (!IsAlive)
            return;

        int damage = _damage - stat.Defense;
        stat.Hp -= damage;

        if (stat.Hp > 0)
        {
            // 경직 피격 대상의 피격상태 우선순위 보다 같거나 높을 경우 hit 상태로
            if (stat.HitStatePriority <= _hitpriority)
                if (connectionToClient != null)
                    OnHit(connectionToClient);
        }
        // 사망
        else
        {
            OnDead(connectionToClient);
            // stateMachine.ChangeState(Define.State.Dead);
        }
    }

    public void OnAttacked(Stat attackerOwner, int _damage, int _hitpriority)
    {
        if (!IsAlive)
            return;

        int damage = _damage - stat.Defense;
        stat.Hp -= damage;

        if (stat.Hp > 0)
        {
            // 경직 피격 대상의 피격상태 우선순위 보다 같거나 높을 경우 hit 상태로
            if (stat.HitStatePriority <= _hitpriority)
                if (connectionToClient != null)
                    OnHit(connectionToClient);
        }
        // 사망
        else
        {
            OnDead(connectionToClient);
            // stateMachine.ChangeState(Define.State.Dead);
        }
    }

    [TargetRpc]
    public void OnHit(NetworkConnection conn)
    {
        stateMachine.ChangeState(Define.State.Hit);
    }

    [TargetRpc]
    public void OnDead(NetworkConnection conn)
    {
        stateMachine.ChangeState(Define.State.Dead);
    }

    #endregion

    protected override void Init(Define.ObjectType _objectType = Define.ObjectType.Player)
    {
        base.Init(_objectType);
        TryGetComponent(out controller);
        TryGetComponent(out inventory);
        TryGetComponent(out party);
        clientInfo = networkObjectInfo as NetworkClientInfo;
        playerStat = stat as PlayerStat;
        playerStateMachine = stateMachine as PlayerStateMachine;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        BaseScene baseScene = Managers.Scene.GetBaseScene(gameObject.scene);
        if (baseScene)
            baseScene.InsertPlayer(netId, gameObject);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (NetworkServer.activeHost == false)
        {
            BaseScene baseScene = Managers.Scene.GetBaseScene(gameObject.scene);
            if (baseScene)
                baseScene.InsertPlayer(netId, gameObject);
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        instance = this;
        // 리지드 바디 초기화
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        controller.StartAuthority();
        clientInfo.SetCamera(gameObject);

        // 애니메이션 파라미터 초기화
        GetNetAnim.animator.parameters.Initialize();
        // 상태 초기화
        stateMachine.ChangeDefaultState();

        // 플레이어 UI 인터페이스가 있으면 UI 초기화
        UI_Scene ui_scene = Managers.UI.SceneUI;
        if(ui_scene is IPlayerUI)
        {
            IPlayerUI playerUI = ui_scene as IPlayerUI;
            playerUI.GetPlayerUI.PlayerUIInfoInit(GetPlayerStat);
            
        }
    }

    public override void OnStopLocalPlayer()
    {
        base.OnStopLocalPlayer();
        instance = null;
        controller.StopAuthority();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (NetworkServer.activeHost == false)
        {
            BaseScene baseScene = Managers.Scene.GetBaseScene(gameObject.scene);
            if (baseScene)
                baseScene.RemovePlayer(cachedNetId);
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        BaseScene baseScene = Managers.Scene.GetBaseScene(gameObject.scene);
        if (baseScene)
            baseScene.RemovePlayer(cachedNetId);
    }

    // 플레이어 사망
    [ServerCallback]
    public void S_PlayerDead()
    {
        // 레이어 변경
        gameObject.layer = LayerMask.NameToLayer("PlayerDead");

        // 현재 경험치 20퍼를 깐다.
        playerStat.Exp -= playerStat.Exp * 0.2f;
        // 리스폰 시킨다.
        RespawnPlayer(12.0f);
    }

    // 리스폰 플레이어
    void RespawnPlayer(float time)
    {
        Target_ShowRespawnPanel(connectionToClient, time);
        StartCoroutine(RespawnPlayerCor(time));
    }

    // 리스폰 패널
    [TargetRpc]
    void Target_ShowRespawnPanel(NetworkConnection conn, float time)
    {
        Managers.UI.ShowPopupUI<UI_Respawn>().SpawnCorStart(time);
    }

    // 리스폰 카운팅 (서버)
    IEnumerator RespawnPlayerCor(float time)
    {
        while(time > 0.0f)
        {
            time -= Time.deltaTime;
            yield return null;
        }

        // 부활
        // 체력 초기화
        playerStat.RespawnStatInit();
        // 레이어 복구
        gameObject.layer = LayerMask.NameToLayer("Player");

        BaseScene baseScene = Managers.Scene.GetBaseScene(gameObject.scene);
        Vector3 spawnPos;
        if (baseScene is BaseInstanceScene instanceScene)
        {
            spawnPos = Managers.Data.InstanceSceneInfoDict[(int)instanceScene.InstanceSceneType]._startPosition;
        }
        else
        {
            spawnPos = Managers.Data.SceneInfoDict[(int)baseScene.SceneType]._startPosition;
        }
        Target_Respawn(connectionToClient, spawnPos);

    }

    // 리스폰(클라)
    [TargetRpc]
    void Target_Respawn(NetworkConnection conn, Vector3 respawnPos)
    {
        // 애니메이션 파라미터 초기화
        GetNetAnim.animator.parameters.Initialize();
        // 상태 초기화
        stateMachine.ChangeDefaultState();
        // 시작 위치로
        transform.position = respawnPos;
    }
}
