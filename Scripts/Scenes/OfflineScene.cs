using System.Collections;
using UnityEngine;

public class OfflineScene : BaseScene
{
    IEnumerator fadeCoroutine;

    // BaseScene로 받아 LoginScene로 캐스팅 해서 접근한다.
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Offline;
        Managers.Input.IsCursor = true;
        Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, count, totalCount) =>
        {
            // Debug.Log($"{key} {count}/{totalCount}");

            if (count == totalCount)
            {
#if UNITY_SERVER
                StartLoaded();
#else
                    if (fadeCoroutine != null)
                        StopCoroutine(fadeCoroutine);
                    fadeCoroutine = Managers.Instance.FadeInOut.FadeIn();
                    StartCoroutine(fadeCoroutine);
                
                    Managers.Resource.LoadAllAsync<Object>("ClientLoad", (key, count, totalCount) =>
                    {
                        if (count == totalCount)
                        {
                            Managers.Resource.LoadAllAsync<Sprite>("SpriteLoad", (key, count, totalCount) =>
                            {
                                if (count == totalCount)
                                {
                                    StartLoaded();
                                }
                            });
                        }
                    });
#endif
            }
        });
    }

    protected override void StartLoaded()
    {
        base.StartLoaded();

        Managers.Data.Init();

#if UNITY_SERVER
        Managers.Instance.StartServer();
#else
        Managers.Sound.Init();
        Managers.Option.Init();
        Managers.Input.Init();
        Managers.UI.ShowSceneUI<UI_MenuScene>();

        Managers.Sound.Play2D("BGM/Offline", Define.Sound2D.Bgm);

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = Managers.Instance.FadeInOut.FadeOut();
        StartCoroutine(fadeCoroutine);
#endif
    }


}
