using Cinemachine;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThirdpersonGame {

    /// <summary>
    /// Handles weapon shooting mechanics including bullet spawning, spread calculation, and aim control.
    /// Manages fire rate, bullet pool usage, and integrates with weapon manager and aim controller.
    /// 处理武器射击机制，包括子弹生成、散布计算和瞄准控制。
    /// 管理射速、子弹池使用，并与武器管理器和瞄准控制器集成。
    /// </summary>
    public class ShootingController : NetworkBehaviour {
        [Header("References")]
        [SerializeField] private WeaponManager weaponManager;
        [SerializeField] private Camera playerCamera;
        //[SerializeField] private Transform rayPoint;
        [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
        [SerializeField] private float normalSensitivity = 1;
        [SerializeField] private float aimSensitivity = 1;

        private Animator animator;
        private int FireLayerIndex;
        private float fireTimer;
        private StarterAssetsInputs starterAssetsInputs;
        private ThirdPersonController thirdPersonController;
        private ThirdPersonAimController thirdPersonAimController;
        private RigManager rigManager;
        private int aimLayerIndex;
        private WeaponData weapon;

        [Networked] private NetworkButtons _previousButtons { get; set; }

        private void Awake() {
            starterAssetsInputs = GetComponent<StarterAssetsInputs>();
            animator = GetComponent<Animator>();
            rigManager = GetComponent<RigManager>();
            thirdPersonController = GetComponent<ThirdPersonController>();
            thirdPersonAimController = GetComponent<ThirdPersonAimController>();
            FireLayerIndex = animator.GetLayerIndex("FireLayer");
            aimLayerIndex = animator.GetLayerIndex("AimingLayer");
            if (FireLayerIndex == 0) {
                Debug.Log("FireLayerIndex is 0, please check the Animator setup.");
            }
        }

        private void Start() {
            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        public override void FixedUpdateNetwork() {

            // 暂时注释掉，之后要用网络同步实现开火功能

            //base.FixedUpdateNetwork();

            //if (GetInput(out NetworkedInput input)) {
            //    if (input.Buttons.IsSet(EInputButton.Fire)) {
            //        bool justPressed = input.Buttons.WasPressed(_previousButtons, EInputButton.Fire);
            //        Shoot(justPressed);
            //        //Health.StopImmortality();
            //    } else if (input.Buttons.IsSet(EInputButton.Reload)) {
            //        //Reload();
            //    }
            //    _previousButtons = input.Buttons;
            //}


            //fireTimer += Runner.DeltaTime;
            //var weapon = weaponManager.CurrentWeapon;
            //if (starterAssetsInputs.fire && fireTimer >= weapon.fireRate && weapon.ClipAmmo > 0) {
            //    if (GetComponent<ThirdPersonAimController>().GetIsAimming()) {
            //        // 处于开镜状态

            //    } else {
            //        // 腰射状态
            //        EnableAim();
            //    }
            //    animator.SetLayerWeight(FireLayerIndex, 0.7f);
            //    Shoot(starterAssetsInputs.fire);
            //    fireTimer = 0f;
            //} else if (starterAssetsInputs.fire == false) {
            //    animator.SetLayerWeight(FireLayerIndex, 0f);
            //    if (!GetComponent<ThirdPersonAimController>().GetIsAimming()) {
            //        // 腰射状态
            //        DisableAim();
            //    }
            //}

            //if (starterAssetsInputs.reload) {
            //    Reload(starterAssetsInputs.reload);
            //    starterAssetsInputs.reload = false;
            //}
        }
        private void DisableAim() {
            aimVirtualCamera.gameObject.SetActive(false);
            animator.SetLayerWeight(aimLayerIndex, 0);

            // 调整rig组件权重
            rigManager.SetWeaponAimRigWeight(0);
        }

        private void EnableAim() {
            aimVirtualCamera.gameObject.SetActive(true);
            animator.SetLayerWeight(aimLayerIndex, 0.8f);

            // 调整rig组件权重
            rigManager.SetWeaponAimRigWeight(1f);
        }

        private void Shoot(bool justPressed) {

            if (justPressed == false) {
                return;
            }

            if (weaponManager.currentWeaponType == WeaponType.Knife) {
                return;
            } else if (weaponManager.currentWeaponType == WeaponType.Pistol) {
                weapon = weaponManager.currentSecondaryWeapon.GetComponent<WeaponData>();
            } else if (weaponManager.currentWeaponType == WeaponType.Rifle) {
                weapon = weaponManager.currentMainWeapon.GetComponent<WeaponData>();
            }

            if (weapon.isReloading)
                return;
            //if (weapon.ExpiredOrNotRunning(Runner) == false)
            //    return false;

            if (weapon.ClipAmmo <= 0) {
                weapon.PlayEmptyClipSound(justPressed);
                return;
            }

            weapon.ClipAmmo--;

            //GameObject bullet = BulletPoolManager.Instance.GetBullet();
            //if (bullet == null) return;
            //Vector3 targetDirection = CalculateTargetDirection();
            //Vector3 spread = new Vector3(
            //    Random.Range(-weapon.aimedFireSpread, weapon.aimedFireSpread),
            //    Random.Range(-weapon.aimedFireSpread, weapon.aimedFireSpread),
            //    0f
            //);
            //Vector3 directionWithSpread = Quaternion.Euler(spread) * targetDirection;
            //bullet.transform.position = weapon.firePoint.position;
            //bullet.transform.forward = directionWithSpread.normalized;
            //bullet.transform.Rotate(0, 90, 0);
            //Bullet bulletComponent = bullet.GetComponent<Bullet>();
            //if (bulletComponent != null) {
            //    bulletComponent.Initialize(directionWithSpread);
            //}
            //bullet.SetActive(true);
        }

        private void Reload(bool justPressed) {
            if (justPressed == false) {
                return;
            }
            weapon.Reload();
        }

        private Vector3 CalculateTargetDirection() {
            //if (rayPoint == null) {
            //    Debug.LogError("rayPoint未设置，请在Inspector中指定");
            //}
            //return (rayPoint.position - weaponManager.CurrentWeapon.firePoint.position).normalized;
            return (thirdPersonAimController.GetRayPoint() - weaponManager.CurrentWeapon.firePoint.position).normalized;

        }
    }
}