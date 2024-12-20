using UnityEngine;
using Mirror;
using static Define;
using UnityEngine.AI;

// �ڵ����� ������Ʈ ��ġ�ϰ� ���� ���� (collider, weapon �� ���� ��ġ ��� ���� �� �ִ� Ŭ���� �̹Ƿ�)
[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(NetworkRigidbodyUnreliable))]
[RequireComponent(typeof(NetworkAnimator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EquipmentManager))]
public abstract class Creature : NetworkBehaviour
{
    // ���� �� ����
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

    // ���� ������ ����ϴ� ������Ʈ
    protected EquipmentManager equipmentManager;
    public EquipmentManager GetEquipment {  get { return equipmentManager; } }


    // ������Ʈ ���� or UI
    protected NetworkObjectInfo networkObjectInfo;
    public NetworkObjectInfo NetworkObjectInfo { get { return networkObjectInfo; } set { networkObjectInfo = value; } }

    // �⺻ ���� ��ġ
    public Transform normalAtkPos;

    // ������Ʈ Ÿ��
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
            Debug.Log("��� �Ŵ����� �����ϴ�. �߰����ּ���.");
#endif
    }

    protected abstract void InitStateMachine();

    // Ŭ�󿡼� ��û�ϰ� �������� �������� �����ɶ� �������� �����ϴ� Start �Լ�
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
