using Cinemachine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using ThirdpersonGame;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.XR;
#endif

namespace ThirdpersonGame {
    public class ThirdPersonController : NetworkBehaviour, IDamageable {
        // Fusion联网组件需要的引用
        public SimpleKCC simpleKCC; // Kinematic Character Controller，运动学角色控制器
        //public HitboxRoot hitboxRoot;
        public WeaponManager weaponManager; // public给UI更新访问

        // 网络同步参数
        [Networked] private NetworkButtons _previousButtons { get; set; }
        [Networked] private NetworkBool _isJumping { get; set; }
        [Networked] private Vector3 _moveVelocity { get; set; }
        [Networked] private float ClimbSpeed { get; set; } // 控制爬梯子速度和动画播放速度
        [Networked] private NetworkBool IsClimbingLadder { get; set; } // 控制爬梯子动画
        [Networked] private NetworkBool IsCrouch { get; set; } // 玩家是否处于蹲伏状态
        [Networked] public NetworkBool IsFire { get; set; } // 玩家是否处于开火状态
        [Networked] public int CurrHealth { get; set; } // 玩家生命值

        [Header("Health")]
        public int MaxHealth = 100;

        [Header("Camera")]
        public GameObject VirtualCameraRoot; // 虚拟摄像机根对象
        public Transform CameraPovit;
        public Transform CameraHandle;
        public CinemachineVirtualCamera fireCamera; // 开火时切换的摄像机，通过标签获取
        public CinemachineVirtualCamera playerFllowCamera;

        [Header("Fire")]
        public Transform rayPoint; // 用于更新武器rig和上半身rig
        public LayerMask mouseColliderLayerMask = new LayerMask();

        [Header("速度参数设置")]
        public float MoveSpeed = 2.0f;
        public float AimmingMoveSpeed;
        public float CrouchMoveSpeed;
        public float ClimbLadderSpeed;
        public float RotationSpeed = 8f;
        public float GroundAcceleration = 55f;
        public float GroundDeceleration = 25f;
        public float AirAcceleration = 25f;
        public float AirDeceleration = 1.3f;
        public float SprintSpeed = 5.335f;
        public float AimmingSprintSpeed;
        public float CrouchSprintSpeed;

        [Header("重力&跳跃冲量")]
        public float UpGravity;
        public float DownGravity;
        public float JumpImpulse;

        [Header("脚步音频与设置")]
        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Header("Ladder Climbing")]
        public bool enableRotation = true;
        private Vector3 lastGrabLadderDirection;
        private bool hitSomething = false; // 在fixupdate中使用射线检查面前是否有物体
        private bool hitSomething2 = false; // 使用lastGrabLadderDirection检测
        private bool hitLadder = false;
        private bool hitLadder2 = false;

        // Animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDClimbSpeed;
        private int _animIDClimb;
        private int _animIDCrouch;
        private int _animIDFire;

        // References
        private Animator _animator;
        private StarterAssetsInputs _input;
        private RigManager _rigManager;
        private bool _hasAnimator;

        #region Unity Lifecycle Methods

        private void Awake() {
            weaponManager = GetComponent<WeaponManager>();
            _rigManager = GetComponent<RigManager>();
            _hasAnimator = TryGetComponent(out _animator);
            _input = GetComponent<StarterAssetsInputs>();
        }

        public override void Spawned() {
            if (HasStateAuthority) {
                CurrHealth = MaxHealth;
            }

            // 禁用非本地玩家的摄像机
            if (HasInputAuthority == false) {
                VirtualCameraRoot.SetActive(false);
            }
        }

        private void Start() {
            AssignAnimationIDs();
        }

        public override void FixedUpdateNetwork() {
            // 如果Awake中没有获取到animator，则每次都尝试获取一次
            if (!_hasAnimator)
                _hasAnimator = TryGetComponent(out _animator);

            if (GetInput(out NetworkedInput input)) {
                // Input is processed on InputAuthority and StateAuthority.
                ProcessInput(input);
            } else {
                // When no input is available, at least continue with movement (e.g. falling).
                Debug.Log("无法获得输入");
                Move();
            }

            if (simpleKCC.IsGrounded) {
                // Stop jumping
                _isJumping = false;
            }
        }

