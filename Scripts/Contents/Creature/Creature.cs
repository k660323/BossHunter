using UnityEngine;
using Mirror;
using static Define;
using UnityEngine.AI;

// 자동으로 컴포넌트 배치하고 삭제 방지 (collider, weapon 은 수동 배치 상속 받을 수 있는 클래스 이므로)
[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(NetworkRigidbodyUnreliable))]
[RequireComponent(typeof(NetworkAnimator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EquipmentManager))]
public abstract class Creature : NetworkBehaviour
{
    // 현재 씬 물리
    protected PhysicsScene physics;
    public PhysicsScene GetSetPhysics {  get { return physics; } set { physics = value; } }

    protected Rigidbody rb;
    public Rigidbody GetRigidBody { get { return rb; } }

    protected NetworkAnimator netAnim;
    public NetworkAnimator GetNetAnim { get { return netAnim; } }

    protected Collider col;
    public Collider GetCollider { get { return col; } }

    protected Stat stat;
    public Stat GetStat { get { return stat; } }

    protected StateMachine stateMachine;
    public StateMachine StateMachine { get { return stateMachine; } protected set { stateMachine = value; } }

    protected NavMeshAgent nav;
    public NavMeshAgent GetNav { get { return nav; } }

    // 무기 정보를 담당하는 컴포넌트
    protected EquipmentManager equipmentManager;
    public EquipmentManager GetEquipment {  get { return equipmentManager; } }


    // 오브젝트 정보 or UI
    protected NetworkObjectInfo networkObjectInfo;
    public NetworkObjectInfo NetworkObjectInfo { get { return networkObjectInfo; } set { networkObjectInfo = value; } }

    // 기본 공격 위치
    public Transform normalAtkPos;

    // 오브젝트 타입
    public ObjectType ObjectType { get; protected set; }

    // netID
    protected uint cachedNetId;

    void Awake()
    {
        Init();
        InitStateMachine();
    }

    protected virtual void Init(ObjectType _objectType = ObjectType.Unknown)
    {
        ObjectType = _objectType;
        TryGetComponent(out rb);
        TryGetComponent(out netAnim);
        TryGetComponent(out col);
        TryGetComponent(out stat);
        TryGetComponent(out stateMachine);
        networkObjectInfo = GetComponentInChildren<NetworkObjectInfo>();
        equipmentManager = GetComponentInChildren<EquipmentManager>();

#if UNITY_EDITOR
        if (equipmentManager == null)
            Debug.Log("장비 매니저가 없습니다. 추가해주세요.");
#endif
    }

    protected abstract void InitStateMachine();

    // 클라에서 요청하고 서버에서 스폰한후 생성될때 서버에서 실행하는 Start 함수
    public override void OnStartServer()
    {
        base.OnStartServer();
        physics = gameObject.scene.GetPhysicsScene();
        cachedNetId = netId;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (Managers.Instance.mode != NetworkManagerMode.Host)
        {
            physics = gameObject.scene.GetPhysicsScene();
            cachedNetId = netId;
        }
    }

    [Command]
    public void CTS_Collider(bool isActive)
    {
        RPC_Collider(isActive);
    }

    [ClientRpc(includeOwner = false)]
    public void RPC_Collider(bool isActive)
    {
        if (GetCollider)
            GetCollider.enabled = isActive;
    }
}
