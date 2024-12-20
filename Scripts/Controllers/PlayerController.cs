using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using UnityEngine.InputSystem.Interactions;

// 1. 위치 벡터
// 2. 방향 벡터
public class PlayerController : BaseController
{
    static PlayerController instance;
    public static PlayerController Instance { get { return instance; } }

    [SerializeField]
    PhysicMaterial nonSlideMaterial;

    // 채팅 입력동안 플레이어 인풋값 받지 않게 끄기
    public static void SetInputField(bool isActive)
    {
        if (Managers.Input != null)
            Managers.Input._playerInput.enabled = isActive;
    }

    public Player player { get; private set; }

    private Vector3 input;
    [SerializeField, SyncVar]
    private Vector3 inputDir;
    public Vector3 inputDirection { get { return inputDir; } private set { inputDir = value; } }
    public Vector3 calculatedDirection { get; private set; }

    [SerializeField, SyncVar]
    private bool isJump;
    public bool IsJump { get { return isJump; } private set { isJump = value; } }

    [SerializeField, SyncVar]
    private bool isDash;
    public bool IsDash { get { return isDash; } private set { isDash = value; } }

    [SerializeField, SyncVar]
    private bool isRun;
    public bool IsRun { get {  return isRun; } private set { isRun = value; } }

    [SerializeField, SyncVar]
    private bool isAttack;
    public bool IsAttack { get { return isAttack; } private set { isAttack = value; } }

    [SerializeField, SyncVar]
    private Vector3 localCameraFwd;
    public Vector3 LocalCameraFwd { get {  return localCameraFwd; } private set {  localCameraFwd = value; } }

    #region 경사 체크 변수
    [Header("경사 지형 검사")]
    [SerializeField, Tooltip("캐릭터가 등반 할 수 있는 최대 경사 각도입니다.")]
    float maxSlopeAngle;
    [SerializeField, Tooltip("경사 지형을 체크할 Raycast 발사 시작 지점입니다.")]
    Transform precedingSlotCheck;
    private const float PRECEDING_RAY_DISTANCE = 2f;
    private const float SLOP_RAY_DISTANCE = 1.0f;
    private RaycastHit slopeHit;
    [SerializeField]
    private bool isOnSlope;
    public bool IsOnSlope { get { return isOnSlope; }
        protected set
        {
            isOnSlope = value;
            if (value)
                player.GetCollider.material = nonSlideMaterial;
            else
                player.GetCollider.material = null;
        }
    }
    #endregion

    #region 바닥 벽 체크 변수
    [Header("땅 체크")]
    [SerializeField, Tooltip("캐릭터가 땅에 붙어 있는지 확인하기 위한 CheckBox 시작 지점입니다.")]
    Transform groundCheck;
    [SerializeField]
    private bool isOnGround;
    public bool IsOnGround { get { return isOnGround; } protected set { isOnGround = value; } }

    // 바닥 충돌 콜라이더
    Collider[] groundHitCol = new Collider[1];

   [SerializeField]
    private bool isOnWall;
    public bool IsOnWall { get { return isOnWall; } set { isOnWall = value; } }

    #endregion

    // 초기화
    public override void Init()
    {
        player = GetComponent<Player>();
        creature = player;
    }

    #region 권한
    public virtual void StartAuthority()
    {
        base.OnStartAuthority();
        instance = this;

        // 커서 비활성화
        Managers.Input.IsCursor = false;
        // 움직임 키 바인드 해제
        ClearBindKey();
        // 움직임 키 바인드
        InitBindKey();
    }

    public virtual void StopAuthority()
    {
        base.OnStopAuthority();
        instance = null;
        // 움직임 키 바인드 해제
        ClearBindKey();
    }

