using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitState : BaseState
{
    public HitState(Define.State _state, Creature _creature, BaseController _controller) : base(_state, _creature, _controller)
    {
    }

    public override void EnterState()
    {
        creatureGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BHit, true);
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {
        creatureGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BHit, false);
    }

    public override void FixedUpdateState()
    {
      
    }

    public override void UpdateState()
    {
       
    }
}
