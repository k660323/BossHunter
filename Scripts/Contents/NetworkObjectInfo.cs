using Data;
using Mirror;
using System;
using UnityEngine;

public class NetworkObjectInfo : NetworkBehaviour
{
    // UI
    [SerializeField]
    protected UI_NetworkObjectInfo uI_NetworkObjectInfo;
    public UI_NetworkObjectInfo UI_NetworkObjectInfo { get { return uI_NetworkObjectInfo; } set { uI_NetworkObjectInfo = value; } }

    protected Stat targetStat;

    [ClientCallback]
    private void Awake()
    {
        Init();
    }

    public virtual void Init()
    {
        uI_NetworkObjectInfo = GetComponentInChildren<UI_NetworkObjectInfo>();
        uI_NetworkObjectInfo.Init();
        TryGetComponent(out targetStat);
    }

    // Client�� Start�Լ�
    // �÷��̾� ������Ʈ�� Identitiy�� clientStarted = false�� ����Ǵ� �Լ� (NetworkServer.RemovePlayerForConnection(conn, false); �� false�� �ٲ��.)
    // Ŭ���̾�Ʈ ������Ʈ�� ����
    public override void OnStartClient()
    {
        if (uI_NetworkObjectInfo == false)
            return;

        if (targetStat)
        {
            // �̺�Ʈ ���
            targetStat.OnHpAction -= uI_NetworkObjectInfo.UpdateHpBar;
            targetStat.OnHpAction += uI_NetworkObjectInfo.UpdateHpBar;
            targetStat.OnLevelAction -= uI_NetworkObjectInfo.UpdateLevel;
            targetStat.OnLevelAction += uI_NetworkObjectInfo.UpdateLevel;
        }

        // Ŭ�� �ʱ�ȭ�� ī�޶� ������Ʈ �۵�
        uI_NetworkObjectInfo.GetlookAtMainCamera.enabled = true;

        // ü�� �ʱ�ȭ
        uI_NetworkObjectInfo.UpdateHpBar(targetStat.Hp, targetStat.MaxHp);

        // ���� �ʱ�ȭ
        uI_NetworkObjectInfo.UpdateLevel(targetStat.Level);

        // ĵ���� ���̰� ����
        uI_NetworkObjectInfo.GetcanvasGroup.alpha = 1.0f;
    }

    // �÷��̾� ������Ʈ�� Identitiy�� clientStarted = true�� ��쿡 ������ ����� ���ų� �ı��� ����Ǵ� �Լ�
    // Ŭ���̾�Ʈ ������Ʈ�� ����
    public override void OnStopClient()
    {
        base.OnStopClient();
        // Ŭ�� ���� ����� ī�޶� ������Ʈ ����
        uI_NetworkObjectInfo.GetlookAtMainCamera.enabled = false;
        // ĵ���� �� Ȱ��ȭ
        uI_NetworkObjectInfo.GetcanvasGroup.alpha = 0.0f;
    }

    private void OnDestroy()
    {
        targetStat.OnHpAction -= uI_NetworkObjectInfo.UpdateHpBar;
        targetStat.OnLevelAction -= uI_NetworkObjectInfo.UpdateLevel;
    }
}
