using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherWeapon : RangedWeapon, IWeaponEffect, IWeaponSound
{
    [SerializeField]
    string[] normalAtkEffects;

    [SerializeField]
    string[] normalAtkSounds;

    public override void Init()
    {
        base.Init();
    }

    [ClientCallback]
    public void PlayComboAttackEffects()
    {
        
    }

    public void PlaySkillEffect()
    {
       
    }


    [ClientCallback]
    public void PlayComboAttackSound()
    {
        int soundCount = comboCount - 1;
        if (soundCount < 0 || soundCount >= normalAtkSounds.Length)
            return;

        Managers.Sound.Play3D(normalAtkSounds[soundCount], transform, false, Define.Sound3D.Effect3D, 0.7f, 1, 0, 20);
    }

    public void PlaySkillSound()
    {

    }
}
