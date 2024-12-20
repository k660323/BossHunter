
public abstract class BaseState
{
    public Define.State state { get; private set; }

    public Creature creatureGS { get; private set; }

    public BaseController controller { get; private set; }

    public BaseState(Define.State _state, Creature _creature, BaseController _controller)
    {
        state = _state;
        creatureGS = _creature;
        controller = _controller;
    }

    public virtual bool CheckCondition()
    {
        return true;
    }

    public abstract void EnterState();

    public abstract void FixedUpdateState();

    public abstract void UpdateState();

    public abstract void ExitState(Define.State _state, BaseState baseState);
}