    // 키보드 키 세팅
    public virtual void InitBindKey()
    {
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.Move].AddListener(OnMove);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.Jump].AddListener(OnJump);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.Dash].AddListener(OnDash);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.Run].AddListener(OnRun);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.NormalAttack].AddListener(OnPlayerCilcked);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.NormalAttack].AddListener(OnNormalAttack);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.PickUp].AddListener(OnPickUp);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.Esc].AddListener(OnActiveCursor);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.I].AddListener(OnShowInventory);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.P].AddListener(OnShowParty);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.M].AddListener(OnShowMyPlayerStat);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.U].AddListener(OnShowUserEquipment);
    }

    // 키보드 키 세팅 해제
    public virtual void ClearBindKey()
    {
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.Move].RemoveListener(OnMove);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.Jump].RemoveListener(OnJump);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.Dash].RemoveListener(OnDash);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.Run].RemoveListener(OnRun);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.NormalAttack].RemoveListener(OnPlayerCilcked);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.NormalAttack].RemoveListener(OnNormalAttack);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.PickUp].RemoveListener(OnPickUp);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.Esc].RemoveListener(OnActiveCursor);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.I].RemoveListener(OnShowInventory);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.P].RemoveListener(OnShowParty);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.M].RemoveListener(OnShowMyPlayerStat);
        Managers.Input._playerInput.actionEvents[(int)Define.PlayerInputAction.U].RemoveListener(OnShowUserEquipment);
    }

    #endregion

    #region PlayerInput
    public void OnMove(InputAction.CallbackContext context)
    {
        if (Managers.Input.IsCursor)
        {
            input = Vector2.zero;
            return;
        }

        input = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (Managers.Input.IsCursor)
        {
            IsJump = false;
            return;
        }

        switch (context.phase)
        {
            case InputActionPhase.Performed:
                IsJump = true;
                break;
            case InputActionPhase.Canceled:
                IsJump = false;
                break;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (Managers.Input.IsCursor)
        {
            IsDash = false;
            return;
        }

        switch (context.phase)
        {
            case InputActionPhase.Started:
                IsDash = true;
                break;
            case InputActionPhase.Canceled:
                IsDash = false;
                break;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (Managers.Input.IsCursor)
        {
            IsRun = false;
            return;
        }

        switch (context.phase)
        {
            case InputActionPhase.Performed:
                IsRun = true;
                break;
            case InputActionPhase.Canceled:
                IsRun = false;
                break;
            case InputActionPhase.Waiting:
                Debug.Log("버튼 입력 대기중");
                break;
        }
    }

    public void OnPlayerCilcked(InputAction.CallbackContext context)
    {
        if (!Managers.Input.IsCursor || Managers.Input.IsOverUI)
            return;

        if (context.performed)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            Vector3 startPos = Camera.main.ScreenToWorldPoint(mousePos);
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            PhysicsScene ps = creature.GetSetPhysics;

            if(ps.Raycast(startPos, ray.direction, out RaycastHit hitInfo, 25.0f, Managers.LayerManager.Player))
            {
                GameObject target = hitInfo.transform.gameObject;

                if (target == player.gameObject)
                    return;

                if(target.TryGetComponent(out Player otherPlayer))
                {
                    // 플레이어 UI 인터페이스가 있으면 UI 초기화
                    UI_Scene ui_scene = Managers.UI.SceneUI;
                    if (ui_scene is IPlayerUI)
                    {
                        IPlayerUI playerUI = ui_scene as IPlayerUI;
                        playerUI.GetPlayerUI.OpenPlayerInteraction(mousePos, otherPlayer);
                    }
                }
            }

        }
    }

    public void OnNormalAttack(InputAction.CallbackContext context)
    {
        // 마우스가 UI 위에서 클릭이 일어났으므로 바로 리턴한다.
        if (Managers.Input.IsCursor || Managers.Input.IsOverUI)
            return;

        switch (context.phase)
        {
            case InputActionPhase.Performed:
                if (context.performed)
                {
                    if (context.interaction is HoldInteraction) // 차지 공격
                    {

                    }
                    else if (context.interaction is PressInteraction) // 일반 공격
                    {
                        isAttack = true;
                    }
                }
                break;
            case InputActionPhase.Canceled:
                Invoke("DelayAttackButtonUp", 0.1f);
                break;
        }
    }

    public void OnPickUp(InputAction.CallbackContext context)
    {
        if (Managers.Input.IsCursor)
            return;

        switch (context.phase)
        {
            case InputActionPhase.Performed:
                PickUpItem();
                break;
        }
    }

    public void OnActiveCursor(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                Managers.Input.IsCursor = !Managers.Input.IsCursor;
                break;
        }
    }

    public void OnShowInventory(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                UI_Scene ui_scene = Managers.UI.SceneUI;
                if (ui_scene is IPlayerUI)
                {
                    IPlayerUI playerUI = ui_scene as IPlayerUI;
                    playerUI.GetPlayerUI.ShowInventory();
                }
                break;
        }
    }

    public void OnShowParty(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                UI_Scene ui_scene = Managers.UI.SceneUI;
                if (ui_scene is IPlayerUI)
                {
                    IPlayerUI playerUI = ui_scene as IPlayerUI;
                    playerUI.GetPlayerUI.ShowParty();
                }
                break;
        }
    }

    public void OnShowMyPlayerStat(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                UI_Scene ui_scene = Managers.UI.SceneUI;
                if (ui_scene is IPlayerUI)
                {
                    IPlayerUI playerUI = ui_scene as IPlayerUI;
                    playerUI.GetPlayerUI.ShowStat();
                }
                break;
        }
    }

    public void OnShowUserEquipment(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                UI_Scene ui_scene = Managers.UI.SceneUI;
                if (ui_scene is IPlayerUI)
                {
                    IPlayerUI playerUI = ui_scene as IPlayerUI;
                    playerUI.GetPlayerUI.ShowEquipment();
                }
                break;
        }
    }

    Collider[] col = new Collider[1];
    [Command]
    void PickUpItem()
    {
        if (creature.GetSetPhysics.OverlapSphere(transform.position, 1f, col, Managers.LayerManager.WorldItem, QueryTriggerInteraction.Collide) > 0)
        {
            if (col[0].TryGetComponent(out WorldItem worldItem))
            {
                if (worldItem.GetItem() != null)
                {
                    if (connectionToClient != null)
                        OnPickUpItemSound(connectionToClient);
                    Item item = player.Inventory.Add(worldItem.GetItem());
                    if (item == null)
                    {
                        worldItem.DestroyItem();
                    }
                }
            }
            col.Initialize();
        }
    }

    [TargetRpc]
    void OnPickUpItemSound(NetworkConnection conn)
    {
        Managers.Sound.Play2D("FX/ItemPickup",Define.Sound2D.Effect2D);
    }

    void DelayAttackButtonUp()
    {
        isAttack = false;
    }

    #endregion

    #region 플레이어 무브 함수

    private void Update()
    {
#if UNITY_SERVER
        calculatedDirection = GetDirection(player.GetStat.MoveSpeed);
        ControlGravity();
#else
        if (netIdentity.isOwned)
        {
            Vector3 forward = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
            Vector3 right = new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z);
            localCameraFwd = forward;
            inputDirection = (forward * input.y + right * input.x).normalized;

            calculatedDirection = GetDirection(creature.GetStat.MoveSpeed);
            ControlGravity();
        }
        else if (Managers.Instance.mode == NetworkManagerMode.Host)
        {
            calculatedDirection = GetDirection(creature.GetStat.MoveSpeed);
            ControlGravity();
        }
