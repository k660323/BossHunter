using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectLifeTime : MonoBehaviour, IEffectPlay
{
    protected ParticleSystem _particleSystem;

    protected void Awake()
    {
        TryGetComponent(out _particleSystem);
    }

    public virtual void EffectPlay()
    {
        Invoke(nameof(PushPool), _particleSystem.main.duration + 0.1f);
    }

    protected void PushPool()
    {
        Managers.Resource.Destroy(gameObject);
    }
}
