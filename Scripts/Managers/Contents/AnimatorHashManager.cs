using System.Collections.Generic;
using UnityEngine;

public class AnimatorHashManager
{
    public readonly int FmoveSpeed = Animator.StringToHash("MoveSpeed");
    public readonly int BJumpIn = Animator.StringToHash("IsJumpIn");
    public readonly int BJumpOut = Animator.StringToHash("IsJumpOut");
    public readonly int BDash = Animator.StringToHash("IsDash");
    public readonly int BAddInputDash = Animator.StringToHash("IsAddInputDash");
    public readonly int BAttack = Animator.StringToHash("IsAttack");
    public readonly int IAttackCombo = Animator.StringToHash("AttackCombo");
    public readonly int FAttackSpeed = Animator.StringToHash("AttackSpeed");
    public readonly int BDead = Animator.StringToHash("IsDead");
    public readonly int BHit = Animator.StringToHash("IsHit");
    public readonly int BStun = Animator.StringToHash("IsStun");
    public readonly int BOnWall = Animator.StringToHash("IsOnWall");
    public readonly int BDissemination = Animator.StringToHash("IsDissemination");

    private Dictionary<string, int> animHashDic = new Dictionary<string, int>();

    public AnimatorHashManager()
    {
        animHashDic.Add("MoveSpeed", Animator.StringToHash("MoveSpeed"));
        animHashDic.Add("IsJumpIn", Animator.StringToHash("IsJumpIn"));
        animHashDic.Add("BJumpOut", Animator.StringToHash("IsJumpOut"));
        animHashDic.Add("IsDash", Animator.StringToHash("IsDash"));
        animHashDic.Add("IsAddInputDash", Animator.StringToHash("IsAddInputDash"));
        animHashDic.Add("IsAttack", Animator.StringToHash("IsAttack"));
        animHashDic.Add("AttackCombo", Animator.StringToHash("AttackCombo"));
        animHashDic.Add("AttackSpeed", Animator.StringToHash("AttackSpeed"));
        animHashDic.Add("IsDead", Animator.StringToHash("IsDead"));
        animHashDic.Add("IsHit", Animator.StringToHash("IsHit"));
        animHashDic.Add("IsStun", Animator.StringToHash("IsStun"));
        animHashDic.Add("IsOnWall", Animator.StringToHash("IsOnWall"));
        animHashDic.Add("IsDissemination", Animator.StringToHash("IsDissemination"));
    }

    public int GetAnimHash(string name)
    {
        return animHashDic[name];
    }
}
