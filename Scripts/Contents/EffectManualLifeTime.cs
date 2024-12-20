using UnityEngine;

public class EffectManualLifeTime : EffectLifeTime
{
    [SerializeField]
    protected float effectDuration;

    public override void EffectPlay()
    {
        Invoke(nameof(PushPool), effectDuration + 0.1f);
    }
}
