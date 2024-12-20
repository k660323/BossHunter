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

    #region �ǰ� ó��
    public bool IsAlive { get { return stat.Hp > 0; } }

    public void OnAttacked(Stat attacker, int _hitpriority = 0)
    {
        if (!IsAlive)
            return;

        int damage = Mathf.Max(0, attacker.TotalAttack - stat.TotalDefense);
        stat.Hp -= damage;

        // ����
        if (stat.Hp > 0)
        {
            // ���� �ǰ� ����� �ǰݻ��� �켱���� ���� ���ų� ���� ��� hit ���·�
            if (stat.TotalHitStatePriority <= _hitpriority)
                if (connectionToClient != null)
                    OnHit(connectionToClient);
        }
        // ���
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
            // ���� �ǰ� ����� �ǰݻ��� �켱���� ���� ���ų� ���� ��� hit ���·�
            if (stat.HitStatePriority <= _hitpriority)
                if (connectionToClient != null)
                    OnHit(connectionToClient);
        }
        // ���
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
            // ���� �ǰ� ����� �ǰݻ��� �켱���� ���� ���ų� ���� ��� hit ���·�
            if (stat.HitStatePriority <= _hitpriority)
                if (connectionToClient != null)
                    OnHit(connectionToClient);
        }
        // ���
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
        // ������ �ٵ� �ʱ�ȭ
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        controller.StartAuthority();
        clientInfo.SetCamera(gameObject);

        // �ִϸ��̼� �Ķ���� �ʱ�ȭ
        GetNetAnim.animator.parameters.Initialize();
        // ���� �ʱ�ȭ
        stateMachine.ChangeDefaultState();

        // �÷��̾� UI �������̽��� ������ UI �ʱ�ȭ
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

    // �÷��̾� ���
    [ServerCallback]
    public void S_PlayerDead()
    {
        // ���̾� ����
        gameObject.layer = LayerMask.NameToLayer("PlayerDead");

        // ���� ����ġ 20�۸� ���.
        playerStat.Exp -= playerStat.Exp * 0.2f;
        // ������ ��Ų��.
        RespawnPlayer(12.0f);
    }

    // ������ �÷��̾�
    void RespawnPlayer(float time)
    {
        Target_ShowRespawnPanel(connectionToClient, time);
        StartCoroutine(RespawnPlayerCor(time));
    }

    // ������ �г�
    [TargetRpc]
    void Target_ShowRespawnPanel(NetworkConnection conn, float time)
    {
        Managers.UI.ShowPopupUI<UI_Respawn>().SpawnCorStart(time);
    }

    // ������ ī���� (����)
    IEnumerator RespawnPlayerCor(float time)
    {
        while(time > 0.0f)
        {
            time -= Time.deltaTime;
            yield return null;
        }

        // ��Ȱ
        // ü�� �ʱ�ȭ
        playerStat.RespawnStatInit();
        // ���̾� ����
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

    // ������(Ŭ��)
    [TargetRpc]
    void Target_Respawn(NetworkConnection conn, Vector3 respawnPos)
    {
        // �ִϸ��̼� �Ķ���� �ʱ�ȭ
        GetNetAnim.animator.parameters.Initialize();
        // ���� �ʱ�ȭ
        stateMachine.ChangeDefaultState();
        // ���� ��ġ��
        transform.position = respawnPos;
    }
}
