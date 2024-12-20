using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashState : BaseStatePlayer
{
    public Queue<Vector3> inputDirectionBuffer { get; private set; }
    public int CurrentDashCount { get; set; } = 0;
    public int MaxDashCount { get; set; }
    public bool CanAddInputBuffer { get; set; } // 버퍼 입력이 가능한가?
    public bool CanDashAttack { get; set; }
    public bool isFinishedDash { get; set; } // 애니메이션의해 호출되는 OnFinishedDash 함수 호출 여부 확인하는 변수

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
        // 대시는 땅에 있어야하고 (1번 경우) 예약 키가 있으면 true 반환, (2번 경우) 대시 키입력과 대시가 가능할 경우 ture 반환
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
        // 다른 애니메이션이 들어올겨우
        if (state != _state)
        {
            // 애니메이션이 함수 캔슬
            if(isFinishedDash == false)
            {
                // 선입력받은 버퍼 비워준다.
                inputDirectionBuffer.Clear();
                // 대쉬 어택 불가능
                CanDashAttack = false;
            }

            PlayerGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BDash, false);

            // 대시 쿨타임 시작
            PlayerGS.StartCoroutine(CheckDashReInputLimitTime());
        }
        // 같은 애니메이션 호출
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
        // 대시 중에 버퍼에 입력 가능한 프레임
        if (PlayerControllerGS.IsDash && PlayerControllerGS.IsOnGround && CanAddInputBuffer && CurrentDashCount < MaxDashCount)
        {
            CurrentDashCount++;
            Vector3 dir = PlayerControllerGS.calculatedDirection;
            inputDirectionBuffer.Enqueue(dir == Vector3.zero ? PlayerGS.transform.forward : dir);
        }
    }
}
