
using Fusion;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;

namespace ThirdpersonGame {

    /// <summary>
    /// UI panel for weapon selection and purchasing in the game.
    /// Manages weapon shop interface, cursor control, and player controller state during shop interaction.
    /// 游戏中武器选择和购买的UI面板。
    /// 管理武器商店界面、光标控制和商店交互期间的玩家控制器状态。
    /// </summary>
    public class WeaponShopPanel : MonoBehaviour {

        public Button[] weaponButtons;

        private ThirdPersonAimController aimController;
        private ThirdPersonController thirdPersonController;
        private ShootingController shootingController;
        private WeaponManager weaponManager;

        void Start() {
            // gameObject.SetActive(false); // 初始隐藏
            for (int i = 0; i < weaponButtons.Length; i++) {
                int index = i;
                weaponButtons[i].onClick.AddListener(() => OnWeaponButtonClicked(index));
            }
        }

        private void Update() {
        }

        public void ToggleActive(ThirdPersonController player) {
            if (Application.isBatchMode == true)
                return;
            if (aimController == null || thirdPersonController == null || shootingController == null || weaponManager == null) {
                aimController = player.GetComponent<ThirdPersonAimController>();
                shootingController = player.GetComponent<ShootingController>();
                weaponManager = player.GetComponent<WeaponManager>();
            }

            bool toActive = !gameObject.activeSelf;
            gameObject.SetActive(toActive);
            if (toActive) {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                // 禁用射击控制器，此处不能禁用ThirdPersonController，不然会重置玩家位置
                aimController.enabled = false;
                shootingController.enabled = false;
            } else {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                aimController.enabled = true;
                shootingController.enabled = true;
            }
        }

        void OnWeaponButtonClicked(int weaponIndex) {
            if (weaponManager == null) return;

            weaponManager.SwitchWeapon(weaponIndex);

        }
    }
}