using Mirror;

public class PlayerStateMachine : StateMachine
{
    [ClientCallback]
    private void FixedUpdate()
    {
        if (isOwned)
            BaseState?.FixedUpdateState();
    }

    [ClientCallback]
    private void Update()
    {
        if (isOwned)
            BaseState?.UpdateState();
    }
}
