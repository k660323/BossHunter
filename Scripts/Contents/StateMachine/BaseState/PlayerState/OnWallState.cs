using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnWallState : BaseStatePlayer
{
    // ���� �ִϸ��̼��� ������� ������ �Ŀ� ���� Ż �� �ְ� �Ѵ�.
    public bool isJumpable;

    RaycastHit hit;

    public OnWallState(Define.State _state, Player _player, PlayerController _playerController) : base(_state, _player, _playerController)
    {
    }

    public override bool CheckCondition()
    {
        // ���� ���°� �ƴϸ� ��ŵ
        if (creatureGS.StateMachine.State != Define.State.Jumping)
            return false;

        // �߰����� ����Ű ������ �ʾ����� ��ŵ
        if (PlayerControllerGS.IsJump == false)
            return false;

        Vector3 rayPos = creatureGS.transform.position;
        float maxDistance = 1.5f;
        // �ݶ��̴� ���� �������̽��� ������ ���� �� ���� �� y�� ��ġ
        if (creatureGS is IColliderInfo)
        {
            IColliderInfo colliderHeight = creatureGS as IColliderInfo;
            rayPos.Set(rayPos.x, rayPos.y + colliderHeight.GetHeight() * 0.75f, rayPos.z);
            maxDistance = colliderHeight.GetRadius() + 0.5f;

        }

        // �������̰� ����Ű�� �����ٸ� Ray�� ���� ���� �ִ��� Ȯ���Ѵ�.
        return creatureGS.GetSetPhysics.Raycast(rayPos, creatureGS.transform.forward, out hit, maxDistance, Managers.LayerManager.Ground | Managers.LayerManager.Wall, QueryTriggerInteraction.Ignore);
    }

    public override void EnterState()
    {
        PlayerGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BJumpIn, false);
        PlayerGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BJumpOut, false);

        // �ݶ��̴� ���� �������̽��� ������ ���� �� ���� �� y�� ��ġ �ݶ��̴� ������ ��ŭ ����Ʈ��
        Vector3 destPos = hit.point;
        if (creatureGS is IColliderInfo)
        {
            IColliderInfo colliderInfo = creatureGS as IColliderInfo;
            // ���� Rayĳ��Ʈ�� ���������� �Ǹ� Ray��ġ�� �÷��̾� �̵�
            destPos.Set(hit.point.x - colliderInfo.GetRadius(), hit.point.y - colliderInfo.GetHeight() * 0.75f, hit.point.z - colliderInfo.GetRadius());
        }

        // ���� Rayĳ��Ʈ�� ���������� �Ǹ� Ray��ġ�� �÷��̾� �̵�
        creatureGS.GetRigidBody.MovePosition(destPos);
        // �������� ������ �̵���Ų��.
        creatureGS.GetRigidBody.MoveRotation(Quaternion.LookRotation(-hit.normal));

        // ���� ���� x
        creatureGS.GetRigidBody.isKinematic = true;
        // �߷� ����.
        creatureGS.GetRigidBody.useGravity = false;
        // ���� �ʱ�ȭ
        creatureGS.GetRigidBody.velocity = Vector3.zero;
        // ��Ÿ�� �ִϸ��̼� ���
        creatureGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BOnWall, true);
        // ��Ʈ�ѷ����� �߷� ���� X
        PlayerControllerGS.IsOnWall = true;
    }

    public override void ExitState(Define.State _state, BaseState baseState)
    {
        // ���� ���� o
        creatureGS.GetRigidBody.isKinematic = false;
        // �߷� Ų��.
        creatureGS.GetRigidBody.useGravity = true;
        // ��Ÿ�� �ִϸ��̼� ����
        creatureGS.GetNetAnim.animator.SetBool(Managers.AnimHash.BOnWall, false);
        // ��Ʈ�ѷ����� �߷� ���� O
        PlayerControllerGS.IsOnWall = false;
        // ���� ���� Ÿ�̹�
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
