using Cinemachine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using ThirdpersonGame;
using Unity.VisualScripting;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.InputSystem;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.XR;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

/// <summary>
/// 第三人称控制器，控制角色移动、旋转、跳跃、爬梯子、相机控制功能。
/// </summary>
namespace ThirdpersonGame {
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput))]
#endif
    public class ThirdPersonController : NetworkBehaviour {

        public SimpleKCC simpleKCC; //Kinematic Character Controller，运动学角色控制器
        public HitboxRoot hitboxRoot;
        public GameObject AimCameraRoot;
        public GameObject ThirdPersonRoot;
        //public Weapons Weapons;
        private SceneObjects _sceneObjects;
        public Health health;
        public Transform CameraHandle;
        public float UpGravity = 15f;
        public float DownGravity = 25f;
        public float JumpForce = 10f;

        [Networked] private NetworkButtons _previousButtons { get; set; }
        [Networked] private int _jumpCount { get; set; }
        [Networked] private Vector3 _moveVelocity { get; set; }
        [Networked] private Vector2 _currentMoveInput { get; set; } // 存储当前移动输入

        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;
        public float AimmingMoveSpeed;
        public float CrouchMoveSpeed;
        public float ClimbLadderSpeed;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;
        public float AimmingSprintSpeed;
        public float CrouchSprintSpeed;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        [Header("Mouse Settings")]
        [Tooltip("鼠标灵敏度，影响相机旋转速度")]
        [Range(0.01f, 10f)]
        public float MouseSensitivity = 1.0f;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private bool isClimbingLadder = false;
        private Vector3 lastGrabLadderDirection;
        public bool enableRotation = true;
        private bool hitSomething = false; //在fixupdate中使用射线检查面前是否有物体
        private bool hitSomething2 = false; // 使用lastGrabLadderDirection检测
        private bool hitLadder = false;
        private bool hitLadder2 = false;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM 
        private UnityEngine.InputSystem.PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private ThirdPersonAimController _thirdPersonAimController;
        private RigManager _rigManager;
        private WeaponManager weaponManager;
        private GameObject _mainCamera;
        private HandleInput _handleInput;
        private bool _rotateOnMove = true; //是否由当前脚本控制角色旋转

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse {
            get {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake() {
            // get a reference to our main camera
            if (_mainCamera == null) {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
            weaponManager = GetComponent<WeaponManager>();
            _rigManager = GetComponent<RigManager>();
            _thirdPersonAimController = GetComponent<ThirdPersonAimController>();
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();

#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif
            _handleInput = GetComponent<HandleInput>();

        }

        private void Start() {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;


            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update() {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            UpdateRigWeight();
        }

        private void OnAnimatorMove() {

            Move();
        }

        private void FixedUpdate() {
            CheckForwardObject();
        }

        //private void LateUpdate() {
        //    CameraRotation();
        //}

        private void AssignAnimationIDs() {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck() {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            if (!isClimbingLadder) {
                Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);
            } else {
                Grounded = true;
            }

            // update animator if using character
            if (_hasAnimator) {
                //Debug.Log($"[GroundedCheck] Grounded: {Grounded}");
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }



        private void CheckForwardObject() {
            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            float avoidFloorDistance = .1f; // 射线发射高度
            float ladderGrabDistance = .4f; // 射线长度

            // 绘制射线debug
            //Debug.DrawRay(
            //    transform.position + Vector3.up * avoidFloorDistance,
            //    targetDirection.normalized * ladderGrabDistance,  // 射线长
            //    Color.magenta,  // 颜色
            //    0.5f           // 显示0.5秒
            //);

            if (Physics.Raycast(transform.position + Vector3.up * avoidFloorDistance,
                    targetDirection, out RaycastHit raycastHit, ladderGrabDistance)) {
                hitSomething = true;
                if (raycastHit.transform.TryGetComponent(out Ladder ladder)) {
                    hitLadder = true;
                } else {
                    hitLadder = false;
                }
            } else {
                hitSomething = hitLadder = false;
            }
            if (Physics.Raycast(transform.position + Vector3.up * avoidFloorDistance,
                    lastGrabLadderDirection, out RaycastHit hit, ladderGrabDistance)) {
                hitSomething2 = true;
                if (hit.transform.TryGetComponent(out Ladder ladder2)) {
                    hitLadder2 = true;
                } else {
                    hitLadder2 = false;
                }
            } else {
                hitSomething2 = hitLadder2 = false;
            }
            // Debug.Log($"hitsomething:{hitSomething}, hitLadder:{hitLadder}");
        }

        private void Move() {

            if (_thirdPersonAimController == null || _handleInput == null
                || _input == null) {
                Debug.LogWarning("Move()包含空组件");
                return;
            }

            // set target speed based on move speed, sprint speed and if sprint is pressed
            bool isAimming = _thirdPersonAimController.GetIsAimming();
            bool isCrouch = _handleInput.GetIsCrouch();
            float targetSpeed = 0f;

            if (isAimming) {
                targetSpeed = _input.sprint ? AimmingSprintSpeed : AimmingMoveSpeed;
            } else {
                targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            }

            if (isCrouch) {
                targetSpeed = _input.sprint ? CrouchSprintSpeed : CrouchMoveSpeed;
            }

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset) {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            } else {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero) {
                // 相对摄像机方向的输入方向
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                // 网络同步时，无法使用这种方法实现平滑转身，暂时直接转身
                //float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                //    RotationSmoothTime);
                float rotation = _targetRotation;

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            if (!isClimbingLadder) {
                if (hitSomething) {
                    if (hitLadder) {
                        GrabLadder(targetDirection);
                    }
                }
            } else {
                if (hitSomething2) {
                    if (!hitLadder2) {
                        // 前方不是梯子
                        DropLadder();
                        _verticalVelocity = 4f;
                    }
                } else {
                    // 前方没有梯子
                    DropLadder();
                    _verticalVelocity = 4f;
                    // 播放登顶动画
                    // _animator.SetBool("ClimbToTop", true);
                }
                if (Vector3.Dot(targetDirection, lastGrabLadderDirection) < 0) {
                    // 正在向下爬梯子
                    float ladderFloorDropDistance = .1f;
                    if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit floorDropDistance,
                        ladderFloorDropDistance)) {
                        DropLadder();
                    }
                }
            }

            if (isClimbingLadder) {
                targetDirection.x = 0f; // 禁止水平移动
                targetDirection.y = targetDirection.z;
                targetDirection.z = 0f;
                _verticalVelocity = 0f; // 禁止垂直速度
                _animator.SetFloat("ClimbSpeed", 0);
                if (_input.move.y != 0) {
                    _speed = ClimbLadderSpeed;
                    if (_input.move.y > 0) {
                        _animator.SetFloat("ClimbSpeed", 1.2f); // 上梯子动画
                    } else {
                        _animator.SetFloat("ClimbSpeed", -1.2f);
                    }
                }
            }

            // move the player
            Vector3 moveVector = targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;

            // 然后用 moveVector 代替原有的 _controller.Move(...) 参数
            _controller.Move(moveVector);

            // update animator if using character
            if (_hasAnimator) {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }

        }

        private void GrabLadder(Vector3 lastGrabLadderDirection) {
            //Debug.Log("GrabLadder被调用");
            isClimbingLadder = true;
            // 切换到空手
            weaponManager.ToKnife();
            this.lastGrabLadderDirection = lastGrabLadderDirection;
            // 禁用转身
            enableRotation = false;
            // 播放动画
            _animator.SetBool("Climb", true);
            _animator.SetFloat("ClimbSpeed", 0);
        }

        private void DropLadder() {
            isClimbingLadder = false;
            enableRotation = true;
            //_input.EnableAim();
            //_input.EnableFire();
            //_input.EnableArmed();
            _animator.SetBool("Climb", false);
        }

        private void JumpAndGravity() {

            // Debug.Log($"_input.jump: {_input.jump}");
            if (Grounded) {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator) {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                    // Debug.Log("[JumpAnim] Set Jump=false, FreeFall=false (Grounded)");
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f) {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f) {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator) {
                        _animator.SetBool(_animIDJump, true);
                        // Debug.Log("[JumpAnim] Set Jump=true (Jump Triggered)");
                    }

                    OnAir();
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f) {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            } else {

                // Debug.Log("not in grounded state");

                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f) {
                    _fallTimeoutDelta -= Time.deltaTime;
                } else {
                    // update animator if using character
                    if (_hasAnimator) {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                    OnAir();
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity) {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax) {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected() {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent) {
            if (animationEvent.animatorClipInfo.weight > 0.5f) {
                if (FootstepAudioClips.Length > 0) {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent) {
            if (animationEvent.animatorClipInfo.weight > 0.5f) {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
            _rigManager.SetLeftHandGridRig(1f);
        }

        private void OnAir() {
            // 在空中时，包括跳跃和自由落体状态
            _rigManager.SetLeftHandGridRig(0f);
        }

        public void SetSensitivity(float newSensitivity) {
            MouseSensitivity = newSensitivity;
        }

        public void SetrotateOnMove(bool newRotateOnMove) {
            _rotateOnMove = newRotateOnMove;
        }

        private void UpdateRigWeight() {
            //根据速度更新rig权重
            if (_animator.GetFloat(_animIDSpeed) >= 0.1f) {
                // 移动时，设置rig权重为0f
                _rigManager.SetUpperBodyAimRigWeight(0f);
                _rigManager.SetHeadAimRigWeight(0f);
            } else {
                _rigManager.SetUpperBodyAimRigWeight(0.7f);
                _rigManager.SetHeadAimRigWeight(0.7f);
            }

            //if (_animator.GetBool(_animIDJump) || _animator.GetBool(_animIDFreeFall) || weaponManager.currentWeaponType == WeaponType.Knife) {
            //    // 跳跃或自由落体时，设置左手网格rig权重为0f
            //    _rigManager.SetLeftHandGridRig(0f);
            //} else {
            //    _rigManager.SetLeftHandGridRig(1f);
            //}
        }
        public override void Spawned() {
            name = $"{Object.InputAuthority} ({(HasInputAuthority ? "Input Authority" : (HasStateAuthority ? "State Authority" : "Proxy"))})";

            // Enable first person visual for local player, third person visual for proxies.
            // 为本地玩家启用第一人称视觉，为代理启用第三人称视觉。
            SetFirstPersonVisuals(HasInputAuthority);

            if (HasInputAuthority == false) {
                // Virtual cameras are enabled only for local player.
                // 虚拟相机只为本地玩家启用。
                var virtualCameras = GetComponentsInChildren<CinemachineVirtualCamera>(true);
                for (int i = 0; i < virtualCameras.Length; i++) {
                    virtualCameras[i].enabled = false;
                }
            }

            _sceneObjects = Runner.GetSingleton<SceneObjects>();
        }
        private void SetFirstPersonVisuals(bool firstPerson) {
            AimCameraRoot.SetActive(firstPerson);
            //ThirdPersonRoot.SetActive(firstPerson == false);

            //Weapons.SetFirstPersonVisuals(firstPerson);
        }
        public override void FixedUpdateNetwork() {
            if (health == null) {
                Debug.LogWarning("health为空");
                return;
            }
            if (simpleKCC == null) {
                Debug.LogWarning("simpleKCC为空");
                return;
            }
            if (hitboxRoot == null) {
                Debug.LogWarning("hitboxRoot为空");
                return;
            }
            if (_sceneObjects.Gameplay.State == EGameplayState.Finished) {
                // After gameplay is finished we still want the player to finish movement and not stuck in the air.
                // 游戏结束后，我们仍然希望玩家完成移动，而不是卡在空中。
                //Move();
                return;
            }

            if (health.IsAlive == false) {
                // We want dead body to finish movement - fall to ground etc.
                // 我们希望尸体完成移动 - 落到地面等。
                //Move();

                // Disable physics casts and collisions with other players.
                // 禁用物理射线检测和与其他玩家的碰撞。
                simpleKCC.SetColliderLayer(LayerMask.NameToLayer("Ignore Raycast"));
                simpleKCC.SetCollisionLayerMask(LayerMask.GetMask("Default"));

                hitboxRoot.HitboxRootActive = false;

                // Force enable third person visual for local player.
                // 强制为本地玩家启用第三人称视觉。
                SetFirstPersonVisuals(false);
                return;
            }

            if (GetInput(out NetworkedInput input)) {
                // Input is processed on InputAuthority and StateAuthority.
                // 输入在InputAuthority和StateAuthority上处理。
                ProcessInput(input);
            } else {
                // When no input is available, at least continue with movement (e.g. falling).
                // 当没有输入可用时，至少继续移动（例如下落）。
                //Move();
                CameraRotation();
            }
        }
        private void ProcessInput(NetworkedInput input) {
            // Processing input - look rotation, jump, movement, weapon fire, weapon switching, weapon reloading, spray decal.
            // 处理输入 - 视角旋转、跳跃、移动、武器射击、武器切换、武器装弹、喷涂贴花。

            // 存储当前移动输入用于旋转计算
            _currentMoveInput = input.MoveDirection;

            simpleKCC.AddLookRotation(input.LookRotationDelta, -89f, 89f);

            // It feels better when player falls quicker
            // 玩家下落更快时感觉更好
            simpleKCC.SetGravity(simpleKCC.RealVelocity.y >= 0f ? -UpGravity : -DownGravity);

            var inputDirection = simpleKCC.TransformRotation * new Vector3(input.MoveDirection.x, 0f, input.MoveDirection.y);
            var jumpImpulse = 0f;

            if (input.Buttons.WasPressed(_previousButtons, EInputButton.Jump) && simpleKCC.IsGrounded) {
                jumpImpulse = JumpForce;
            }

            //MovePlayer(inputDirection * MoveSpeed, jumpImpulse);
            Move();

            CameraRotation();

            if (simpleKCC.HasJumped) {
                _jumpCount++;
            }

            //if (input.Buttons.IsSet(EInputButton.Fire)) {
            //    bool justPressed = input.Buttons.WasPressed(_previousButtons, EInputButton.Fire);
            //    Weapons.Fire(justPressed);
            //    health.StopImmortality();
            //} else if (input.Buttons.IsSet(EInputButton.Reload)) {
            //    Weapons.Reload();
            //}

            //if (input.Buttons.WasPressed(_previousButtons, EInputButton.Pistol)) {
            //    Weapons.SwitchWeapon(EWeaponType.Pistol);
            //} else if (input.Buttons.WasPressed(_previousButtons, EInputButton.Rifle)) {
            //    Weapons.SwitchWeapon(EWeaponType.Rifle);
            //} else if (input.Buttons.WasPressed(_previousButtons, EInputButton.Knife)) {
            //    Weapons.SwitchWeapon(EWeaponType.Knife);
            //}

            //if (input.Buttons.WasPressed(_previousButtons, EInputButton.Spray) && HasStateAuthority) {
            //    if (Runner.GetPhysicsScene().Raycast(CameraHandle.position, simpleKCC.LookDirection, out var hit, 2.5f, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore)) {
            //        // When spraying on the ground, rotate it so it aligns with player view.
            //        // 在地面喷涂时，旋转它使其与玩家视角对齐。
            //        var sprayOrientation = hit.normal.y > 0.9f ? simpleKCC.TransformRotation : Quaternion.identity;
            //        Runner.Spawn(SprayPrefab, hit.point, sprayOrientation * Quaternion.LookRotation(-hit.normal));
            //    }
            //}

            // Store input buttons when the processing is done - next tick it is compared against current input buttons.
            // 处理完成后存储输入按钮 - 下一帧将其与当前输入按钮进行比较。
            _previousButtons = input.Buttons;
        }
        private void CameraRotation() {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition) {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
                float sensitivity = IsCurrentDeviceMouse ? MouseSensitivity : 1.0f;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier * sensitivity;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier * sensitivity;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }
        private void RefreshCamera() {
            // Camera is set based on KCC look rotation.
            // 相机基于KCC视角旋转设置。
            Vector2 pitchRotation = simpleKCC.GetLookRotation(true, false);
            CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(pitchRotation);
            //CameraRotation();
            // if there is an input and camera position is not fixed
            //if (pitchRotation.sqrMagnitude >= _threshold && !LockCameraPosition) {
            //    //Don't multiply mouse input by Time.deltaTime;
            //    float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
            //    float sensitivity = IsCurrentDeviceMouse ? MouseSensitivity : 1.0f;

            //    _cinemachineTargetYaw += pitchRotation.x * deltaTimeMultiplier * sensitivity;
            //    _cinemachineTargetPitch += pitchRotation.y * deltaTimeMultiplier * sensitivity;
            //}

            //// clamp our rotations so our values are limited 360 degrees
            //_cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            //_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            //// Cinemachine will follow this target
            //CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            //    _cinemachineTargetYaw, 0.0f);
        }
    }
}