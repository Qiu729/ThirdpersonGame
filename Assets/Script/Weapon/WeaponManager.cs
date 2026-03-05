using Fusion;
using Fusion.Addons.SimpleKCC;
using ThirdpersonGame;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;


namespace ThirdpersonGame {
    /// <summary>
    /// Manages weapon switching, activation, and IK rig control for the player.
    /// Handles switching between rifle, pistol, and knife with proper animation and input management.
    /// 管理玩家的武器切换、激活和IK rig控制。
    /// 处理步枪、手枪和匕首之间的切换，包括适当的动画和输入管理。
    /// </summary>
    public class WeaponManager : NetworkBehaviour {
        private StarterAssetsInputs starterAssetsInputs;
        private Animator animator;

        [Header("当前武器")]
        public GameObject currentMainWeapon;
        public GameObject currentSecondaryWeapon;
        public WeaponType currentWeaponType = WeaponType.Knife; // 当前武器类型
        [Header("武器数据")]
        public WeaponData[] weapons;
        public int currentWeaponIndex = 0;
        public WeaponData CurrentWeapon => weapons[currentWeaponIndex];
        [Header("IK Rig")]
        public Rig leftHandIK; // 左手IK Rig，用于手持武器时启用IK

        [Networked] private NetworkButtons _previousButtons { get; set; }
        [Networked] private NetworkBool IsRifle { get; set; } // 步枪动画参数同步
        [Networked] private NetworkBool IsPistol { get; set; } // 手枪动画参数同步
        [Networked] private NetworkBool currentMainWeaponActive { get; set; } // 当前主武器激活状态同步
        [Networked] private NetworkBool currentSecondaryWeaponActive { get; set; } // 当前副武器激活状态同步

        #region UnityLifecycle
        private void Awake() {
            starterAssetsInputs = GetComponent<StarterAssetsInputs>();
            animator = GetComponent<Animator>();
            if (currentMainWeapon != null) {
                currentMainWeapon.SetActive(false);
            } else {
                Debug.LogError("当前主武器未设置，请在Inspector中设置currentMainWeapon");
            }
            if (currentSecondaryWeapon != null) {
                currentSecondaryWeapon.SetActive(false);
            } else {
                Debug.LogError("当前副武器未设置，请在Inspector中设置currentSecondaryWeapon");
            }
        }

        private void Start() {
            ToKnife(); // 初始化时设置为空手状态
        }
        void Update() {

            // 暂时注释掉，之后要用网络同步实现武器切换功能

            //if (starterAssetsInputs.rifle) {
            //    ToRifle();
            //    starterAssetsInputs.rifle = false;
            //}
            //if (starterAssetsInputs.pistol) {
            //    ToPistol();
            //    starterAssetsInputs.pistol = false;
            //}
            //if (starterAssetsInputs.knife) {
            //    ToKnife();
            //    starterAssetsInputs.knife = false;
            //}
        }
        #endregion

        public override void Render() {
            base.Render();
            animator.SetBool("Pistol", IsPistol);
            animator.SetBool("Rifle", IsRifle);
            currentMainWeapon.SetActive(currentMainWeaponActive);
            currentSecondaryWeapon.SetActive(currentSecondaryWeaponActive);
        }
        public void ToRifle() {
            //Debug.Log("切换到主武器");
            currentWeaponType = WeaponType.Rifle; // 更新当前武器类型
            leftHandIK.weight = 1f; // 启用IK Rig
            //starterAssetsInputs.EnableFire(); // 启用fire输入状态
            //starterAssetsInputs.EnableAim(); // 启用aim输入状态
            currentMainWeaponActive = true;
            currentSecondaryWeaponActive = false;
            IsRifle = true;
            if (IsPistol) {
                IsPistol = false;
            }
        }

        public void ToPistol() {
            //Debug.Log("切换到副武器");
            currentWeaponType = WeaponType.Pistol; // 更新当前武器类型
            leftHandIK.weight = 0;
            //starterAssetsInputs.EnableFire(); // 启用fire输入状态
            //starterAssetsInputs.EnableAim(); // 启用aim输入状态
            currentMainWeaponActive = false;
            currentSecondaryWeaponActive = true;
            IsPistol = true;
            if (IsRifle) {
                IsRifle = false;
            }
        }

        public void ToKnife() {
            //Debug.Log("切换到空手");
            currentWeaponType = WeaponType.Knife; // 更新当前武器类型
            leftHandIK.weight = 0;
            //starterAssetsInputs.DisableFire();
            //starterAssetsInputs.DisableAim();
            currentMainWeaponActive = false;
            currentSecondaryWeaponActive = false;
            IsRifle = false;
            IsPistol = false;
        }

        public void SwitchWeapon(int index) {
            if (index >= 0 && index < weapons.Length) {
                currentWeaponIndex = index;
                // 输出调试信息
                var weapon = weapons[index];
                if (weapon.weaponType == WeaponType.Rifle) {
                    currentMainWeaponActive = true;
                    currentMainWeapon = weapon.gameObject;
                    currentSecondaryWeaponActive = false;
                } else if (weapon.weaponType == WeaponType.Pistol) {
                    currentMainWeaponActive = false;
                    currentSecondaryWeapon = weapon.gameObject;
                    currentSecondaryWeaponActive = true;
                } else if (weapon.weaponType == WeaponType.Knife) {
                }
                Debug.Log($"切换武器：{weapon.weaponName}，类型：{weapon.weaponType}");
                // 可加：切换武器动画、UI提示等
            }
        }

        // 获取当前武器数据
        public WeaponData GetCurrentWeapon() {
            if (currentWeaponType == WeaponType.Rifle) {
                return currentMainWeapon.GetComponent<WeaponData>();
            } else if (currentWeaponType == WeaponType.Pistol) {
                return currentSecondaryWeapon.GetComponent<WeaponData>();
            } else {
                return null; // 匕首没有WeaponData
            }
        }

        public override void FixedUpdateNetwork() {
            base.FixedUpdateNetwork();

            if (GetInput(out NetworkedInput input)) {
                if (input.Buttons.WasPressed(_previousButtons, EInputButton.Pistol)) {
                    ToPistol();
                } else if (input.Buttons.WasPressed(_previousButtons, EInputButton.Rifle)) {
                    ToRifle();
                } else if (input.Buttons.WasPressed(_previousButtons, EInputButton.Knife)) {
                    ToKnife();
                }
                _previousButtons = input.Buttons;
            }
        }

    }
}