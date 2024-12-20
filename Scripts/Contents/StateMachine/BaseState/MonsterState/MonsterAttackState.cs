using UnityEngine;

public class MonsterAttackState : BaseStateMonster
{

    public MonsterAttackState(Define.State _state, Monster _monster, MonsterController _monsterController) : base(_state, _monster, _monsterController)
    {
    }

    // 공격 가능한지 && 공격 범위 체크
    public override bool CheckCondition()
    {
        Weapon weapon = creatureGS.GetEquipment.EquipWeapon;

        // 무기가 없거나 공격중 또는 쿨이거나, 타겟이 없을경우 상태 변환 취소
        if (weapon == null || MonsterControllerGS.Target == null)
            return false;

        // 대상이 살아있지 않으면 대상을 지우고 false를 리턴한다.
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
        
        // 공격 범위면 공격
        float distance = Vector3.Distance(curPos, targetPos);
        if (distance <= creatureGS.GetStat.AtkDistance)
        {
            if (weapon.IsLastComboToFirstCombo())
            {
                return true;
            }

            // 만약 콤보 어택 인터페이스가 있으면 콤보 어택 가능 함수 실행
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
