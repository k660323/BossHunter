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

    // �̵�
    [ServerCallback]
    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + dir * moveSpeed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.LookRotation(dir);
    }

    [ServerCallback]
    protected void OnTriggerEnter(Collider other)
    {
        // ������ �ִ� ������Ʈ�� �ƴϸ� ����
        if (other.TryGetComponent(out IHitable hitable) == false)
            return;

        // ���̾ �ٸ��� ����
        if (1 << other.gameObject.layer != enemyLayer)
            return;

        // ������ ����� �� ó������ ����
        if (hitObjects.Contains(other.gameObject))
            return;

        // �ؽøʿ� ��� �߰�
        hitObjects.Add(other.gameObject);

        // ���� ī��Ʈ
        curHitCount++;

        // ���� ó��
        hitable.OnAttacked(ownerObject.GetStat, damage, hitpriorty);

        // Ÿ�� ī���͸� �ʰ��� 
        if (curHitCount == maxHitCount)
        {
            // �����ֱ� �ڷ�ƾ ����
            if (lifeTimeCor != null)
                StopCoroutine(lifeTimeCor);

            // ���� ����� ������Ʈ�� ��� ����
            NetworkServer.UnSpawn(gameObject);
        }
    }
}
