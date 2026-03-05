
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThirdpersonGame {
    /// <summary>
    /// Handles input for opening and closing the weapon shop panel.
    /// Manages the shop panel toggle functionality based on player input.
    /// 处理打开和关闭武器商店面板的输入。
    /// 基于玩家输入管理商店面板的切换功能。
    /// </summary>
    public class WeaponShopPanelInput : MonoBehaviour {
        public WeaponShopPanel weaponShopPanel;
        private StarterAssetsInputs starterAssetsInputs;
        private bool lastShopPanelInput = false;

        void Awake() {
            starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        }

        void Update() {
            if (starterAssetsInputs.shopPanel) {
                weaponShopPanel.ToggleActive();
                starterAssetsInputs.shopPanel = false; // 重置输入状态，防止重复触发
            }
        }
    }
}