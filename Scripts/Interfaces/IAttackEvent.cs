using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackEvent
{
    [ServerCallback]
    public abstract void OnHitJudgmentStart();

    [ServerCallback]
    public abstract void OnHitJudgmentEnd();
}
