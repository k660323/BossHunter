using Mirror.Examples.CCU;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : BaseStatePlayer, IPreInput
{
    // �� �Է� ���� �� �ִ� �÷���
    private bool canAddInputBuffer;
    public bool CanAddInputBuffer { get { return canAddInputBuffer; } set { canAddInputBuffer = value; } }

    // �� �Է� ���� ���� �÷��� ��
    public bool preInput;
    public bool PreInput { get { return preInput; } set { preInput = value; } }
    
    public AttackState(Define.State _state, Player _player, PlayerController _playerController) : base(_state, _player, _playerController)
    {

    }

    public override bool CheckCondition()
    {
        Weapon weapon = creatureGS.GetEquipment.EquipWeapon;
        if (PreInput == false)
        {
            // ����� �Է� ������ false
            if (PlayerControllerGS.IsAttack == false)
                return false;

            // ���Ⱑ ������ false
            if (weapon == null)
                return false;
        }

        if (weapon.IsLastComboToFirstCombo())
            return true;

        return weapon.IsAttackable();
    }

    public override void EnterState()
    {
        PlayerGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FAttackSpeed, PlayerGS.GetStat.TotalAtkSpeed);
        PlayerGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BAttack, true);
        PlayerGS.GetEquipment.EquipWeapon?.CTS_StartAttack(state);
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {
        PlayerGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BAttack, false);
        CanAddInputBuffer = false;
        PreInput = false;
        PlayerGS.GetEquipment.EquipWeapon?.CTS_EndAttack(state);
       
    }

    public override void FixedUpdateState()
    {
     
    }

    public override void UpdateState()
    {
        // �����Է� ���� �����϶� ����Ű�� �������� �� �Է� true
        if (CanAddInputBuffer && PlayerControllerGS.IsAttack)
        {
            PreInput = true;
        }

        if (PlayerGS.StateMachine.ChangeState(Define.State.Dash))
            return;
    }
}
