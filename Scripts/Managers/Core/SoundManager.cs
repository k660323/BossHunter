using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager
{
    AudioMixer mixer;
    public AudioMixer Mixer
    {
        get
        {
            if (mixer == null)
            {
                mixer = Managers.Resource.Load<AudioMixer>("MasterAudioMixer");
            }

            return mixer;
        }
    }

    AudioSource[] _audioSources2D = new AudioSource[(int)Define.Sound2D.MaxCount];
    
    const int audio3DCount = 60;
    public Queue<AudioSource3D> _audioSources3DQueue = new Queue<AudioSource3D>();
    public Queue<AudioSource3D> _usingAudioSourcesQueue = new Queue<AudioSource3D>();
    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
    // MP3 Player -> AudioSource
    // MP3 À½¿ø -> AudioClip
    // °ü°´(±Í) -> AudioListener

    public void Init()
    {
        GameObject root = GameObject.FindWithTag("@Sounds");
     
        if(root == null)
        {
            root = new GameObject { name = "@Sounds" };
            root.tag = "@Sounds";
            Object.DontDestroyOnLoad(root);

            Init2DSound(root);
            Init3DSound(root);
        }
        else
        {
            _audioSources3DQueue.Clear();
            _usingAudioSourcesQueue.Clear();
            _audioClips.Clear();

            Transform Sound2D = root.transform.Find("@Sound2D");
            for (int i = 0; i < Sound2D.childCount; i++)
            {
                if (Sound2D.GetChild(i).TryGetComponent(out AudioSource audioSource))
                    _audioSources2D[i] = audioSource;
            }

            Transform Sound3D = root.transform.Find("@Sound3D");
            for (int i = 0; i < Sound3D.childCount; i++)
            {
                if (Sound3D.GetChild(i).TryGetComponent(out AudioSource3D audioSource))
                    _audioSources3DQueue.Enqueue(audioSource);
            }
        }
    }

    void Init2DSound(GameObject root)
    {
        GameObject Sound2D = new GameObject { name = "@Sound2D" };
        Sound2D.transform.parent = root.transform;

        string[] soundNames2D = System.Enum.GetNames(typeof(Define.Sound2D));
        for (int i = 0; i < soundNames2D.Length - 1; i++)
        {
            GameObject go = new GameObject { name = soundNames2D[i] };
            _audioSources2D[i] = go.AddComponent<AudioSource>();
            _audioSources2D[i].volume = 1.0f;
            go.transform.parent = Sound2D.transform;
        }

        _audioSources2D[(int)Define.Sound2D.Effect2D].outputAudioMixerGroup = Mixer.FindMatchingGroups("Effect")[0];
        _audioSources2D[(int)Define.Sound2D.Bgm].outputAudioMixerGroup = Mixer.FindMatchingGroups("Background")[0];
        _audioSources2D[(int)Define.Sound2D.Bgm].loop = true;
    }

    void Init3DSound(GameObject root)
    {
        GameObject Sound3D = new GameObject { name = "@Sound3D" };
        Sound3D.transform.parent = root.transform;

        string soundName3D = System.Enum.GetName(typeof(Define.Sound3D), 0);
        for (int i = 0; i < audio3DCount; i++)
        {
            GameObject go = new GameObject { name = soundName3D };
            go.transform.parent = Sound3D.transform;

            AudioSource3D audioSource3D = go.AddComponent<AudioSource3D>();
            audioSource3D.audioSource.outputAudioMixerGroup = Mixer.FindMatchingGroups("Effect")[0];
            audioSource3D.audioSource.volume = 1.0f;
            audioSource3D.audioSource.spatialBlend = 1.0f;
            audioSource3D.audioSource.rolloffMode = AudioRolloffMode.Linear;
            _audioSources3DQueue.Enqueue(audioSource3D);
        }
    }

    public void Clear()
    {
#if !UNITY_SERVER
        foreach (AudioSource audioSource in _audioSources2D)
        {
            audioSource.clip = null;
            audioSource.Stop();
        }

        foreach (AudioSource3D audioSource3D in _usingAudioSourcesQueue)
        {
            audioSource3D.audioSource.clip = null;
            audioSource3D.audioSource.Stop();
            audioSource3D.ReturnQueue();
        }

        _audioClips.Clear();
#endif
    }

    public void Play2D(string path, Define.Sound2D type = Define.Sound2D.Effect2D, float pitch = 1.0f)
    {
#if !UNITY_SERVER
        AudioClip audioClip = GetOrAddAudioClip2D(path, type);
        Play2D(audioClip, type, pitch);
#endif
    }

    public void Play2D(AudioClip audioClip, Define.Sound2D type = Define.Sound2D.Effect2D, float pitch = 1.0f)
    {
#if !UNITY_SERVER
        if (audioClip == null)
            return;

        if (type == Define.Sound2D.Bgm)
        {
            AudioSource audioSource = _audioSources2D[(int)Define.Sound2D.Bgm];
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.pitch = pitch;
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else
        {
            AudioSource audioSource = _audioSources2D[(int)Define.Sound2D.Effect2D];
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
        }
#endif
    }

    AudioClip GetOrAddAudioClip2D(string path, Define.Sound2D type = Define.Sound2D.Effect2D)
    {
        AudioClip audioClip = null;

        if (type == Define.Sound2D.Bgm)
        {
            audioClip = Managers.Resource.Load<AudioClip>(path);
        }
        else
        {
            if (_audioClips.TryGetValue(path, out audioClip) == false)
            {
                audioClip = Managers.Resource.Load<AudioClip>(path);
                _audioClips.Add(path, audioClip);
            }
        }

        if (audioClip == null)
            Debug.Log($"AudioClip Missing! {path}");

        return audioClip;
    }

    public void Play3D(string path, Transform targetPos, bool isAttach = false, Define.Sound3D type = Define.Sound3D.Effect3D, float volume = 1.0f, float pitch = 1.0f, float minDistance = 7.5f, float maxDistance = 25.0f)
    {
#if !UNITY_SERVER
        AudioClip audioClip = GetOrAddAudioClip3D(path, type);
        Play3D(audioClip, targetPos, isAttach, type, volume, pitch, minDistance, maxDistance);
#endif
    }

    public void Play3D(AudioClip audioClip, Transform targetPos, bool isAttach = false, Define.Sound3D type = Define.Sound3D.Effect3D, float volume = 1.0f, float pitch = 1.0f, float minDistance = 7.5f, float maxDistance = 25.0f)
    {
#if !UNITY_SERVER
        if (audioClip == null)
            return;

        if (_audioSources3DQueue.Count == 0)
            return;

        AudioSource3D audioSource3D = _audioSources3DQueue.Dequeue();
        audioSource3D.audioSource.clip = audioClip;
        audioSource3D.transform.position = targetPos.position;
        if (isAttach)
            audioSource3D.transform.parent = targetPos;
        audioSource3D.audioSource.pitch = pitch;
        audioSource3D.audioSource.volume = volume;
        audioSource3D.audioSource.minDistance = minDistance;
        audioSource3D.audioSource.maxDistance = maxDistance;

        audioSource3D.Play();
#endif
    }

    public void Play3D(string path, Vector3 pos, Define.Sound3D type = Define.Sound3D.Effect3D, float volume = 1.0f, float pitch = 1.0f, float minDistance = 7.5f, float maxDistance = 25.0f)
    {
#if !UNITY_SERVER
        AudioClip audioClip = GetOrAddAudioClip3D(path, type);
        Play3D(audioClip, pos, type, volume, pitch, minDistance, maxDistance);
#endif
    }

    public void Play3D(AudioClip audioClip, Vector3 targetPos, Define.Sound3D type = Define.Sound3D.Effect3D, float volume = 1.0f, float pitch = 1.0f, float minDistance = 7.5f, float maxDistance = 25.0f)
    {
#if !UNITY_SERVER
        if (audioClip == null)
            return;

        if (_audioSources3DQueue.Count == 0)
            return;

        AudioSource3D audioSource3D = _audioSources3DQueue.Dequeue();
        audioSource3D.audioSource.clip = audioClip;
        audioSource3D.transform.position = targetPos;
        audioSource3D.audioSource.pitch = pitch;
        audioSource3D.audioSource.volume = volume;
        audioSource3D.audioSource.minDistance = minDistance;
        audioSource3D.audioSource.maxDistance = maxDistance;

        audioSource3D.Play();
#endif
    }

    AudioClip GetOrAddAudioClip3D(string path, Define.Sound3D type = Define.Sound3D.Effect3D)
    {
        if (path.Contains("FX/") == false)
            path = $"FX/{path}";

        AudioClip audioClip = null;

        if (_audioClips.TryGetValue(path, out audioClip) == false)
        {
            audioClip = Managers.Resource.Load<AudioClip>(path);
            _audioClips.Add(path, audioClip);
        }

        if (audioClip == null)
            Debug.Log($"AudioClip Missing! {path}");

        return audioClip;
    }
}
