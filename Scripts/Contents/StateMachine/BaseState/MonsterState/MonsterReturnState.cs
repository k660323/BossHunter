using Mirror.Examples.CCU;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterReturnState : BaseStateMonster
{
    public MonsterReturnState(Define.State _state, Monster _monster, MonsterController _monsterController) : base(_state, _monster, _monsterController)
    {
    }

    public override void EnterState()
    {
        // �̵� �ִϸ��̼� ����
        MonsterGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 1.0f);

        // Nav �ӵ� ����
        MonsterGS.GetNav.speed = MonsterGS.GetMonsterStat.MoveSpeed;

        // ������ ����
        MonsterGS.GetNav.SetDestination(MonsterControllerGS.SpawnPos);

        // Nav ���� Ȱ��ȭ
        MonsterGS.GetNav.isStopped = false;

        // ���� ��� �ʱ�ȭ
        MonsterControllerGS.Target = null;
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {
        // �̵� �ִϸ��̼� ����
        MonsterGS.GetNetAnim.animator.SetFloat(Managers.AnimHash.FmoveSpeed, 0.0f);

        // Nav ���� ��Ȱ��ȭ
        MonsterGS.GetNav.isStopped = true;

        // ���� ������ Ǯ�Ƿ� ����
        MonsterGS.GetMonsterStat.Hp = MonsterGS.GetMonsterStat.MaxHp;
    }

    public override void FixedUpdateState()
    {
        // Nav �ӵ� ����
        MonsterGS.GetNav.speed = MonsterGS.GetMonsterStat.MoveSpeed;

        // ���� ���� ��ġ�� ���� ��ġ�� ���̰� 0.1f ���ϸ� ���̵��
        if (MonsterGS.GetNav.remainingDistance < 0.1f)
        {
            // �������� �����ϱ����� ���� ��ġ ����
            MonsterGS.transform.position = MonsterControllerGS.SpawnPos;
            MonsterGS.StateMachine.ChangeState(Define.State.Idle);
        }
    }

    public override void UpdateState()
    {
       
    }
}
