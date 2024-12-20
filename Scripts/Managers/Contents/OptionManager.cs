using Mirror.Examples.AdditiveLevels;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GraphicOption
{
    public int width;
    public int height;
    public int minRefreshRate = 30;
    public int maxRefreshRate = 600;
    public int refreshRate;
    public bool isFullScreen;

    public bool isSync;
    public bool isFrame;
    public int targetFrame;
    public int graphicQualityIndex;

    public void Init()
    {
        // 최소 사양
        width = Screen.currentResolution.width;
        height = Screen.currentResolution.height;
        refreshRate = Screen.currentResolution.refreshRate;
        isFullScreen = Screen.fullScreen;

        isSync = false;
        isFrame = false;
        targetFrame = 60;
        graphicQualityIndex = 0;

        //string data = JsonUtility.ToJson(this);
        //using (var writer = File.CreateText($"Assets/Resources/Data/Setting.txt"))
        //{
        //    writer.WriteLine(data);
        //}
    }

    public void ApplyOption(bool isSave = true)
    {
        Screen.SetResolution(width, height, isFullScreen, refreshRate);
        QualitySettings.vSyncCount = isSync == true ? 1 : 0;
        Managers.Option.FpsObject.gameObject.SetActive(isFrame);
        Application.targetFrameRate = targetFrame;
        QualitySettings.SetQualityLevel(graphicQualityIndex);

        if (isSave)
        {
            string data = JsonUtility.ToJson(this);
            // File.WriteAllText($"Assets/Resources/Data/GraphicSetting.txt", data);
            File.WriteAllText($"{Application.persistentDataPath}/GraphicSetting.txt", data);
        }
    }

    public void ApplyOption(int _width, int _height, bool _isFullScreen, int _refreshRate, bool _vSync, bool _fream, int _targetFrame, int _graphicQualityIndex)
    {
        width = _width;
        height = _height;
        isFullScreen = _isFullScreen;
        refreshRate = _refreshRate;
        isSync = _vSync;
        isFrame = _fream;
        targetFrame = _targetFrame;
        graphicQualityIndex = _graphicQualityIndex;

        ApplyOption();
    }
}

public class SoundOption
{
    public int volMin = 0;
    public int volMax = 100;

    public int masterVol;
    public int backgroundVol;
    public int effectVol;

    public void Init()
    {
        masterVol = 100;
        backgroundVol = 50;
        effectVol = 50;
    }

    public void ApplyOption(bool isSave = true)
    {
        float mVol = masterVol == 0 ? -80 : Mathf.Log10(masterVol / 100f) * 20;
        float bVol = backgroundVol == 0 ? -80 : Mathf.Log10(backgroundVol / 100f) * 20;
        float eVol = effectVol == 0 ? -80 : Mathf.Log10(effectVol / 100f) * 20;

        Managers.Sound.Mixer.SetFloat("MasterSound", mVol);
        Managers.Sound.Mixer.SetFloat("BackgroundSound", bVol);
        Managers.Sound.Mixer.SetFloat("EffectSound", eVol);

        if (isSave)
        {
            string data = JsonUtility.ToJson(this);
            File.WriteAllText($"{Application.persistentDataPath}/SoundVol.txt", data);
        }
    }

    public void ApplyOption(int _masterVol, int _backgroundVol, int _effectVol)
    {
        masterVol = _masterVol;
        backgroundVol = _backgroundVol;
        effectVol = _effectVol;
        ApplyOption(false);
    }
}

public class GamePlayOption
{
    public int mouseHVMin = 1;
    public int mouseHVMax = 100;
    public int mouseHorizontal;
    public int mouseVirtical;

    public int wheelMin = 1;
    public int wheelMax = 5;
    public int wheel;

    public void Init()
    {
        mouseHorizontal = 50;
        mouseVirtical = 50;
        wheel = 1;
    }

    public void ApplyOption(bool isSave = true)
    {
        if (isSave)
        {
            string data = JsonUtility.ToJson(this);
            File.WriteAllText($"{Application.persistentDataPath}/GamePlaySetting.txt", data);
        }
    }

    public void ApplyOption(int _mouseHorizontal, int _mouseVirtical, int _zoom)
    {
        mouseHorizontal = _mouseHorizontal;
        mouseVirtical = _mouseVirtical;
        wheel = _zoom;
    }
}

public class OptionManager
{
    FPSCounter fpsObject;
    public FPSCounter FpsObject
    {
        get
        {
            if (fpsObject == null)
            {
                fpsObject = Managers.Instance.gameObject.GetComponentInChildren<FPSCounter>();
                if (fpsObject == null)
                    Managers.Resource.Instantiate("FPSCounter",Managers.Instance.gameObject.transform).TryGetComponent(out fpsObject);
            }
            return fpsObject;
        }
    }

    public GraphicOption gOption = new GraphicOption();
    public SoundOption sOption = new SoundOption();
    public GamePlayOption gamePlayOption = new GamePlayOption();

    public void Init()
    {
        if (File.Exists($"{Application.persistentDataPath}/GraphicSetting.txt"))
        {
            string graphicSetting = File.ReadAllText($"{Application.persistentDataPath}/GraphicSetting.txt");
            gOption = JsonUtility.FromJson<GraphicOption>(graphicSetting);
            gOption.ApplyOption(false);
        }
        else
        {
            gOption.Init();
            gOption.ApplyOption();
        }

        if (File.Exists($"{Application.persistentDataPath}/SoundVol.txt"))
        {
            string soundSetting = File.ReadAllText($"{Application.persistentDataPath}/SoundVol.txt"); ;
            sOption = JsonUtility.FromJson<SoundOption>(soundSetting);
            sOption.ApplyOption(false);
        }
        else
        {
            sOption.Init();
            sOption.ApplyOption();
        }


        if (File.Exists($"{Application.persistentDataPath}/GamePlaySetting.txt"))
        {
            string gamePlaySetting = File.ReadAllText($"{Application.persistentDataPath}/GamePlaySetting.txt"); ;
            gamePlayOption = JsonUtility.FromJson<GamePlayOption>(gamePlaySetting);
            gamePlayOption.ApplyOption(false);
        }
        else
        {
            gamePlayOption.Init();
            gamePlayOption.ApplyOption();
        }
    }
}
