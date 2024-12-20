using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Tweens;

public class BombSeeds : Projectile
{
    [SerializeField]
    string boomCreateSound;
    [SerializeField]
    string boomSound;
    [SerializeField]
    string boomEffect;

    Collider[] col = new Collider[1];

    [ServerCallback]
    public void SetBoomInfo(Vector3 startPos, Quaternion qut, Vector3 _dir, float minPower, float maxPower, float _moveSpeed, int _damage, float _lifeTime, int _hitpriorty, int _enemyLayer, int _hitCount = 1, Creature _ownerObject = null)
    {
        base.SetProjectileInfo(startPos, qut, _dir, _moveSpeed, _damage, _lifeTime, _hitpriorty, _enemyLayer, _hitCount, _ownerObject);
        col.Initialize();
        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        rb.AddForce((transform.forward + transform.up) * Random.Range(minPower, maxPower), ForceMode.Impulse);
        BoomCreateSound();

    }

    [ServerCallback]
    protected void OnTriggerEnter(Collider other)
    {
        // ���� �浹�� ���� ������ ����
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            rb.velocity = Vector3.zero;
            rb.useGravity = false;
            return;
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // ���� ����Ʈ ����
            BoomEffect();
            // ���� ���� ����
            BoomSound();
            // ������ �ش�.
            Boom();
            // ������Ʈ ����
            // �����ֱ� �ڷ�ƾ ����
            if (lifeTimeCor != null)
                StopCoroutine(lifeTimeCor);
            // ���� ����� ������Ʈ�� ��� ����
            NetworkServer.UnSpawn(gameObject);
        }
    }

    public void BoomEffect()
    {
        BaseScene baseScene = Managers.Scene.GetBaseScene(gameObject.scene);
        if (baseScene)
        {
            baseScene.GetSceneNetwork<BaseSceneNetwork>().RPC_Effect(boomEffect, transform.position, transform.rotation);
        }
    }

    public void BoomCreateSound()
    {
        BaseScene baseScene = Managers.Scene.GetBaseScene(gameObject.scene);
        if (baseScene)
        {
            baseScene.GetSceneNetwork<BaseSceneNetwork>().RPC_Play3D(boomCreateSound, transform.position, Define.Sound3D.Effect3D, 0.5f, 1, 10, 25);
        }
    }

    public void BoomSound()
    {
        BaseScene baseScene = Managers.Scene.GetBaseScene(gameObject.scene);
        if (baseScene)
        {
            baseScene.GetSceneNetwork<BaseSceneNetwork>().RPC_Play3D(boomSound, transform.position, Define.Sound3D.Effect3D, 0.5f, 1, 10, 25);
        }
    }

    // ���� ����
    void Boom()
    {
        if(physicsScene.OverlapSphere(transform.position, 5.0f, col, Managers.LayerManager.Player, QueryTriggerInteraction.Ignore) > 0)
        {
            for(int i=0; i< col.Length; i++)
            {
                if (col[i] == null)
                    return;

                if (col[i].TryGetComponent(out Player player) == false)
                    continue;

                if(player is IHitable hitable)
                {
                    hitable.OnAttacked(ownerObject.GetStat, damage, hitpriorty);
                }
                
            }
        }
    }

    // ����ü �����ֱ� �ڸ�ƾ
    [ServerCallback]
    public override IEnumerator LifeTimeCor()
    {
        while (lifeTime > 0.0f && netIdentity.isServer)
        {
            lifeTime -= Time.deltaTime;
            yield return null;
        }

        // ���� ����Ʈ ����
        BoomEffect();
        // ���� ���� ����
        BoomSound();
        // ������ �ش�.
        Boom();
        // ���� ����� ������Ʈ�� ��� ����
        NetworkServer.UnSpawn(gameObject);
    }
}
