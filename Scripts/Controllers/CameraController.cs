using Cinemachine;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    // ������ ���
    [SerializeField]
    private GameObject target;
    // �ó׸ӽ�
    [SerializeField]
    private CinemachineVirtualCameraBase freeLook;
    // ������ ��ġ
    [SerializeField]
    private Transform followPosition;

    float moveAngleX;
    float moveAnlgeY;

    // ��Ʈ�ѷ� ������ ���
    [SerializeField]
    Define.CameraTargetOffset targetOffsetMode; 
    // ������ ��
    Vector3 targetOffset;

    // ������ ��ġ�� (ī�޶� ��ġ)
    Vector3 deltaFollowPos;

    // �⺻ ī�޶� �Ÿ�
    [SerializeField, Header("�ּ� ��Ʈ�ѷ��� ī�޶��� �Ÿ�")]
    float minDetectRange = 2.0f;
    [SerializeField, Header("�ִ� ��Ʈ�ѷ��� ī�޶��� �Ÿ�")]
    float maxDetectRange = 15.0f;

    // ���� ī�޶� �Ÿ�
    [SerializeField, Range(2.0f, 15.0f), Header("���� ��Ʈ�ѷ��� ī�޶��� �Ÿ�")]
    float detectDistance = 10.0f;

    // currentDistance�� get,set�Լ� ���� ������ ���� �ȿ� ���� ������ �� �ְ� �Ѵ�.
    float DetectRange { get { return detectDistance; } set { detectDistance = Mathf.Clamp(value, minDetectRange, maxDetectRange); } }
    
    float inputDistance;
    float InputMouseWheel {  get { return inputDistance; } set { inputDistance = Mathf.Clamp(value, minDetectRange, maxDetectRange); } }

    private InputAction mouseWheelAction;

    // ���� Ÿ���� ������ ��������
    public PhysicsScene physics;

#if !UNITY_SERVER
    void Awake()
    {
        // ���콺 �Է� ���ε�
        Managers.Input._playerInput.actionEvents[1].RemoveAllListeners();
        Managers.Input._playerInput.actionEvents[1].AddListener(OnLook);

        // ���콺 ��ũ�� ���ε�
        mouseWheelAction = new InputAction(binding: "<Mouse>/scroll/y");
        mouseWheelAction.performed += ctx => OnMouseWheel(ctx.ReadValue<float>());
        mouseWheelAction.Enable();

        // ����ȭ
        InputMouseWheel = DetectRange;
       
    }
