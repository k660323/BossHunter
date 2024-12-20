using Mirror;

public abstract class AnimationEvent : NetworkBehaviour
{
    protected Creature creature;
    private void Awake()
    {
        Init();
    }

    public abstract void Init();

    public abstract void OnCanDashAttack();

    public abstract void OnDashEnd();

    public abstract void OnAttackEnd();

    public abstract void OnDeadEnd();

    public abstract void OnHitEnd();
}
