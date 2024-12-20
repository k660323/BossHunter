using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnWallState : BaseStatePlayer
{
    // 점프 애니메이션을 어느정도 실행한 후에 벽을 탈 수 있게 한다.
    public bool isJumpable;

    RaycastHit hit;

    public OnWallState(Define.State _state, Player _player, PlayerController _playerController) : base(_state, _player, _playerController)
    {
    }

    public override bool CheckCondition()
    {
        // 점프 상태가 아니면 스킵
        if (creatureGS.StateMachine.State != Define.State.Jumping)
            return false;

        // 추가적인 점프키 누르지 않았으면 스킵
        if (PlayerControllerGS.IsJump == false)
            return false;

        Vector3 rayPos = creatureGS.transform.position;
        float maxDistance = 1.5f;
        // 콜라이더 높이 인터페이스가 있으면 대충 손 높이 쯤 y축 배치
        if (creatureGS is IColliderInfo)
        {
            IColliderInfo colliderHeight = creatureGS as IColliderInfo;
            rayPos.Set(rayPos.x, rayPos.y + colliderHeight.GetHeight() * 0.75f, rayPos.z);
            maxDistance = colliderHeight.GetRadius() + 0.5f;

        }

        // 점프중이고 점프키를 눌렀다면 Ray를 쏴서 벽이 있는지 확인한다.
        return creatureGS.GetSetPhysics.Raycast(rayPos, creatureGS.transform.forward, out hit, maxDistance, Managers.LayerManager.Ground | Managers.LayerManager.Wall, QueryTriggerInteraction.Ignore);
    }

    public override void EnterState()
    {
        PlayerGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BJumpIn, false);
        PlayerGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BJumpOut, false);

        // 콜라이더 높이 인터페이스가 있으면 대충 손 높이 쯤 y축 배치 콜라이더 반지름 만큼 떨어트림
        Vector3 destPos = hit.point;
        if (creatureGS is IColliderInfo)
        {
            IColliderInfo colliderInfo = creatureGS as IColliderInfo;
            // 만약 Ray캐스트가 성공적으로 되면 Ray위치로 플레이어 이동
            destPos.Set(hit.point.x - colliderInfo.GetRadius(), hit.point.y - colliderInfo.GetHeight() * 0.75f, hit.point.z - colliderInfo.GetRadius());
        }

        // 만약 Ray캐스트가 성공적으로 되면 Ray위치로 플레이어 이동
        creatureGS.GetRigidBody.MovePosition(destPos);
        // 벽쪽으로 보도록 이동시킨다.
        creatureGS.GetRigidBody.MoveRotation(Quaternion.LookRotation(-hit.normal));

        // 물리 영향 x
        creatureGS.GetRigidBody.isKinematic = true;
        // 중력 끈다.
        creatureGS.GetRigidBody.useGravity = false;
        // 가속 초기화
        creatureGS.GetRigidBody.velocity = Vector3.zero;
        // 벽타기 애니메이션 재생
        creatureGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BOnWall, true);
        // 컨트롤러에서 중력 제어 X
        PlayerControllerGS.IsOnWall = true;
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {
        // 물리 영향 o
        creatureGS.GetRigidBody.isKinematic = false;
        // 중력 킨다.
        creatureGS.GetRigidBody.useGravity = true;
        // 벽타기 애니메이션 끄기
        creatureGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BOnWall, false);
        // 컨트롤러에서 중력 제어 O
        PlayerControllerGS.IsOnWall = false;
        // 점프 가능 타이밍
        isJumpable = false;
    }

    public override void FixedUpdateState()
    {
        if (isJumpable)
            creatureGS.StateMachine.ChangeState(Define.State.Jumping);
    }

    public override void UpdateState()
    {
      
    }
}
