using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class TownSceneNetwork : BaseSceneNetwork
{
    [Scene, Tooltip("Which scene to send player from here")]
    [SerializeField]
    string destinationScene;

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    protected override void ShowSceneUI()
    {
        Managers.UI.ShowSceneUI<UI_PlayerUtil>();
    }
}
