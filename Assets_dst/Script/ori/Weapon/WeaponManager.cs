using Fusion;
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
            ToKnife(); // 初始化时设置为空手状态
        }
        void Update() {
            if (starterAssetsInputs.rifle) {
                ToRifle();
                starterAssetsInputs.rifle = false;
            }
            if (starterAssetsInputs.pistol) {
                ToPistol();
                starterAssetsInputs.pistol = false;
            }
            if (starterAssetsInputs.knife) {
                ToKnife();
                starterAssetsInputs.knife = false;
            }
        }

        public void ToRifle() {
            //Debug.Log("切换到主武器");
            currentWeaponType = WeaponType.Rifle; // 更新当前武器类型
            leftHandIK.weight = 1f; // 启用IK Rig
            starterAssetsInputs.EnableFire(); // 启用fire输入状态
            starterAssetsInputs.EnableAim(); // 启用aim输入状态
            currentSecondaryWeapon.SetActive(false);
            currentMainWeapon.SetActive(true);
            animator.SetBool("Rifle", true);
            if (animator.GetBool("Pistol")) {
                animator.SetBool("Pistol", false);
            }
        }

        public void ToPistol() {
            //Debug.Log("切换到副武器");
            currentWeaponType = WeaponType.Pistol; // 更新当前武器类型
            leftHandIK.weight = 0;
            starterAssetsInputs.EnableFire(); // 启用fire输入状态
            starterAssetsInputs.EnableAim(); // 启用aim输入状态
            currentMainWeapon.SetActive(false);
            currentSecondaryWeapon.SetActive(true);
            animator.SetBool("Pistol", true);
            if (animator.GetBool("Rifle")) {
                animator.SetBool("Rifle", false);
            }
        }

        public void ToKnife() {
            //Debug.Log("切换到空手");
            currentWeaponType = WeaponType.Knife; // 更新当前武器类型
            leftHandIK.weight = 0;
            starterAssetsInputs.DisableFire();
            starterAssetsInputs.DisableAim();
            currentMainWeapon.SetActive(false);
            currentSecondaryWeapon.SetActive(false);
            animator.SetBool("Rifle", false);
            animator.SetBool("Pistol", false);
        }

        public void SwitchWeapon(int index) {
            if (index >= 0 && index < weapons.Length) {
                currentWeaponIndex = index;
                // 输出调试信息
                var weapon = weapons[index];
                Debug.Log($"切换武器：{weapon.weaponName}，类型：{weapon.weaponType}");
                // 可加：切换武器动画、UI提示等
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