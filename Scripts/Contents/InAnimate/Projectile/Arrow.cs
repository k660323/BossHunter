using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : Projectile
{
    TrailRenderer trailRenderer;

    protected override void Awake()
    {
        base.Awake();
        trailRenderer = GetComponentInChildren<TrailRenderer>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        trailRenderer.enabled = true;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        trailRenderer.enabled = false;
    }

    // 이동
    [ServerCallback]
    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + dir * moveSpeed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.LookRotation(dir);
    }

    [ServerCallback]
    protected void OnTriggerEnter(Collider other)
    {
        // 때릴수 있는 오브젝트가 아니면 리턴
        if (other.TryGetComponent(out IHitable hitable) == false)
            return;

        // 레이어가 다르면 리턴
        if (1 << other.gameObject.layer != enemyLayer)
            return;

        // 동일한 대상을 또 처리하지 않음
        if (hitObjects.Contains(other.gameObject))
            return;

        // 해시맵에 대상 추가
        hitObjects.Add(other.gameObject);

        // 공격 카운트
        curHitCount++;

        // 공격 처리
        hitable.OnAttacked(ownerObject.GetStat, damage, hitpriorty);

        // 타수 카운터를 초과시 
        if (curHitCount == maxHitCount)
        {
            // 생명주기 코루틴 정지
            if (lifeTimeCor != null)
                StopCoroutine(lifeTimeCor);

            // 폴링 등록한 오브젝트일 경우 폴링
            NetworkServer.UnSpawn(gameObject);
        }
    }
}
