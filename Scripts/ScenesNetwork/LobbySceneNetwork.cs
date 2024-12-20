using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbySceneNetwork : BaseSceneNetwork
{
    [Scene, Tooltip("Which scene to send player from here")]
    [SerializeField]
    string destinationScene;

    public override void OnStartServer()
    {
        base.OnStartServer();

    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log("로컬 플레이어");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

    }

    protected override void ShowSceneUI()
    {
        Managers.UI.ShowSceneUI<UI_LobbyScene>();
    }
}
