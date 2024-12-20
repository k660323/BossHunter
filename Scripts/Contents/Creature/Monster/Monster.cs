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

    // 클라에서 요청하고 서버에서 스폰한후 생성될때 서버에서 실행하는 Start 함수
    public override void OnStartServer()
    {
        base.OnStartServer();

        BaseScene baseScene = Managers.Scene.GetBaseScene(gameObject.scene);
        if (baseScene)
            baseScene.InsertMonster(netId, gameObject);

        // 리지드 바디 초기화
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        // 내비 초기화
        GetNav.enabled = true;

        // 애니메이션 파라미터 초기화
        GetNetAnim.animator.parameters.Initialize();
        // 상태 초기화
        stateMachine.ChangeDefaultState();

        // 체력 초기화
        monsterStat.RespawnStatInit();

        // 스크립트 활성화
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

        // 리지드바디 초기화
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;

        // 스크립트 비활성화
        this.enabled = false;

        // 삭제 델리케이트 수행후 이벤트 삭제
        destoryAction?.Invoke(GetMonsterStat.MonsterType);
        destoryAction = null;

        Managers.Resource.NetworkDestory(gameObject, true);
    }
}
