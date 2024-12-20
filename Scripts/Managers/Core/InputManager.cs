using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager
{
    public PlayerInput _playerInput;
 

    public void Init()
    {
        if(!Managers.Instance.TryGetComponent(out _playerInput))
        {
            _playerInput.actions = Managers.Resource.Load<InputActionAsset>("inputactions");
            _playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
        }
    }


    private bool isOverUI;
    public bool IsOverUI { get { return isOverUI; } }

    private bool isCursor;
    public bool IsCursor { 
        get {  return isCursor; }
        set
        {
            isCursor = value;
            if (value)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }
    }

#if !UNITY_SERVER
    public void Update()
    {
        isOverUI = EventSystem.current.IsPointerOverGameObject();
    }
#endif
}
