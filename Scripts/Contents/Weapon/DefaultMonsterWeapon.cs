using UnityEngine;

public class DefaultMonsterWeapon : MeleeWeapon, IWeaponEffect, IWeaponSound
{
    [SerializeField]
    string[] normalAtkEffects;

    [SerializeField]
    string[] normalAtkSounds;

    public override void Init()
    {
        base.Init();
    }

    public void PlayComboAttackEffects()
    {
       
    }

    public void PlayComboAttackSound()
    {
        int soundCount = comboCount - 1;
        if (soundCount < 0 || soundCount >= normalAtkSounds.Length)
            return;

        Managers.Sound.Play3D(normalAtkSounds[soundCount], transform, false, Define.Sound3D.Effect3D, 1, 1, 0, 20);
    }

    public void PlaySkillEffect()
    {
        
    }

    public void PlaySkillSound()
    {
       
    }
}
