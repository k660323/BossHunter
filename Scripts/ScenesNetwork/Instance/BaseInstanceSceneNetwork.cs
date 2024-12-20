using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public abstract class BaseInstanceSceneNetwork : BaseSceneNetwork
{
    protected BaseInstanceScene baseInstanceScene;
    protected override void Init()
    {
        if(TryGetComponent(out baseInstanceScene))
        {
            baseScene = baseInstanceScene;
        }
    }

    // ¾À È¯°æ ¼³Á¤
    protected override void SetSceneLightingAndShader()
    {
        SceneManager.SetActiveScene(gameObject.scene);
        if (Managers.Data.InstanceSceneLightingDict.TryGetValue((int)baseInstanceScene.InstanceSceneType, out Data.Lighting data))
        {
            // ºû
            OnlineScene onlineScene = Managers.Scene.GetCachedBaseScene(Define.Scene.Online) as OnlineScene;

            Light light = onlineScene.directionLight;
            light.transform.rotation = Quaternion.Euler(data._directionLight._rot);
            light.type = data._directionLight._lightType;
            light.color = data._directionLight._lightColor;
            light.intensity = data._directionLight._intensity;
            light.renderMode = data._directionLight._lightRenderMode;
            light.cullingMask = data._directionLight._cullingMask;
            light.shadowStrength = data._directionLight._shadowStrength;
            light.shadowNearPlane = data._directionLight._shadowNearPlane;
            RenderSettings.sun = light;

            Volume urpPP = onlineScene.urpPostProcessing;
            urpPP.sharedProfile = sharedProfile;
        }
    }
}
