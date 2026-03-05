using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Cinemachine;
using ThirdpersonGame;

namespace ThirdpersonGame {
    /// <summary>
    /// Main player script which handles input processing, visuals.
    /// 主要玩家脚本，处理输入处理和视觉效果。
    /// </summary>
    public class Player : NetworkBehaviour {
        [Header("Components")] // 组件
        public SimpleKCC KCC;
        public Weapons Weapons;
        public Health Health;
        public Animator Animator;
        public HitboxRoot HitboxRoot;

        [Header("Setup")] // 设置
        public float MoveSpeed = 6f;
        public float JumpForce = 10f;
        public AudioSource JumpSound;
        public AudioClip[] JumpClips;
        public Transform CameraHandle;
        public GameObject FirstPersonRoot;
        public GameObject ThirdPersonRoot;
        public NetworkObject SprayPrefab;

        [Header("Movement")] // 移动
        public float UpGravity = 15f;
        public float DownGravity = 25f;
        public float GroundAcceleration = 55f;
        public float GroundDeceleration = 25f;
        public float AirAcceleration = 25f;
        public float AirDeceleration = 1.3f;

        [Networked]
        private NetworkButtons _previousButtons { get; set; }
        [Networked]
        private int _jumpCount { get; set; }
        [Networked]
        private Vector3 _moveVelocity { get; set; }

        private int _visibleJumpCount;

        private SceneObjects _sceneObjects;

