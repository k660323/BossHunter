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

    // Client의 Start함수
    // 플레이어 오브젝트의 Identitiy의 clientStarted = false면 실행되는 함수 (NetworkServer.RemovePlayerForConnection(conn, false); 시 false로 바뀐다.)
    // 클라이언트 오브젝트만 실행
    public override void OnStartClient()
    {
        if (uI_NetworkObjectInfo == false)
            return;

        if (targetStat)
        {
            // 이벤트 등록
            targetStat.OnHpAction -= uI_NetworkObjectInfo.UpdateHpBar;
            targetStat.OnHpAction += uI_NetworkObjectInfo.UpdateHpBar;
            targetStat.OnLevelAction -= uI_NetworkObjectInfo.UpdateLevel;
            targetStat.OnLevelAction += uI_NetworkObjectInfo.UpdateLevel;
        }

        // 클라 초기화시 카메라 컴포넌트 작동
        uI_NetworkObjectInfo.GetlookAtMainCamera.enabled = true;

        // 체력 초기화
        uI_NetworkObjectInfo.UpdateHpBar(targetStat.Hp, targetStat.MaxHp);

        // 레벨 초기화
        uI_NetworkObjectInfo.UpdateLevel(targetStat.Level);

        // 캔버스 보이게 설정
        uI_NetworkObjectInfo.GetcanvasGroup.alpha = 1.0f;
    }

    // 플레이어 오브젝트의 Identitiy의 clientStarted = true일 경우에 서버의 통신을 끊거나 파괴시 실행되는 함수
    // 클라이언트 오브젝트만 실행
    public override void OnStopClient()
    {
        base.OnStopClient();
        // 클라 연결 끊길시 카메라 컴포넌트 정지
        uI_NetworkObjectInfo.GetlookAtMainCamera.enabled = false;
        // 캔버스 비 활성화
        uI_NetworkObjectInfo.GetcanvasGroup.alpha = 0.0f;
    }

    private void OnDestroy()
    {
        targetStat.OnHpAction -= uI_NetworkObjectInfo.UpdateHpBar;
        targetStat.OnLevelAction -= uI_NetworkObjectInfo.UpdateLevel;
    }
}
