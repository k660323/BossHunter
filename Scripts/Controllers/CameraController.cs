using Cinemachine;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    // 추적할 대상
    [SerializeField]
    private GameObject target;
    // 시네머신
    [SerializeField]
    private CinemachineVirtualCameraBase freeLook;
    // 관찰할 위치
    [SerializeField]
    private Transform followPosition;

    float moveAngleX;
    float moveAnlgeY;

    // 컨트롤러 오프셋 모드
    [SerializeField]
    Define.CameraTargetOffset targetOffsetMode; 
    // 오프셋 값
    Vector3 targetOffset;

    // 보정된 위치값 (카메라 위치)
    Vector3 deltaFollowPos;

    // 기본 카메라 거리
    [SerializeField, Header("최소 컨트롤러와 카메라의 거리")]
    float minDetectRange = 2.0f;
    [SerializeField, Header("최대 컨트롤러와 카메라의 거리")]
    float maxDetectRange = 15.0f;

    // 현재 카메라 거리
    [SerializeField, Range(2.0f, 15.0f), Header("현재 컨트롤러와 카메라의 거리")]
    float detectDistance = 10.0f;

    // currentDistance의 get,set함수 정의 지정된 범위 안에 값을 설정할 수 있게 한다.
    float DetectRange { get { return detectDistance; } set { detectDistance = Mathf.Clamp(value, minDetectRange, maxDetectRange); } }
    
    float inputDistance;
    float InputMouseWheel {  get { return inputDistance; } set { inputDistance = Mathf.Clamp(value, minDetectRange, maxDetectRange); } }

    private InputAction mouseWheelAction;

    // 현재 타겟의 물리씬 가져오기
    public PhysicsScene physics;

#if !UNITY_SERVER
    void Awake()
    {
        // 마우스 입력 바인드
        Managers.Input._playerInput.actionEvents[1].RemoveAllListeners();
        Managers.Input._playerInput.actionEvents[1].AddListener(OnLook);

        // 마우스 스크롤 바인드
        mouseWheelAction = new InputAction(binding: "<Mouse>/scroll/y");
        mouseWheelAction.performed += ctx => OnMouseWheel(ctx.ReadValue<float>());
        mouseWheelAction.Enable();

        // 동기화
        InputMouseWheel = DetectRange;
       
    }
#endif

    #region 유저 Input 콜백
    void OnLook(InputAction.CallbackContext context)
    {
        if (Managers.Input.IsCursor)
            return;

        Vector2 mouseInput = context.ReadValue<Vector2>();
        Vector3 camAngle = transform.rotation.eulerAngles;

        // 위 아래
        moveAngleX = camAngle.x - (mouseInput.y * Managers.Option.gamePlayOption.mouseVirtical * 0.1f);

        // 좌우
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

    // 마우스 휠 입력값
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

        // 컨트롤러 위치 설정
        transform.position = target.transform.position + targetOffset;
        transform.rotation = Quaternion.Euler(new Vector3(moveAngleX, moveAnlgeY, 0.0f));

        // 휠 설정
        DetectRange = Mathf.Lerp(DetectRange, InputMouseWheel, 0.5f);

        // 카메라 위치 보정
        // 충돌하는 물체 체크
        CameraBlockCheck();

        followPosition.transform.position = deltaFollowPos;
    }

    #region 타켓 설정 후보정

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
        // 컨트롤러 위치
        Vector3 controllerPos = transform.position;
        // 카메라 관찰 위치
        Vector3 cameraPos = followPosition.position;

        // 거리 구해서 정규화
        float ctcDistance = Vector3.Distance(cameraPos, controllerPos);
        Vector3 dir = (cameraPos - controllerPos).normalized;

        // 컨트롤러 -> 카메라 사이에 layer 오브젝트 감지
        if (physics.Raycast(controllerPos, dir, out hit, ctcDistance + 0.1f, Managers.LayerManager.Ground | Managers.LayerManager.Wall))
        {
            // 카메라 위치가 hit 위치로 이동
            deltaFollowPos = hit.point;
        }
        // 실패시 카메라 -> 기본 카메라 위치 까지의 layer 오브젝트 감지
        else
        {
            // 카메라 뒤쪽 방향 벡터
            Vector3 backDir = -followPosition.forward;
            // 물체 충돌 없을시 기본 오브젝트 위치
            Vector3 defFollowPos = controllerPos + dir * DetectRange;
            // 현재 카메라 위치와 기본 카메라 위치의 거리값
            float restRange = Vector3.Distance(cameraPos, defFollowPos) + 0.1f;

            // 만약 뒤에 해당 layer 충돌이 감지되면 천천히 이동
            if (physics.Raycast(cameraPos, backDir, out hit, restRange, Managers.LayerManager.Ground))
            {
                // 카메라 위치 너프하게 기본 거리 적용
                deltaFollowPos = Vector3.Lerp(deltaFollowPos, hit.point, 0.5f);
            }
            else
            {
                // 없을시 기본 거리 설정
                deltaFollowPos = defFollowPos;
            }
        }
    }

    #endregion

    [ClientCallback]
    private void OnDestroy()
    {
        // 키 입력 제거
        if (Managers.Input._playerInput == null)
            return;
        Managers.Input._playerInput.actionEvents[1].RemoveListener(OnLook);
        mouseWheelAction.Disable();
    }
}
