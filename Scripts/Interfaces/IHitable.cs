using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitable
{
    public bool IsAlive { get; }

    public void OnAttacked(Stat attacker, int _hitpriority = 0);

    public void OnAttacked(int _damage, int _hitpriority = 0);

    public void OnAttacked(Stat attackerOwner, int _damage, int _hitpriority = 0);

}