        private void LateUpdate() {
            // LateUpdate：在所有Update函数调用之后调用。适合处理相机跟随等需要在Update之后进行的操作。
            if (HasInputAuthority == false)
                return;

            CameraPovit.rotation = Quaternion.Euler(_input.lookRotation);
            Camera.main.transform.SetLocalPositionAndRotation(CameraHandle.position, CameraHandle.rotation);
        }

        #endregion

        #region Public Methods

        #endregion

        #region Animation & Audio

        // Fusion建议使用Render方法来替换Unity的Update方法来处理动画和视觉效果
        public override void Render() {
            base.Render();


            // 只有本地玩家更新视觉
            if (HasInputAuthority) {
                UpdateMouseWorldPosition();
                UpdateRigWeight();
            }

            _animator.SetFloat(_animIDSpeed, simpleKCC.RealSpeed, 0.15f, Time.deltaTime);
            _animator.SetFloat(_animIDMotionSpeed, 1f);
            _animator.SetBool(_animIDJump, _isJumping);
            _animator.SetBool(_animIDGrounded, simpleKCC.IsGrounded);
            _animator.SetBool(_animIDFreeFall, simpleKCC.RealVelocity.y < -10f);
            _animator.SetFloat(_animIDClimbSpeed, ClimbSpeed);
            _animator.SetBool(_animIDClimb, IsClimbingLadder);
            _animator.SetBool(_animIDCrouch, IsCrouch);
            _animator.SetBool(_animIDFire, IsFire);
        }

        private void AssignAnimationIDs() {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDClimbSpeed = Animator.StringToHash("ClimbSpeed");
            _animIDClimb = Animator.StringToHash("Climb");
            _animIDCrouch = Animator.StringToHash("Crouch");
            _animIDFire = Animator.StringToHash("Fire");
        }