        public void PlayFireEffect() {
            // Player fire animation (hands) is not played when strafing because we lack a proper
            // animation and we do not want to make the animation controller more complex
            // 玩家射击动画（手部）在侧移时不播放，因为我们缺少合适的动画，也不想让动画控制器更复杂
            if (Mathf.Abs(GetAnimationMoveVelocity().x) > 0.2f)
                return;

            Animator.SetTrigger("Fire");
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

        public override void FixedUpdateNetwork() {
            if (_sceneObjects.Gameplay.State == EGameplayState.Finished) {
                // After gameplay is finished we still want the player to finish movement and not stuck in the air.
                // 游戏结束后，我们仍然希望玩家完成移动，而不是卡在空中。
                MovePlayer();
                return;
            }

            if (Health.IsAlive == false) {
                // We want dead body to finish movement - fall to ground etc.
                // 我们希望尸体完成移动 - 落到地面等。
                MovePlayer();

                // Disable physics casts and collisions with other players.
                // 禁用物理射线检测和与其他玩家的碰撞。
                KCC.SetColliderLayer(LayerMask.NameToLayer("Ignore Raycast"));
                KCC.SetCollisionLayerMask(LayerMask.GetMask("Default"));

                HitboxRoot.HitboxRootActive = false;

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
                MovePlayer();
                RefreshCamera();
            }
        }

        public override void Render() {
            if (_sceneObjects.Gameplay.State == EGameplayState.Finished)
                return;

            var moveVelocity = GetAnimationMoveVelocity();

            // Set animation parameters.
            // 设置动画参数。
            Animator.SetFloat(AnimatorId.LocomotionTime, Time.time * 2f);
            Animator.SetBool(AnimatorId.IsAlive, Health.IsAlive);
            Animator.SetBool(AnimatorId.IsGrounded, KCC.IsGrounded);
            Animator.SetBool(AnimatorId.IsReloading, Weapons.CurrentWeapon.IsReloading);
            Animator.SetFloat(AnimatorId.MoveX, moveVelocity.x, 0.05f, Time.deltaTime);
            Animator.SetFloat(AnimatorId.MoveZ, moveVelocity.z, 0.05f, Time.deltaTime);
            Animator.SetFloat(AnimatorId.MoveSpeed, moveVelocity.magnitude);
            Animator.SetFloat(AnimatorId.Look, -KCC.GetLookRotation(true, false).x / 90f);

            if (Health.IsAlive == false) {
                // Disable UpperBody (override) and Look (additive) layers. Death animation is full-body.
                // 禁用UpperBody（覆盖）和Look（叠加）层。死亡动画是全身的。

                int upperBodyLayerIndex = Animator.GetLayerIndex("UpperBody");
                Animator.SetLayerWeight(upperBodyLayerIndex, Mathf.Max(0f, Animator.GetLayerWeight(upperBodyLayerIndex) - Time.deltaTime));

                int lookLayerIndex = Animator.GetLayerIndex("Look");
                Animator.SetLayerWeight(lookLayerIndex, Mathf.Max(0f, Animator.GetLayerWeight(lookLayerIndex) - Time.deltaTime));
            }

            if (_visibleJumpCount < _jumpCount) {
                Animator.SetTrigger("Jump");

                JumpSound.clip = JumpClips[Random.Range(0, JumpClips.Length)];
                JumpSound.Play();
            }

            _visibleJumpCount = _jumpCount;
        }

        private void LateUpdate() {
            if (HasInputAuthority == false)
                return;

            RefreshCamera();
        }

        private void ProcessInput(NetworkedInput input) {
            // Processing input - look rotation, jump, movement, weapon fire, weapon switching, weapon reloading, spray decal.
            // 处理输入 - 视角旋转、跳跃、移动、武器射击、武器切换、武器装弹、喷涂贴花。

            KCC.AddLookRotation(input.LookRotationDelta, -89f, 89f);

            // It feels better when player falls quicker
            // 玩家下落更快时感觉更好
            KCC.SetGravity(KCC.RealVelocity.y >= 0f ? -UpGravity : -DownGravity);

            var inputDirection = KCC.TransformRotation * new Vector3(input.MoveDirection.x, 0f, input.MoveDirection.y);
            var jumpImpulse = 0f;

            if (input.Buttons.WasPressed(_previousButtons, EInputButton.Jump) && KCC.IsGrounded) {
                jumpImpulse = JumpForce;
            }

            MovePlayer(inputDirection * MoveSpeed, jumpImpulse);
            RefreshCamera();

            if (KCC.HasJumped) {
                _jumpCount++;
            }

            if (input.Buttons.IsSet(EInputButton.Fire)) {
                bool justPressed = input.Buttons.WasPressed(_previousButtons, EInputButton.Fire);
                Weapons.Fire(justPressed);
                Health.StopImmortality();
            } else if (input.Buttons.IsSet(EInputButton.Reload)) {
                Weapons.Reload();
            }

            if (input.Buttons.WasPressed(_previousButtons, EInputButton.Pistol)) {
                Weapons.SwitchWeapon(EWeaponType.Pistol);
            } else if (input.Buttons.WasPressed(_previousButtons, EInputButton.Rifle)) {
                Weapons.SwitchWeapon(EWeaponType.Rifle);
            } else if (input.Buttons.WasPressed(_previousButtons, EInputButton.Knife)) {
                Weapons.SwitchWeapon(EWeaponType.Knife);
            }

            //if (input.Buttons.WasPressed(_previousButtons, EInputButton.Spray) && HasStateAuthority) {
            //    if (Runner.GetPhysicsScene().Raycast(CameraHandle.position, KCC.LookDirection, out var hit, 2.5f, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore)) {
            //        // When spraying on the ground, rotate it so it aligns with player view.
            //        // 在地面喷涂时，旋转它使其与玩家视角对齐。
            //        var sprayOrientation = hit.normal.y > 0.9f ? KCC.TransformRotation : Quaternion.identity;
            //        Runner.Spawn(SprayPrefab, hit.point, sprayOrientation * Quaternion.LookRotation(-hit.normal));
            //    }
            //}

            // Store input buttons when the processing is done - next tick it is compared against current input buttons.
            // 处理完成后存储输入按钮 - 下一帧将其与当前输入按钮进行比较。
            _previousButtons = input.Buttons;
        }

        private void MovePlayer(Vector3 desiredMoveVelocity = default, float jumpImpulse = default) {
            float acceleration = 1f;

            if (desiredMoveVelocity == Vector3.zero) {
                // No desired move velocity - we are stopping.
                // 没有期望的移动速度 - 我们正在停止。
                acceleration = KCC.IsGrounded == true ? GroundDeceleration : AirDeceleration;
            } else {
                acceleration = KCC.IsGrounded == true ? GroundAcceleration : AirAcceleration;
            }

            _moveVelocity = Vector3.Lerp(_moveVelocity, desiredMoveVelocity, acceleration * Runner.DeltaTime);
            KCC.Move(_moveVelocity, jumpImpulse);
        }

        private void RefreshCamera() {
            // Camera is set based on KCC look rotation.
            // 相机基于KCC视角旋转设置。
            Vector2 pitchRotation = KCC.GetLookRotation(true, false);
            CameraHandle.localRotation = Quaternion.Euler(pitchRotation);
        }

        private void SetFirstPersonVisuals(bool firstPerson) {
            FirstPersonRoot.SetActive(firstPerson);
            ThirdPersonRoot.SetActive(firstPerson == false);

            Weapons.SetFirstPersonVisuals(firstPerson);
        }

        private Vector3 GetAnimationMoveVelocity() {
            if (KCC.RealSpeed < 0.01f)
                return default;

            var velocity = KCC.RealVelocity;

            // We only care about X an Z directions.
            // 我们只关心X和Z方向。
            velocity.y = 0f;

            if (velocity.sqrMagnitude > 1f) {
                velocity.Normalize();
            }

            // Transform velocity vector to local space.
            // 将速度向量转换到局部空间。
            return transform.InverseTransformVector(velocity);
        }
    }
}
