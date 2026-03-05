using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ThirdpersonGame {
    public enum EInputButton {
        Jump,
        Fire,
        Reload,
        Pistol,
        Rifle,
        Knife,
        Crouch,
        ShopPanel,
        Aim,
        Sprint,
    }

    // 发送给服务器的输入数据结构
    public struct NetworkedInput : INetworkInput {
        public Vector2 MoveDirection;
        public Vector2 Look;
        public Vector3 AimPosition;
        public NetworkButtons Buttons;
    }

    [DefaultExecutionOrder(-10)]
    public sealed class StarterAssetsInputs : NetworkBehaviour {

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;

        public static float LookSensitivity; // 暂时没有添加到计算中

        // 角色移动需要用到的参数
        public RectTransform joystickRectTransform; // 摇杆UI区域
        private int? lookTouchId = null; // 追踪用于视角控制的触摸ID
        private int? stickTouchID = null; // 追踪用于移动控制的触摸ID

        public Vector2 lookRotation => _accumulatedInput.Look;
        private Vector2 _stickMoveDirection;
        private PlayerInput playerInput;
        private bool _isMoving => _stickMoveDirection.sqrMagnitude > 0.01f;
        private NetworkedInput _accumulatedInput;

        public override void Spawned() {
            if (HasInputAuthority == false)
                return;

            // Register to Fusion input poll callback.
            var networkEvents = Runner.GetComponent<NetworkEvents>();
            networkEvents.OnInput.AddListener(OnInput);

            playerInput = GetComponent<PlayerInput>();

            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;
        }

        public override void Despawned(NetworkRunner runner, bool hasState) {
            if (runner == null)
                return;

            var networkEvents = runner.GetComponent<NetworkEvents>();
            if (networkEvents != null) {
                networkEvents.OnInput.RemoveListener(OnInput);
            }
        }
        private void OnInput(NetworkRunner runner, NetworkInput networkInput) {
            // Fusion polls accumulated input. This callback can be executed multiple times in a row if there is a performance spike.
            networkInput.Set(_accumulatedInput);
        }

        private void Update() {
            if (HasInputAuthority == false)
                return;

            // Escape key is used for locking/unlocking cursor in game view.
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            var touch = Touchscreen.current;

            if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)) {
                if (Cursor.lockState == CursorLockMode.Locked) {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                } else {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }

            // Accumulate input only if the cursor is locked.
            if (Cursor.lockState != CursorLockMode.Locked) {
                _accumulatedInput.MoveDirection = default;
                return;
            }

            foreach (var touchInput in touch.touches) {
                if (touchInput.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began) {
                    if (IsInJoystickArea(touchInput.position.ReadValue())) {
                        stickTouchID = touchInput.touchId.ReadValue();
                    } else {
                        lookTouchId = touchInput.touchId.ReadValue();
                    }
                } else if (touchInput.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved) {
                    if (touchInput.touchId.ReadValue() == stickTouchID) {
                        _stickMoveDirection = touchInput.value.position - touchInput.startPosition.ReadValue();
                        //Debug.Log("摇杆输入: " + _accumulatedInput.MoveDirection);
                    } else if (touchInput.touchId.ReadValue() == lookTouchId) {
                        //Debug.Log("视角控制输入: " + touchInput.delta.ReadValue());
                        var lookRotationDelta = new Vector2(-touchInput.delta.y.ReadValue(), touchInput.delta.x.ReadValue()) * 0.1f;
                        _accumulatedInput.Look = ClampLookRotation(_accumulatedInput.Look + lookRotationDelta);
                    }
                } else if (touchInput.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended) {
                    if (touchInput.touchId.ReadValue() == lookTouchId) {
                        lookTouchId = null;
                    } else if (touchInput.touchId.ReadValue() == stickTouchID) {
                        stickTouchID = null;
                        _stickMoveDirection = Vector2.zero;
                    }
                }
            }

            // Mouse look input
            if (mouse != null) {
                var lookRotationDelta = new Vector2(-mouse.delta.y.ReadValue(), mouse.delta.x.ReadValue()) * 0.1f;
                _accumulatedInput.Look = ClampLookRotation(_accumulatedInput.Look + lookRotationDelta);
            }


            // Movement input using new Input System
            Vector2 moveDirection = _stickMoveDirection;
            if (keyboard != null) {
                if (keyboard.wKey.isPressed) moveDirection.y += 1;
                if (keyboard.sKey.isPressed) moveDirection.y -= 1;
                if (keyboard.aKey.isPressed) moveDirection.x -= 1;
                if (keyboard.dKey.isPressed) moveDirection.x += 1;
            }
            _accumulatedInput.MoveDirection = moveDirection.normalized;



            Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, LayerMask.GetMask("Default", "Player", "Ground"))) {
                var aimPosition = raycastHit.point;
                _accumulatedInput.AimPosition = aimPosition;
            }

            if (mouse != null) {
                _accumulatedInput.Buttons.Set(EInputButton.Fire, mouse.leftButton.isPressed);
                _accumulatedInput.Buttons.Set(EInputButton.Aim, mouse.rightButton.isPressed);
            }

            if (keyboard != null) {
                _accumulatedInput.Buttons.Set(EInputButton.Jump, keyboard.spaceKey.isPressed);
                _accumulatedInput.Buttons.Set(EInputButton.Reload, keyboard.rKey.isPressed);
                _accumulatedInput.Buttons.Set(EInputButton.Pistol, keyboard.digit2Key.isPressed);
                _accumulatedInput.Buttons.Set(EInputButton.Rifle, keyboard.digit1Key.isPressed);
                _accumulatedInput.Buttons.Set(EInputButton.Knife, keyboard.digit3Key.isPressed);
                _accumulatedInput.Buttons.Set(EInputButton.Crouch, keyboard.leftCtrlKey.isPressed);
                _accumulatedInput.Buttons.Set(EInputButton.ShopPanel, keyboard.bKey.isPressed);
                _accumulatedInput.Buttons.Set(EInputButton.Sprint, keyboard.leftShiftKey.isPressed);
            }
        }

        private Vector2 ClampLookRotation(Vector2 lookRotation) {
            lookRotation.x = Mathf.Clamp(lookRotation.x, -30f, 70f);
            return lookRotation;
        }

        private bool IsInJoystickArea(Vector2 screenPosition) {
            if (joystickRectTransform == null) return false;

            // 将屏幕坐标转换为 RectTransform 的本地坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickRectTransform,
                screenPosition,
                null, // 使用主相机（如果是 Canvas 是 Screen Space - Overlay）
                out Vector2 localPoint
            );

            // 使用 rect.rect.Contains 判断点是否在矩形内
            return joystickRectTransform.rect.Contains(localPoint);
        }

    }

}