#endif
    }

    protected Vector3 GetDirection(float currentMoveSpeed)
    {
        IsOnSlope = IsSlopeCheck();
        IsOnGround = IsGroundCheck();

        // 다음 프레임 위치 미리 계산하여 언덕인지 확인하여 이동할지 말지 결정
        Vector3 calculatedDirection = 
            CalculateNextFrameGroundAngle(currentMoveSpeed) < maxSlopeAngle ? inputDirection : Vector3.zero;
    
        // 다음 프레임 위치에서 구한 방향이 현재 경사진곳이고 땅이면 프로젝션 방향을 구한다.
        calculatedDirection = (IsOnGround && IsOnSlope) ?
            AdjustDirectionToSlope(calculatedDirection) : calculatedDirection;

        return calculatedDirection;
    }
    
    // 경사 확인 // 캐릭터 밑에 Ray를 싸서 땅인지 확인하는 함수
    public bool IsSlopeCheck()
    {
        if(creature.GetSetPhysics.Raycast(transform.position, Vector3.down, out slopeHit, SLOP_RAY_DISTANCE, Managers.LayerManager.Ground))
        {
            var angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle != 0f && angle < maxSlopeAngle;
        }
        return false;
    }

    // 땅인지 피직스 체크박스로 확인
    public bool IsGroundCheck()
    {
        Vector3 boxSize = new Vector3(transform.lossyScale.x * 0.2f, 0.01f, transform.lossyScale.z * 0.2f);
        if (creature.GetSetPhysics.OverlapBox(groundCheck.position, boxSize, groundHitCol, transform.rotation, Managers.LayerManager.Ground | Managers.LayerManager.Wall) > 0)
        {
            groundHitCol.Initialize();
            return true;
        }

        return false;
    }

    public float CalculateNextFrameGroundAngle(float moveSpeed)
    {
        // 다음 프레임 캐릭터 앞 부분 위치
        var nextFramePlayerPosition = precedingSlotCheck.position + inputDirection * moveSpeed * Time.fixedDeltaTime;
        
        // 다음 프레임 위치에서 경사인지 체크한다.
        if(creature.GetSetPhysics.Raycast(nextFramePlayerPosition, Vector3.down, out RaycastHit hitInfo, PRECEDING_RAY_DISTANCE, Managers.LayerManager.Ground))
        {
            return Vector3.Angle(Vector3.up, hitInfo.normal);
        }

        return 0f;
    }
    
    // 해당 방향을 현재 땅의 노말벡터로 프로젝션 한다.
    public Vector3 AdjustDirectionToSlope(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal);
    }

    protected void ControlGravity()
    {
        if(IsOnGround && IsOnSlope)
        {
            creature.GetRigidBody.useGravity = false;
            return;
        }

        if (isOnWall == false)
            creature.GetRigidBody.useGravity = true;
    }
    #endregion
}
