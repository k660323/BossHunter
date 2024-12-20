using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISound
{
    public void OnMoveSound();

    public void OnJumpSound();

    public void OnDashSound();

    public void OnHitSound();

    public void OnDeadSound();

}
