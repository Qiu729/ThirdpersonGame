using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;
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
        LockCursor,
    }
    /// <summary>
    /// Input structure sent over network to the server.
    /// ͨ�����緢�͵�������������ṹ��
    /// </summary>
    public struct NetworkedInput : INetworkInput {
        public Vector2 MoveDirection;
        public Vector2 LookRotationDelta;
        public NetworkButtons Buttons;
    }
    /// <summary>
    /// Handles all player input including movement, look, jump, sprint, aim, fire, and weapon switching.
    /// Processes input from keyboard/mouse and sends it over the network to the server.
    /// 处理所有玩家输入，包括移动、视角、跳跃、冲刺、瞄准、射击和武器切换。
    /// 处理键盘/鼠标输入并通过网络发送到服务器。
    /// </summary>
    public sealed class StarterAssetsInputs : NetworkBehaviour, IBeforeUpdate {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;
        public bool aim;
        public bool fire;
        public bool shopPanel;
        //public bool armed;
        public bool crouch;
        public bool rifle;
        public bool pistol;
        public bool knife;
        public bool lockCursor;
        public bool reload;

        private bool enableFire = true; // �Ƿ����ÿ�������
        private bool enableAim = true;
        //private bool enableArmed = true;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        public static float LookSensitivity;
        private NetworkedInput _accumulatedInput;
        private Vector2Accumulator _lookRotationAccumulator = new Vector2Accumulator(0.02f, true);

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value) {
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value) {
            if (cursorInputForLook) {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnJump(InputValue value) {
            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value) {
            SprintInput(value.isPressed);
        }

        public void OnAim(InputValue value) {
            if (enableAim) {
                AimInput(value.isPressed);
            }
        }

        public void OnFire(InputValue value) {
            if (enableFire) {
                FireInput(value.isPressed);
            }
        }
        public void OnShopPanel(InputValue value) {
            ShopPanelInput(value.isPressed);
        }
        //public void OnArmed(InputValue value) {
        //    if (enableArmed) {
        //        ArmedInput(value.isPressed);
        //    }
        //}
        public void OnRifle(InputValue value) {
            RifleInput(value.isPressed);
        }
        public void OnPistol(InputValue value) {
            PistolInput(value.isPressed);
        }
        public void OnKnife(InputValue value) {
            KnifeInput(value.isPressed);
        }

        public void OnCrouch(InputValue value) {
            CrouchInput(value.isPressed);
        }

        public void OnLockCursor(InputValue value) {
            LockCursorInput(value.isPressed);
        }
        public void OnReload(InputValue value) {
            ReloadInput(value.isPressed);
        }

#endif


        public void MoveInput(Vector2 newMoveDirection) {
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection) {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState) {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState) {
            sprint = newSprintState;
        }

        public void AimInput(bool newAimState) {
            aim = newAimState;
        }


        public void FireInput(bool newFireState) {
            fire = newFireState;
        }
        public void ShopPanelInput(bool newShopPanelState) {
            shopPanel = newShopPanelState;
        }

        //public void ArmedInput(bool newArmedState) {
        //    armed = newArmedState;
        //}
        public void RifleInput(bool newRifleState) {
            rifle = newRifleState;
        }
        public void PistolInput(bool newPistolState) {
            pistol = newPistolState;
        }
        public void KnifeInput(bool newKnifeState) {
            knife = newKnifeState;
        }

        public void CrouchInput(bool newCrouchState) {
            crouch = newCrouchState;
        }
        public void LockCursorInput(bool newLockCursorState) {
            lockCursor = newLockCursorState;
        }
        public void ReloadInput(bool newReloadState) {
            reload = newReloadState;
        }

        private void OnApplicationFocus(bool hasFocus) {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState) {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }

        public void EnableFire() {
            enableFire = true;
        }

        public void DisableFire() {
            enableFire = false;
            fire = false; // ����fire״̬
        }

        public void EnableAim() {
            enableAim = true;
        }

        public void DisableAim() {
            enableAim = false;
            aim = false; // ����aim״̬
        }
        //public void EnableArmed() {
        //    enableArmed = true;
        //}

        //public void DisableArmed() {
        //    enableArmed = false;
        //    armed = false; // ����aim״̬
        //}

        //Spawned()�ص����Ա���Ϊ��Fusion��Awake()�����������������Ӳ�����Ч�󱻵��á�
        public override void Spawned() {
            if (HasInputAuthority == false)
                return;

            // Register to Fusion input poll callback.
            // ע�ᵽFusion������ѯ�ص���
            var networkEvents = Runner.GetComponent<NetworkEvents>();
            networkEvents.OnInput.AddListener(OnInput);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        //����OnDestory()
        public override void Despawned(NetworkRunner runner, bool hasState) {
            if (runner == null)
                return;

            var networkEvents = runner.GetComponent<NetworkEvents>();
            if (networkEvents != null) {
                networkEvents.OnInput.RemoveListener(OnInput);
            }
        }
        private void OnInput(NetworkRunner runner, NetworkInput networkInput) {
            // Mouse movement (delta values) is aligned to engine update.
            // To get perfectly smooth interpolated look, we need to align the mouse input with Fusion ticks.
            // ����ƶ�������ֵ����������¶��롣
            // Ϊ�˻������ƽ���Ĳ�ֵ�ӽǣ�������Ҫ�����������Fusion�δ���롣
            _accumulatedInput.LookRotationDelta = _lookRotationAccumulator.ConsumeTickAligned(runner);

            // Fusion polls accumulated input. This callback can be executed multiple times in a row if there is a performance spike.
            // Fusion��ѯ�ۻ������롣����������ܷ�ֵ���˻ص���������ִ�ж�Ρ�
            networkInput.Set(_accumulatedInput);
        }
        void IBeforeUpdate.BeforeUpdate() {
            // This method is called BEFORE ANY FixedUpdateNetwork() and is used to accumulate input from Keyboard/Mouse.
            // Input accumulation is mandatory - this method is called multiple times before new forward FixedUpdateNetwork() - common if rendering speed is faster than Fusion simulation.
            // �˷�����ANY FixedUpdateNetwork()֮ǰ���ã������ۻ����Լ���/�������롣
            // �����ۻ���ǿ���Ե� - ���µ�ǰ��FixedUpdateNetwork()֮ǰ���˷�������ε��� - �����Ⱦ�ٶȱ�Fusionģ��죬���ǳ����ġ�

            if (HasInputAuthority == false)
                return;

            // Escape key is used for locking/unlocking cursor in game view.
            // Escape ����������Ϸ��ͼ������/������ꡣ
            var keyboard = Keyboard.current;
            if (lockCursor) {
                if (Cursor.lockState == CursorLockMode.Locked) {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                } else {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }

            // Accumulate input only if the cursor is locked.
            // ֻ���ڹ������ʱ���ۻ����롣
            if (Cursor.lockState != CursorLockMode.Locked)
                return;

            var mouse = Mouse.current;
            if (mouse != null) {
                var mouseDelta = mouse.delta.ReadValue();

                var lookRotationDelta = new Vector2(-mouseDelta.y, mouseDelta.x);
                lookRotationDelta *= LookSensitivity / 60f;
                _lookRotationAccumulator.Accumulate(lookRotationDelta);

                //_accumulatedInput.Buttons.Set(EInputButton.Fire, mouse.leftButton.isPressed);
            }

            if (keyboard != null) {
                var moveDirection = Vector2.zero;

                if (keyboard.wKey.isPressed) { moveDirection += Vector2.up; }
                if (keyboard.sKey.isPressed) { moveDirection += Vector2.down; }
                if (keyboard.aKey.isPressed) { moveDirection += Vector2.left; }
                if (keyboard.dKey.isPressed) { moveDirection += Vector2.right; }

                _accumulatedInput.MoveDirection = moveDirection.normalized;

                _accumulatedInput.Buttons.Set(EInputButton.Jump, jump);
                _accumulatedInput.Buttons.Set(EInputButton.Fire, fire);
                _accumulatedInput.Buttons.Set(EInputButton.Reload, reload);
                _accumulatedInput.Buttons.Set(EInputButton.Pistol, pistol);
                _accumulatedInput.Buttons.Set(EInputButton.Rifle, rifle);
                _accumulatedInput.Buttons.Set(EInputButton.Knife, knife);
                _accumulatedInput.Buttons.Set(EInputButton.Crouch, crouch);
                _accumulatedInput.Buttons.Set(EInputButton.ShopPanel, shopPanel);
                _accumulatedInput.Buttons.Set(EInputButton.Aim, aim);
                _accumulatedInput.Buttons.Set(EInputButton.Sprint, sprint);
                _accumulatedInput.Buttons.Set(EInputButton.LockCursor, lockCursor);
            }
        }
    }

}