#endif

    #region ���� Input �ݹ�
    void OnLook(InputAction.CallbackContext context)
    {
        if (Managers.Input.IsCursor)
            return;

        Vector2 mouseInput = context.ReadValue<Vector2>();
        Vector3 camAngle = transform.rotation.eulerAngles;

        // �� �Ʒ�
        moveAngleX = camAngle.x - (mouseInput.y * Managers.Option.gamePlayOption.mouseVirtical * 0.1f);

        // �¿�
        moveAnlgeY = camAngle.y + (mouseInput.x * Managers.Option.gamePlayOption.mouseHorizontal * 0.1f);

        moveAngleX = CheckYaw(moveAngleX);
    }

    float CheckYaw(float x)
    {
        if (x < 180)
            return Mathf.Clamp(x, -1.0f, 80.0f);
        else
            return Mathf.Clamp(x, 300.0f, 360.0f);
    }

    // ���콺 �� �Է°�
    void OnMouseWheel(float y)
    {
        if (Managers.Input.IsCursor)
            return;

        InputMouseWheel += y == 120 ? -0.5f : 0.5f;
    }

    #endregion

    [ClientCallback]
    void FixedUpdate()
    {
        if (target == null)
            return;

        // ��Ʈ�ѷ� ��ġ ����
        transform.position = target.transform.position + targetOffset;
        transform.rotation = Quaternion.Euler(new Vector3(moveAngleX, moveAnlgeY, 0.0f));

        // �� ����
        DetectRange = Mathf.Lerp(DetectRange, InputMouseWheel, 0.5f);

        // ī�޶� ��ġ ����
        // �浹�ϴ� ��ü üũ
        CameraBlockCheck();

        followPosition.transform.position = deltaFollowPos;
    }

    #region Ÿ�� ���� �ĺ���

    public void SetTarget(GameObject target)
    {
        if (target != null)
        {
            this.target = target;
            physics = target.scene.GetPhysicsScene();
            SetTargetOffset(targetOffsetMode);
        }
        else
        {
            this.target = null;
        }
    }

    public void SetTargetOffset(Define.CameraTargetOffset offset)
    {
        targetOffsetMode = offset;

        IColliderInfo height;
        switch (offset) 
        {
            case Define.CameraTargetOffset.Bottom:
                targetOffset = Vector3.zero;
                break;
            case Define.CameraTargetOffset.HalfBottom:
                if(target.TryGetComponent(out height))
                    targetOffset = new Vector3(0, height.GetHeight() * 0.25f, 0);
                break;
            case Define.CameraTargetOffset.Middle:
                if (target.TryGetComponent(out height))
                    targetOffset = new Vector3(0, height.GetHeight() * 0.5f, 0);
                break;
            case Define.CameraTargetOffset.HalfTop:
                if (target.TryGetComponent(out height))
                    targetOffset = new Vector3(0, height.GetHeight() * 0.75f, 0);
                break;
            case Define.CameraTargetOffset.Top:
                if (target.TryGetComponent(out height))
                    targetOffset = new Vector3(0, height.GetHeight(), 0);
                break;
        }
    }

    void CameraBlockCheck()
    {
        RaycastHit hit;
        // ��Ʈ�ѷ� ��ġ
        Vector3 controllerPos = transform.position;
        // ī�޶� ���� ��ġ
        Vector3 cameraPos = followPosition.position;

        // �Ÿ� ���ؼ� ����ȭ
        float ctcDistance = Vector3.Distance(cameraPos, controllerPos);
        Vector3 dir = (cameraPos - controllerPos).normalized;

        // ��Ʈ�ѷ� -> ī�޶� ���̿� layer ������Ʈ ����
        if (physics.Raycast(controllerPos, dir, out hit, ctcDistance + 0.1f, Managers.LayerManager.Ground | Managers.LayerManager.Wall))
        {
            // ī�޶� ��ġ�� hit ��ġ�� �̵�
            deltaFollowPos = hit.point;
        }
        // ���н� ī�޶� -> �⺻ ī�޶� ��ġ ������ layer ������Ʈ ����
        else
        {
            // ī�޶� ���� ���� ����
            Vector3 backDir = -followPosition.forward;
            // ��ü �浹 ������ �⺻ ������Ʈ ��ġ
            Vector3 defFollowPos = controllerPos + dir * DetectRange;
            // ���� ī�޶� ��ġ�� �⺻ ī�޶� ��ġ�� �Ÿ���
            float restRange = Vector3.Distance(cameraPos, defFollowPos) + 0.1f;

            // ���� �ڿ� �ش� layer �浹�� �����Ǹ� õõ�� �̵�
            if (physics.Raycast(cameraPos, backDir, out hit, restRange, Managers.LayerManager.Ground))
            {
                // ī�޶� ��ġ �����ϰ� �⺻ �Ÿ� ����
                deltaFollowPos = Vector3.Lerp(deltaFollowPos, hit.point, 0.5f);
            }
            else
            {
                // ������ �⺻ �Ÿ� ����
                deltaFollowPos = defFollowPos;
            }
        }
    }

    #endregion

    [ClientCallback]
    private void OnDestroy()
    {
        // Ű �Է� ����
        if (Managers.Input._playerInput == null)
            return;
        Managers.Input._playerInput.actionEvents[1].RemoveListener(OnLook);
        mouseWheelAction.Disable();
    }
}
