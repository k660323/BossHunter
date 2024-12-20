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

    // �г��� �ݹ� �׼�
    protected Action<string> OnNickNameAction;

    public override void Init()
    {
        base.Init();

        if (uI_NetworkObjectInfo == false)
            return;
        uI_NetworkClientInfo = uI_NetworkObjectInfo as UI_NetworkClientInfo;

        // �̺�Ʈ ���
        OnNickNameAction -= uI_NetworkObjectInfo.UpdateNickName;
        OnNickNameAction += uI_NetworkObjectInfo.UpdateNickName;
    }

    #region NetworkBehaviour �ݹ� �Լ�
    public override void OnStartClient()
    {
        base.OnStartClient();

        // �̺�Ʈ ���
        uI_NetworkObjectInfo.UpdateNickName(nickName);

        // ��������� ����
        uI_NetworkObjectInfo.SetHpBarColor(Color.yellow);
    }

    // �÷��̾� ������Ʈ�� Identitiy�� clientStarted = false�� ����Ǵ� �Լ� (NetworkServer.RemovePlayerForConnection(conn, false); �� false�� �ٲ��.)
    // Ŭ���̾�Ʈ ���� �÷��̾ ����
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        // ���� Ŭ���̾�Ʈ�� ü�¹ٸ� ǥ������ �ʴ´�.
        uI_NetworkObjectInfo.SetHpBarObject(false);
    }

    #endregion

    #region ��ƿ �Լ�
    // Ŭ�� ���� �Լ�
    // ī�޶� ����
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
    // ���� ���� �Լ�
    // �ش� ������Ʈ�� �ش� ������ �̵� ��Ŵ
    // NetworkClinetInfo�� OnStartServer���� ȣ���ϴ°� ����
    [ServerCallback]
    public void MoveGameObjectsToScene(ref List<GameObject> gameObjects, string destinationScene)
    {
        // Move player to new subscene.
        for (int i = 0; i < gameObjects.Count; i++)
        {
            SceneManager.MoveGameObjectToScene(gameObjects[i], SceneManager.GetSceneByPath(destinationScene));
        }
    }

    // ���� ���� �Լ�
    // �ش� ������Ʈ�� �ش� ������ �̵� ��Ŵ
    // NetworkClinetInfo�� OnStartServer���� ȣ���ϴ°� ����
    [ServerCallback]
    public void MoveGameObjectsToScene(ref List<GameObject> gameObjects, Scene scene)
    {
        // Move player to new subscene.
        for (int i = 0; i < gameObjects.Count; i++)
        {
            SceneManager.MoveGameObjectToScene(gameObjects[i], scene);
        }
    }

    // �г��� ����
    [ServerCallback]
    public void CTS_SetNickName(string nickName)
    {
        NickName = nickName;
    }

    // Ŭ���̾�Ʈ �ݹ� �Լ�
    // ������Ʈ�� ������ ���� �г��� ����� ����
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
