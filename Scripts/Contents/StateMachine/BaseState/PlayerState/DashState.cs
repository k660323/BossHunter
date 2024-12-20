using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashState : BaseStatePlayer
{
    public Queue<Vector3> inputDirectionBuffer { get; private set; }
    public int CurrentDashCount { get; set; } = 0;
    public int MaxDashCount { get; set; }
    public bool CanAddInputBuffer { get; set; } // ���� �Է��� �����Ѱ�?
    public bool CanDashAttack { get; set; }
    public bool isFinishedDash { get; set; } // �ִϸ��̼����� ȣ��Ǵ� OnFinishedDash �Լ� ȣ�� ���� Ȯ���ϴ� ����

    bool isDashable;
    public readonly float dashPower;
    public readonly float dashTetanyTime;
    public readonly float dashCooltime;

    public DashState(Define.State _state, Player _player, PlayerController _playerController, float _dashPower = 3.0f, float _dashTetanyTime = 3.0f, float _dashCoolTime = 1.0f, int _maxDashCount = 0) : base(_state, _player, _playerController)
    {
        inputDirectionBuffer = new Queue<Vector3>();
        isDashable = true;
        dashPower = _dashPower;
        dashTetanyTime = _dashTetanyTime;
        dashCooltime = _dashCoolTime;
        MaxDashCount = _maxDashCount;
    }

    public override bool CheckCondition()
    {
        // ��ô� ���� �־���ϰ� (1�� ���) ���� Ű�� ������ true ��ȯ, (2�� ���) ��� Ű�Է°� ��ð� ������ ��� ture ��ȯ
        return PlayerControllerGS.IsOnGround && 
            (inputDirectionBuffer.Count > 0 ?
            (MaxDashCount >= inputDirectionBuffer.Count ? true : false) : 
            PlayerControllerGS.IsDash && isDashable);
    }

    public override void EnterState()
    {
        Vector3 dashDirection = Vector3.zero;

        if (inputDirectionBuffer.Count > 0)
        {
            dashDirection = inputDirectionBuffer.Dequeue();
        }
        else
        {
            Vector3 dir = PlayerControllerGS.calculatedDirection;
            dashDirection = (dir == Vector3.zero) ? PlayerGS.transform.forward : dir;
        }


        dashDirection = (dashDirection == Vector3.zero) ? PlayerControllerGS.calculatedDirection : dashDirection;

        PlayerGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BDash, true);
        PlayerControllerGS.LookAt(new Vector3(dashDirection.x, 0f, dashDirection.z));

        PlayerGS.GetRigidBody.velocity = dashDirection * PlayerGS.GetStat.MoveSpeed * dashPower;
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {
        PlayerGS.GetRigidBody.velocity = Vector3.zero;
        // �ٸ� �ִϸ��̼��� ���ðܿ�
        if (state != _state)
        {
            // �ִϸ��̼��� �Լ� ĵ��
            if(isFinishedDash == false)
            {
                // ���Է¹��� ���� ����ش�.
                inputDirectionBuffer.Clear();
                // �뽬 ���� �Ұ���
                CanDashAttack = false;
            }

            PlayerGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BDash, false);

            // ��� ��Ÿ�� ����
            PlayerGS.StartCoroutine(CheckDashReInputLimitTime());
        }
        // ���� �ִϸ��̼� ȣ��
        else
        {
            //PlayerGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BAddInputDash, true);
        }
    }

    public IEnumerator CheckDashReInputLimitTime()
    {
        isDashable = false;
        isFinishedDash = false;
        yield return new WaitForSeconds(dashCooltime);
        isDashable = true;
        CanAddInputBuffer = false;
        CurrentDashCount = 0;
    }

    public override void FixedUpdateState()
    {
       
    }

    public override void UpdateState()
    {
        // ��� �߿� ���ۿ� �Է� ������ ������
        if (PlayerControllerGS.IsDash && PlayerControllerGS.IsOnGround && CanAddInputBuffer && CurrentDashCount < MaxDashCount)
        {
            CurrentDashCount++;
            Vector3 dir = PlayerControllerGS.calculatedDirection;
            inputDirectionBuffer.Enqueue(dir == Vector3.zero ? PlayerGS.transform.forward : dir);
        }
    }
}
