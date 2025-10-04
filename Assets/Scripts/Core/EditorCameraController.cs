// Author Oxe
// Created at 04.10.2025 10:26

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class EditorCameraController : MonoBehaviour
{
    [Header("Look")]
    //Нужно сделать так чтобы эти данные мы могли изменять в SettingMenu
    public float lookSensitivity = 0.15f;
    public float pitchMin = -85f;
    public float pitchMax =  85f;
    //
    public bool  holdMiddleMouseToLook = true;
    public bool  lockCursorWhileLooking = true;
    
    [Header("Move")]
    //Эти тоже
    public float moveSpeed = 6f;
    public float fastMultiplier = 3f;
    public float slowMultiplier = 0.35f;
    public float verticalSpeedScale = 1f;

    [Header("Scroll speed tuning")]
    public float scrollSpeedStep = 1f;
    public float minMoveSpeed = 0.5f;
    public float maxMoveSpeed = 50f;
    //
    float _yaw;
    float _pitch;

    void Start()
    {
        var euler = transform.rotation.eulerAngles;
        _yaw   = euler.y;
        _pitch = euler.x;
    }

    void Update()
    {
        float scroll = GetScroll();
        if (Mathf.Abs(scroll) > 0.0001f)
            moveSpeed = Mathf.Clamp(moveSpeed + scroll * scrollSpeedStep, minMoveSpeed, maxMoveSpeed);

        bool looking = !holdMiddleMouseToLook || GetMiddleMouse();
        if (looking)
        {
            Vector2 mDelta = GetMouseDelta();
            _yaw   += mDelta.x * lookSensitivity;
            _pitch -= mDelta.y * lookSensitivity;
            _pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);

            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

            if (lockCursorWhileLooking) SetCursorLock(true);
        }
        else if (lockCursorWhileLooking)
            SetCursorLock(false);

        Vector3 dir = Vector3.zero;
        float hor = GetAxisHorizontal();
        float ver = GetAxisVertical();
        dir += transform.forward * ver;
        dir += transform.right   * hor;

        float up = 0f;
        if (GetKeySpace()) up += 1f;
        if (GetKeyCtrl()  || GetKeyC()) up -= 1f;
        if (GetKeyQ()) up -= 1f;
        if (GetKeyE()) up += 1f;
        dir += Vector3.up * up * verticalSpeedScale;

        if (dir.sqrMagnitude > 0f)
        {
            dir.Normalize();
            float mult = 1f;
            if (GetKeyShift()) mult *= fastMultiplier;
            if (GetKeyCtrl())  mult *= slowMultiplier;
            transform.position += dir * (moveSpeed * mult * Time.deltaTime);
        }
    }

    void OnDisable()
    {
        SetCursorLock(false);
    }

    static float GetAxisHorizontal()
    {
        #if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k == null) return 0f;
        float v = 0f;
        if (k.aKey.isPressed || k.leftArrowKey.isPressed)  v -= 1f;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) v += 1f;
        return v;
        #else
        return Input.GetAxisRaw("Horizontal");
        #endif
    }

    static float GetAxisVertical()
    {
        #if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k == null) return 0f;
        float v = 0f;
        if (k.sKey.isPressed || k.downArrowKey.isPressed)  v -= 1f;
        if (k.wKey.isPressed || k.upArrowKey.isPressed)    v += 1f;
        return v;
        #else
        return Input.GetAxisRaw("Vertical");
        #endif
    }

    static Vector2 GetMouseDelta()
    {
        #if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
        #else
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        #endif
    }

    static float GetScroll()
    {
        #if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.scroll.ReadValue().y / 120f : 0f;
        #else
        return Input.mouseScrollDelta.y;
        #endif
    }

    static bool GetMiddleMouse()
    {
        #if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.middleButton.isPressed;
        #else
        return Input.GetMouseButton(2);
        #endif
    }

    static bool GetKeyShift()
    {
        #if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        return k != null && (k.leftShiftKey.isPressed || k.rightShiftKey.isPressed);
        #else
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        #endif
    }
    static bool GetKeyCtrl()
    {
        #if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        return k != null && (k.leftCtrlKey.isPressed || k.rightCtrlKey.isPressed);
        #else
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        #endif
    }
    static bool GetKeyC()
    {
        #if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current; return k != null && k.cKey.isPressed;
        #else
        return Input.GetKey(KeyCode.C);
        #endif
    }
    static bool GetKeySpace()
    {
        #if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current; return k != null && k.spaceKey.isPressed;
        #else
        return Input.GetKey(KeyCode.Space);
        #endif
    }
    static bool GetKeyQ()
    {
        #if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current; return k != null && k.qKey.isPressed;
        #else
        return Input.GetKey(KeyCode.Q);
        #endif
    }
    static bool GetKeyE()
    {
        #if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current; return k != null && k.eKey.isPressed;
        #else
        return Input.GetKey(KeyCode.E);
        #endif
    }

    static void SetCursorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !locked;
    }
}