        private void OnFootstep(AnimationEvent animationEvent) {
            if (animationEvent.animatorClipInfo.weight > 0.5f) {
                if (FootstepAudioClips.Length > 0) {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(simpleKCC.Position), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent) {
            if (animationEvent.animatorClipInfo.weight > 0.5f) {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(simpleKCC.Position), FootstepAudioVolume);
            }
            _rigManager.SetLeftHandGridRig(1f);
        }

        private void OnAir() {
            // 在空中时，包括跳跃和自由落体状态
            _rigManager.SetLeftHandGridRig(0f);
        }

        private void UpdateRigWeight() {

            /*
             rig 权重控制逻辑说明：
                射击时使用武器IK，否则不使用
                持枪时使用头部IK，否则不使用
                跳跃时关掉手部IK，否则其他情况正常

                上半身IK效果不好，暂时不用
             */

            if (IsFire) {
                _rigManager.SetWeaponAimRigWeight(1f);
            } else {
                _rigManager.SetWeaponAimRigWeight(0f);
            }

            if (weaponManager.currentWeaponType == WeaponType.Knife) {
                _rigManager.SetHeadAimRigWeight(0f);
            } else {
                _rigManager.SetHeadAimRigWeight(0.7f);
            }

            if (_isJumping) {
                _rigManager.SetLeftHandGridRig(0f);
            } else {
                _rigManager.SetLeftHandGridRig(1f);
            }
        }

        #endregion

        #region Movement

        private void Move(Vector3 desiredMoveVelocity = default, float jumpImpulse = default) {
            // 只有在不爬梯子时才应用重力
            if (IsClimbingLadder == false)
                simpleKCC.SetGravity(simpleKCC.RealVelocity.y >= 0f ? UpGravity : DownGravity);


            float acceleration;
            if (desiredMoveVelocity != Vector3.zero) {
                // 爬梯子和开火状态下不旋转角色
                if (IsClimbingLadder == false && IsFire == false) {
                    var currentRotation = simpleKCC.TransformRotation;
                    var targetRotation = Quaternion.LookRotation(desiredMoveVelocity);
                    var nextRotation = Quaternion.Lerp(currentRotation, targetRotation, RotationSpeed * Runner.DeltaTime);
                    simpleKCC.SetLookRotation(nextRotation.eulerAngles);
                }
                acceleration = simpleKCC.IsGrounded ? GroundAcceleration : AirAcceleration;
            } else {
                acceleration = simpleKCC.IsGrounded ? GroundDeceleration : AirDeceleration;
            }

            _moveVelocity = Vector3.Lerp(_moveVelocity, desiredMoveVelocity, acceleration * Runner.DeltaTime);

            // 在爬梯子状态下，不进行地面投影，也不使用插值
            if (IsClimbingLadder) {
                // 直接设置速度，不使用插值
                _moveVelocity = desiredMoveVelocity;
            } else if (simpleKCC.ProjectOnGround(_moveVelocity, out var projectedVector)) {
                // 确保一致的运动速度，即使在陡坡上（不适用于梯子）
                _moveVelocity = projectedVector;
            }

            simpleKCC.Move(_moveVelocity, jumpImpulse);
        }

        #endregion

        #region Ladder Climbing

        // 使用射线检测前方物体，爬梯子逻辑辅组函数
        private void CheckForwardObject() {
            Vector3 targetDirection = Vector3.forward;

            float avoidFloorDistance = .1f; // 射线发射高度
            float ladderGrabDistance = .4f; // 射线长度

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
        }

        private void StartClimbingLadder() {
            // 切换到空手
            weaponManager.ToKnife();

            // 记录梯子方向
            lastGrabLadderDirection = transform.forward;

            // 禁用转身，启用爬梯状态
            enableRotation = false;
            IsClimbingLadder = true;
            ClimbSpeed = 0f;

            // 重置KCC速度，确保能开始垂直移动
            simpleKCC.ResetVelocity();

            // 调整角色位置贴近梯子
            SnapToLadderPosition();
        }

        private void DropLadder() {
            enableRotation = true;
            IsClimbingLadder = false;
            ClimbSpeed = 0f;

            // 恢复重力
            simpleKCC.SetGravity(simpleKCC.RealVelocity.y >= 0f ? UpGravity : DownGravity);
        }

        private void ProcessLadderMovement(NetworkedInput input, ref Vector3 desiredMoveVelocity, ref float jumpImpulse) {
            // 爬梯子时完全禁用重力
            simpleKCC.SetGravity(0f);

            // 使用垂直输入控制爬梯速度
            float verticalInput = input.MoveDirection.y;
            ClimbSpeed = verticalInput * ClimbLadderSpeed;

            // 设置垂直移动速度
            desiredMoveVelocity = new Vector3(0f, ClimbSpeed, 0f);

            // 在爬梯子状态下，我们需要特殊处理地面状态
            if (simpleKCC.IsGrounded && ClimbSpeed > 0) {
                // 当在地面且想向上爬时，我们需要给一个小跳跃脉冲来离开地面
                // 这样KCC就不会忽略垂直速度了
                jumpImpulse = 0.1f; // 很小的跳跃力，只是为了离开地面
            }

            // 跳跃键用于离开梯子
            if (input.Buttons.WasPressed(_previousButtons, EInputButton.Jump)) {
                DropLadder();
                jumpImpulse = JumpImpulse * 0.3f;
                return;
            }

            // 检查是否应该离开梯子
            CheckLadderExitConditions();
        }

        private void CheckLadderExitConditions() {
            // 检查头顶是否有障碍（到达梯子顶部）
            if (ClimbSpeed > 0 && Physics.Raycast(transform.position + Vector3.up * 1.8f, Vector3.up, 0.3f)) {
                DropLadder();
                return;
            }

            // 检查是否仍然在梯子范围内
            if (!hitLadder2) {
                DropLadder();
                return;
            }
        }

        private void SnapToLadderPosition() {
            // 将角色位置调整到梯子前方合适的位置
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, lastGrabLadderDirection,
                out hit, 1f)) {
                if (hit.transform.TryGetComponent(out Ladder ladder)) {
                    // 调整位置，确保角色贴近梯子但不会嵌入
                    Vector3 targetPosition = hit.point - lastGrabLadderDirection * 0.3f;
                    targetPosition.y = transform.position.y;
                    transform.position = targetPosition;

                    // 面向梯子
                    transform.rotation = Quaternion.LookRotation(-lastGrabLadderDirection);
                }
            }
        }

        #endregion

        #region Fire

        private void Fire() {
            IsFire = true;

            // 切换到开火摄像机
            fireCamera.gameObject.SetActive(true);

            // 通知武器开始射击
            weaponManager.GetCurrentWeapon().GetComponent<WeaponData>().StartFire();
        }

        private void OnFireStop() {
            if (IsFire == false) {
                return;
            }
            fireCamera.gameObject.SetActive(false);
            //rayPoint.gameObject.SetActive(false);
            IsFire = false;
            // 通知武器停止射击
            weaponManager.GetCurrentWeapon().GetComponent<WeaponData>().StopFire();
        }

        private void UpdateMouseWorldPosition() {
            //计算mouseWorldPosition位置
            //rayPoint.gameObject.SetActive(true);  // 持枪状态下都要计算mouseWorldPosition位置
            //mouseWorldPosition = Vector3.zero;
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);

            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, mouseColliderLayerMask)) {
                rayPoint.position = raycastHit.point;
                //mouseWorldPosition = raycastHit.point;
                //if (IsFire) {
                //    // 处理子弹命中
                //    //Debug.Log($"子弹命中{raycastHit.collider.name}");
                //    if (raycastHit.collider.TryGetComponent(out IDamageable damageable)) {
                //        damageable.TakeDamage();
                //    }
                //}
            }
        }

        #endregion

        #region Input Processing

        private void ProcessInput(NetworkedInput input) {
            // 玩家跳跃 
            var jumpImpulse = 0f;

            if (simpleKCC.IsGrounded) {
                if (input.Buttons.WasPressed(_previousButtons, EInputButton.Jump)) {
                    jumpImpulse = JumpImpulse;
                    _isJumping = true;
                }
            }

            // 玩家蹲下
            if (input.Buttons.WasPressed(_previousButtons, EInputButton.Crouch)) {
                IsCrouch = !IsCrouch;
            }

            // 射击逻辑
            if (weaponManager.currentWeaponType != WeaponType.Knife) {
                //Debug.Log($"{name}更新瞄准位置...");
                //UpdateMouseWorldPosition();

                // 射击
                if (input.Buttons.WasPressed(_previousButtons, EInputButton.Fire)) {
                    Fire();
                }
                if (input.Buttons.WasReleased(_previousButtons, EInputButton.Fire)) {
                    OnFireStop();
                }
            }

            // 开火时玩家跟随视角旋转
            if (IsFire) {
                simpleKCC.SetLookRotation(input.Look, -89f, 89f);


            }

            // 玩家移动
            float speed = input.Buttons.IsSet(EInputButton.Sprint) ? SprintSpeed : MoveSpeed;

            // 基于输入和相机方向计算移动方向
            var lookRotation = Quaternion.Euler(0f, input.Look.y, 0f);
            var moveDirection = lookRotation * new Vector3(input.MoveDirection.x, 0, input.MoveDirection.y);
            Vector3 desiredMoveVelocity = Vector3.zero;

            // 爬梯子逻辑
            CheckForwardObject();

            // 如果在爬梯子状态，优先处理梯子移动
            if (IsClimbingLadder) {
                ProcessLadderMovement(input, ref desiredMoveVelocity, ref jumpImpulse);
            } else {
                // 正常移动
                desiredMoveVelocity = moveDirection * speed;

                // 检查是否可以开始爬梯子（面前有梯子且向前移动）
                if (hitLadder && input.MoveDirection.y > 0.1f) {
                    StartClimbingLadder();
                }
            }

            Move(desiredMoveVelocity, jumpImpulse);

            _previousButtons = input.Buttons;
        }

        #endregion

        #region 受击处理
        public void TakeDamage() {
            CurrHealth -= 1;
        }
        #endregion

    }
}