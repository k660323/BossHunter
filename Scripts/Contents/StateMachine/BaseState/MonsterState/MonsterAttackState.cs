using UnityEngine;

public class MonsterAttackState : BaseStateMonster
{

    public MonsterAttackState(Define.State _state, Monster _monster, MonsterController _monsterController) : base(_state, _monster, _monsterController)
    {
    }

    // ���� �������� && ���� ���� üũ
    public override bool CheckCondition()
    {
        Weapon weapon = creatureGS.GetEquipment.EquipWeapon;

        // ���Ⱑ ���ų� ������ �Ǵ� ���̰ų�, Ÿ���� ������� ���� ��ȯ ���
        if (weapon == null || MonsterControllerGS.Target == null)
            return false;

        // ����� ������� ������ ����� ����� false�� �����Ѵ�.
        if(MonsterControllerGS.TargetCreature is IHitable hitable)
        {
            if(hitable.IsAlive == false)
            {
                MonsterControllerGS.Target = null;
                return false;
            }
        }

        Vector3 curPos = creatureGS.transform.position;
        Vector3 targetPos = MonsterControllerGS.Target.transform.position;
        
        // ���� ������ ����
        float distance = Vector3.Distance(curPos, targetPos);
        if (distance <= creatureGS.GetStat.AtkDistance)
        {
            if (weapon.IsLastComboToFirstCombo())
            {
                return true;
            }

            // ���� �޺� ���� �������̽��� ������ �޺� ���� ���� �Լ� ����
            return weapon.IsAttackable();
        }
        else
        {
            return false;
        }
    }

    public override void EnterState()
    {
        MonsterGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FAttackSpeed, MonsterGS.GetStat.TotalAtkSpeed);
        MonsterGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BAttack, true);
        MonsterControllerGS.LookAt(MonsterControllerGS.Target.transform);
        MonsterGS.GetEquipment.EquipWeapon?.CTS_StartAttack(state);
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {
        MonsterGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BAttack, false);
        MonsterGS.GetEquipment.EquipWeapon?.CTS_EndAttack(state);
    }

    public override void FixedUpdateState()
    {
     
    }

    public override void UpdateState()
    {
        
    }
}
