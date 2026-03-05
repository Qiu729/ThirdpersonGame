
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
        public WeaponManager weaponManager;
        public Button[] weaponButtons;
        public ThirdPersonController thirdPersonController;
        public ShootingController shootingController;
        public ThirdPersonAimController thirdPersonAimController;

        void Start() {
            // gameObject.SetActive(false); // 初始隐藏
            for (int i = 0; i < weaponButtons.Length; i++) {
                int index = i;
                weaponButtons[i].onClick.AddListener(() => OnWeaponButtonClicked(index));
            }
        }

        public void ToggleActive() {
            bool toActive = !gameObject.activeSelf;
            gameObject.SetActive(toActive);
            if (toActive) {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                // 禁用角色控制器和射击控制器
                thirdPersonController.enabled = false;
                shootingController.enabled = false;
                thirdPersonAimController.enabled = false;
            } else {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                thirdPersonController.enabled = true;
                shootingController.enabled = true;
                thirdPersonAimController.enabled = true;
            }
        }

        void OnWeaponButtonClicked(int weaponIndex) {
            weaponManager.SwitchWeapon(weaponIndex);
            // gameObject.SetActive(false); // 选完自动关闭
        }
    }
}