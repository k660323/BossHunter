using Mirror;
using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MonsterStateMachine))]
[RequireComponent(typeof(MonsterStat))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(MonsterController))]
public abstract class Monster : Creature
{
    protected MonsterController controller;
    public MonsterController GetController { get { return controller; } }

    protected MonsterStat monsterStat;
    public MonsterStat GetMonsterStat { get { return monsterStat; } }

    protected MonsterStateMachine monsterStateMachine;
    public MonsterStateMachine MonsterStateMachine { get { return monsterStateMachine; } protected set { monsterStateMachine = value; } }

    public Action<Define.MonsterType> destoryAction;

    protected override void Init(Define.ObjectType objectType = Define.ObjectType.Monster)
    {
        base.Init(objectType);
        TryGetComponent(out nav);
        TryGetComponent(out controller);
        monsterStat = stat as MonsterStat;
        monsterStateMachine = stateMachine as MonsterStateMachine;
    }

    // Ŭ�󿡼� ��û�ϰ� �������� �������� �����ɶ� �������� �����ϴ� Start �Լ�
    public override void OnStartServer()
    {
        base.OnStartServer();

        BaseScene baseScene = Managers.Scene.GetBaseScene(gameObject.scene);
        if (baseScene)
            baseScene.InsertMonster(netId, gameObject);

        // ������ �ٵ� �ʱ�ȭ
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        // ���� �ʱ�ȭ
        GetNav.enabled = true;

        // �ִϸ��̼� �Ķ���� �ʱ�ȭ
        GetNetAnim.animator.parameters.Initialize();
        // ���� �ʱ�ȭ
        stateMachine.ChangeDefaultState();

        // ü�� �ʱ�ȭ
        monsterStat.RespawnStatInit();

        // ��ũ��Ʈ Ȱ��ȭ
        this.enabled = true;
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        if(NetworkServer.activeHost == false)
        {
            BaseScene baseScene = Managers.Scene.GetBaseScene(gameObject.scene);
            if (baseScene)
                baseScene.InsertMonster(netId, gameObject);
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (NetworkServer.activeHost == false)
        {
            BaseScene baseScene = Managers.Scene.GetBaseScene(gameObject.scene);
            if (baseScene)
                baseScene.RemoveMonster(cachedNetId);
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        BaseScene baseScene = Managers.Scene.GetBaseScene(gameObject.scene);
        if (baseScene)
            baseScene.RemoveMonster(cachedNetId);

        // ������ٵ� �ʱ�ȭ
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;

        // ��ũ��Ʈ ��Ȱ��ȭ
        this.enabled = false;

        // ���� ��������Ʈ ������ �̺�Ʈ ����
        destoryAction?.Invoke(GetMonsterStat.MonsterType);
        destoryAction = null;

        Managers.Resource.NetworkDestory(gameObject, true);
    }
}
