using Mirror;
using UnityEngine;

public class WarriorWeapon : MeleeWeapon, IWeaponEffect, IWeaponSound
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
        if (Util.IsSameScene(gameObject) == false)
            return;

        int effectCount = comboCount - 1;
        if (effectCount < 0)
            return;

        GameObject effectObj = Managers.Resource.Instantiate(normalAtkEffects[effectCount], null, true);
        effectObj.transform.parent = null;
        effectObj.transform.position = transform.root.position;
        effectObj.transform.rotation = transform.root.rotation;

        if (effectObj.TryGetComponent(out IEffectPlay effectPlay))
        {
            effectPlay.EffectPlay();
        }
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

        Managers.Sound.Play3D(normalAtkSounds[soundCount], transform, false, Define.Sound3D.Effect3D, 1, 1, 0, 20);
    }

    public void PlaySkillSound()
    {

    }
}
