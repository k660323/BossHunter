using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClientInfo : NetworkObjectInfo
{
    //UI
    [SerializeField]
    protected UI_NetworkClientInfo uI_NetworkClientInfo;
    public UI_NetworkClientInfo UI_NetworkClientInfo { get { return uI_NetworkClientInfo; } set { uI_NetworkClientInfo = value; } }

    [SerializeField, SyncVar(hook = nameof(OnNickNameChanged))]
    protected string nickName = string.Empty;
    public string NickName { get { return nickName; } protected set { nickName = value; } }

    // 닉네임 콜백 액션
    protected Action<string> OnNickNameAction;

    public override void Init()
    {
        base.Init();

        if (uI_NetworkObjectInfo == false)
            return;
        uI_NetworkClientInfo = uI_NetworkObjectInfo as UI_NetworkClientInfo;

        // 이벤트 등록
        OnNickNameAction -= uI_NetworkObjectInfo.UpdateNickName;
        OnNickNameAction += uI_NetworkObjectInfo.UpdateNickName;
    }

    #region NetworkBehaviour 콜백 함수
    public override void OnStartClient()
    {
        base.OnStartClient();

        // 이벤트 등록
        uI_NetworkObjectInfo.UpdateNickName(nickName);

        // 노란색으로 설정
        uI_NetworkObjectInfo.SetHpBarColor(Color.yellow);
    }

    // 플레이어 오브젝트의 Identitiy의 clientStarted = false면 실행되는 함수 (NetworkServer.RemovePlayerForConnection(conn, false); 시 false로 바뀐다.)
    // 클라이언트 로컬 플레이어만 실행
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        // 로컬 클라이언트는 체력바를 표시하지 않는다.
        uI_NetworkObjectInfo.SetHpBarObject(false);
    }

    #endregion

    #region 유틸 함수
    // 클라 전용 함수
    // 카메라 세팅
    [ClientCallback]
    public void SetCamera(GameObject target)
    {
        if (target == null)
            return;

        OnlineScene onlineScene = Managers.Scene.GetCachedBaseScene(Define.Scene.Online) as OnlineScene;
        if (onlineScene)
        {
            onlineScene.camController.SetTarget(target);
        }

    }
    // 서버 전용 함수
    // 해당 오브젝트를 해당 씬으로 이동 시킴
    // NetworkClinetInfo의 OnStartServer에서 호출하는걸 권장
    [ServerCallback]
    public void MoveGameObjectsToScene(ref List<GameObject> gameObjects, string destinationScene)
    {
        // Move player to new subscene.
        for (int i = 0; i < gameObjects.Count; i++)
        {
            SceneManager.MoveGameObjectToScene(gameObjects[i], SceneManager.GetSceneByPath(destinationScene));
        }
    }

    // 서버 전용 함수
    // 해당 오브젝트를 해당 씬으로 이동 시킴
    // NetworkClinetInfo의 OnStartServer에서 호출하는걸 권장
    [ServerCallback]
    public void MoveGameObjectsToScene(ref List<GameObject> gameObjects, Scene scene)
    {
        // Move player to new subscene.
        for (int i = 0; i < gameObjects.Count; i++)
        {
            SceneManager.MoveGameObjectToScene(gameObjects[i], scene);
        }
    }

    // 닉네임 설정
    [ServerCallback]
    public void CTS_SetNickName(string nickName)
    {
        NickName = nickName;
    }

    // 클라이언트 콜백 함수
    // 오브젝트가 있으면 추후 닉네임 변경시 변경
    [ClientCallback]
    void OnNickNameChanged(string _old, string _new)
    {
        OnNickNameAction?.Invoke(_new);

    }
    #endregion

    private void OnDestroy()
    {
        OnNickNameAction -= uI_NetworkObjectInfo.UpdateNickName;
    }
}
