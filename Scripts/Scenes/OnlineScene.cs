using UnityEngine;
using UnityEngine.Rendering;

public class OnlineScene : BaseScene
{
    public Volume urpPostProcessing;
    public GameObject cam;
    public CameraController camController;
    public Light directionLight;

    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Online;
#if UNITY_SERVER
    urpPostProcessing.gameObject.SetActive(false);
    cam.SetActive(false);
    camController.gameObject.SetActive(false);
    directionLight.gameObject.SetActive(false);
#endif

    }
}
