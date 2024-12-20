using Data;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerDryadAnimEvt : MonsterAnimEvt
{
    [ServerCallback]
    public void DisseminationEnd()
    {
        creature.StateMachine.ChangeState(Define.State.Idle);
    }
}
