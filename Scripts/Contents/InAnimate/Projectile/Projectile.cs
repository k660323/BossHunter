using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(NetworkRigidbodyUnreliable))]
[RequireComponent(typeof(Rigidbody))]
public class Projectile : InAnimate
{
    // 소유자
    protected Creature ownerObject;
    // 투사체 방향
    protected Vector3 dir;
    // 리지드 바디
    protected Rigidbody rb;
    // 콜라이더
    protected Collider _collider;
    
    // 생명주기 코루틴
    protected IEnumerator lifeTimeCor;

    // 중복 공격 처리 못하게 하는 해시셋
    protected HashSet<GameObject> hitObjects = new HashSet<GameObject>();

    #region 무기에 있는 스펙 가져와서 적용 시켜야 한다.
    // 이동 속도
    protected float moveSpeed;
    // 공격 데미지
    protected int damage;
    // 생명 주기
    protected float lifeTime;
    // 히트 우선순위
    protected int hitpriorty;
    // 적대 레이어
    protected int enemyLayer;
    // 때릴 대상 횟수
    protected int maxHitCount;
    // 현재 때린 대상
    protected int curHitCount;
    #endregion

    [ServerCallback]
    protected virtual void Awake()
    {
        TryGetComponent(out rb);
        if (TryGetComponent(out _collider))
            _collider.enabled = true;
        
    }

    // 투사체 설정
    [ServerCallback]
    public virtual void SetProjectileInfo(Vector3 startPos, Vector3 _dir, float _moveSpeed, int _damage, float _lifeTime, int _hitpriorty, int _enemyLayer, int _hitCount = 1, Creature _ownerObject = null)
    {
        transform.position = startPos;
        transform.rotation = Quaternion.LookRotation(_dir);
        dir = _dir;
        moveSpeed = _moveSpeed;
        damage = _damage;
        lifeTime = _lifeTime;
        hitpriorty = _hitpriorty;
        enemyLayer = _enemyLayer;
        maxHitCount = _hitCount;
        ownerObject = _ownerObject;

        NetworkServer.Spawn(gameObject);
        // 현재 타수 초기화
        curHitCount = 0;
        // 타격 오브젝트 클리어
        hitObjects.Clear();

        lifeTimeCor = LifeTimeCor();
        StartCoroutine(lifeTimeCor);
    }

    // 투사체 설정
    [ServerCallback]
    public virtual void SetProjectileInfo(Vector3 startPos, Quaternion qut, Vector3 _dir, float _moveSpeed, int _damage, float _lifeTime, int _hitpriorty, int _enemyLayer, int _hitCount = 1, Creature _ownerObject = null)
    {
        transform.position = startPos;
        transform.rotation = qut;
        dir = _dir;
        moveSpeed = _moveSpeed;
        damage = _damage;
        lifeTime = _lifeTime;
        hitpriorty = _hitpriorty;
        enemyLayer = _enemyLayer;
        maxHitCount = _hitCount;
        ownerObject = _ownerObject;

        NetworkServer.Spawn(gameObject);
        // 현재 타수 초기화
        curHitCount = 0;
        // 타격 오브젝트 클리어
        hitObjects.Clear();

        lifeTimeCor = LifeTimeCor();
        StartCoroutine(lifeTimeCor);
    }

    // 투사체 생명주기 코르틴
    [ServerCallback]
    public virtual IEnumerator LifeTimeCor()
    {
        while (lifeTime > 0.0f && netIdentity.isServer)
        {
            lifeTime -= Time.deltaTime;
            yield return null;
        }
        // 폴링 등록한 오브젝트일 경우 폴링
        NetworkServer.UnSpawn(gameObject);
    }
}
