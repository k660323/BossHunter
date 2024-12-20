using Mirror;
using UnityEngine;

// 몬스터 각종 변수 담당하는 컴포넌트
public class MonsterController : BaseController
{
    public Monster monster { get; private set; }

    // 몬스터가 주시하고 있는 대상
    [SerializeField]
    private GameObject target;
    public GameObject Target 
    { 
        get 
        { 
            return target;
        } 
        set 
        { 
            target = value; 
            if (value) 
            { 
                value.TryGetComponent(out targetCreature); 
            }
            else
            {
                targetCreature = null;
            }
        } 
    }

    [SerializeField]
    private Creature targetCreature;
    public Creature TargetCreature { get {  return targetCreature; } }

    [SerializeField]
    private Vector3 spawnPos;
    public Vector3 SpawnPos { get { return spawnPos; } set { spawnPos = value; } }

    [SerializeField, SyncVar]
    protected Vector3 chaseStartPos;
    public Vector3 ChaseStartPos { get { return chaseStartPos; } set { chaseStartPos = value; } }

    [ServerCallback]
    public override void Init()
    {
        monster = GetComponent<Monster>();
        creature = monster;

    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        target = null;
        spawnPos = transform.position;
        chaseStartPos = transform.position;
    }
}
