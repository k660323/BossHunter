using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(NetworkRigidbodyUnreliable))]
[RequireComponent(typeof(Rigidbody))]
public class Projectile : InAnimate
{
    // ������
    protected Creature ownerObject;
    // ����ü ����
    protected Vector3 dir;
    // ������ �ٵ�
    protected Rigidbody rb;
    // �ݶ��̴�
    protected Collider _collider;
    
    // �����ֱ� �ڷ�ƾ
    protected IEnumerator lifeTimeCor;

    // �ߺ� ���� ó�� ���ϰ� �ϴ� �ؽü�
    protected HashSet<GameObject> hitObjects = new HashSet<GameObject>();

    #region ���⿡ �ִ� ���� �����ͼ� ���� ���Ѿ� �Ѵ�.
    // �̵� �ӵ�
    protected float moveSpeed;
    // ���� ������
    protected int damage;
    // ���� �ֱ�
    protected float lifeTime;
    // ��Ʈ �켱����
    protected int hitpriorty;
    // ���� ���̾�
    protected int enemyLayer;
    // ���� ��� Ƚ��
    protected int maxHitCount;
    // ���� ���� ���
    protected int curHitCount;
    #endregion

    [ServerCallback]
    protected virtual void Awake()
    {
        TryGetComponent(out rb);
        if (TryGetComponent(out _collider))
            _collider.enabled = true;
        
    }

    // ����ü ����
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
        // ���� Ÿ�� �ʱ�ȭ
        curHitCount = 0;
        // Ÿ�� ������Ʈ Ŭ����
        hitObjects.Clear();

        lifeTimeCor = LifeTimeCor();
        StartCoroutine(lifeTimeCor);
    }

    // ����ü ����
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
        // ���� Ÿ�� �ʱ�ȭ
        curHitCount = 0;
        // Ÿ�� ������Ʈ Ŭ����
        hitObjects.Clear();

        lifeTimeCor = LifeTimeCor();
        StartCoroutine(lifeTimeCor);
    }

    // ����ü �����ֱ� �ڸ�ƾ
    [ServerCallback]
    public virtual IEnumerator LifeTimeCor()
    {
        while (lifeTime > 0.0f && netIdentity.isServer)
        {
            lifeTime -= Time.deltaTime;
            yield return null;
        }
        // ���� ����� ������Ʈ�� ��� ����
        NetworkServer.UnSpawn(gameObject);
    }
}
