using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using UnityEngine.InputSystem.Interactions;

// 1. ��ġ ����
// 2. ���� ����
public class PlayerController : BaseController
{
    static PlayerController instance;
    public static PlayerController Instance { get { return instance; } }

    [SerializeField]
    PhysicMaterial nonSlideMaterial;

    // ä�� �Էµ��� �÷��̾� ��ǲ�� ���� �ʰ� ����
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

    #region ��� üũ ����
    [Header("��� ���� �˻�")]
    [SerializeField, Tooltip("ĳ���Ͱ� ��� �� �� �ִ� �ִ� ��� �����Դϴ�.")]
    float maxSlopeAngle;
    [SerializeField, Tooltip("��� ������ üũ�� Raycast �߻� ���� �����Դϴ�.")]
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

    #region �ٴ� �� üũ ����
    [Header("�� üũ")]
    [SerializeField, Tooltip("ĳ���Ͱ� ���� �پ� �ִ��� Ȯ���ϱ� ���� CheckBox ���� �����Դϴ�.")]
    Transform groundCheck;
    [SerializeField]
    private bool isOnGround;
    public bool IsOnGround { get { return isOnGround; } protected set { isOnGround = value; } }

    // �ٴ� �浹 �ݶ��̴�
    Collider[] groundHitCol = new Collider[1];

   [SerializeField]
    private bool isOnWall;
    public bool IsOnWall { get { return isOnWall; } set { isOnWall = value; } }

    #endregion

    // �ʱ�ȭ
    public override void Init()
    {
        player = GetComponent<Player>();
        creature = player;
    }

    #region ����
    public virtual void StartAuthority()
    {
        base.OnStartAuthority();
        instance = this;

        // Ŀ�� ��Ȱ��ȭ
        Managers.Input.IsCursor = false;
        // ������ Ű ���ε� ����
        ClearBindKey();
        // ������ Ű ���ε�
        InitBindKey();
    }

    public virtual void StopAuthority()
    {
        base.OnStopAuthority();
        instance = null;
        // ������ Ű ���ε� ����
        ClearBindKey();
    }

    // Ű���� Ű ����
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

    // Ű���� Ű ���� ����
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
                Debug.Log("��ư �Է� �����");
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
                    // �÷��̾� UI �������̽��� ������ UI �ʱ�ȭ
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
        // ���콺�� UI ������ Ŭ���� �Ͼ���Ƿ� �ٷ� �����Ѵ�.
        if (Managers.Input.IsCursor || Managers.Input.IsOverUI)
            return;

        switch (context.phase)
        {
            case InputActionPhase.Performed:
                if (context.performed)
                {
                    if (context.interaction is HoldInteraction) // ���� ����
                    {

                    }
                    else if (context.interaction is PressInteraction) // �Ϲ� ����
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

    #region �÷��̾� ���� �Լ�

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

        // ���� ������ ��ġ �̸� ����Ͽ� ������� Ȯ���Ͽ� �̵����� ���� ����
        Vector3 calculatedDirection = 
            CalculateNextFrameGroundAngle(currentMoveSpeed) < maxSlopeAngle ? inputDirection : Vector3.zero;
    
        // ���� ������ ��ġ���� ���� ������ ���� ��������̰� ���̸� �������� ������ ���Ѵ�.
        calculatedDirection = (IsOnGround && IsOnSlope) ?
            AdjustDirectionToSlope(calculatedDirection) : calculatedDirection;

        return calculatedDirection;
    }
    
    // ��� Ȯ�� // ĳ���� �ؿ� Ray�� �μ� ������ Ȯ���ϴ� �Լ�
    public bool IsSlopeCheck()
    {
        if(creature.GetSetPhysics.Raycast(transform.position, Vector3.down, out slopeHit, SLOP_RAY_DISTANCE, Managers.LayerManager.Ground))
        {
            var angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle != 0f && angle < maxSlopeAngle;
        }
        return false;
    }

    // ������ ������ üũ�ڽ��� Ȯ��
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
        // ���� ������ ĳ���� �� �κ� ��ġ
        var nextFramePlayerPosition = precedingSlotCheck.position + inputDirection * moveSpeed * Time.fixedDeltaTime;
        
        // ���� ������ ��ġ���� ������� üũ�Ѵ�.
        if(creature.GetSetPhysics.Raycast(nextFramePlayerPosition, Vector3.down, out RaycastHit hitInfo, PRECEDING_RAY_DISTANCE, Managers.LayerManager.Ground))
        {
            return Vector3.Angle(Vector3.up, hitInfo.normal);
        }

        return 0f;
    }
    
    // �ش� ������ ���� ���� �븻���ͷ� �������� �Ѵ�.
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
