
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

namespace ThirdpersonGame {
    /// <summary>
    /// Handles input for opening and closing the weapon shop panel.
    /// Manages the shop panel toggle functionality based on player input.
    /// 处理打开和关闭武器商店面板的输入。
    /// 基于玩家输入管理商店面板的切换功能。
    /// </summary>
    public class WeaponShopPanelInput : MonoBehaviour {
        //public Gameplay gameplay;
        //[HideInInspector]
        //public NetworkRunner Runner;

        public WeaponShopPanel weaponShopPanel;
        private StarterAssetsInputs starterAssetsInputs;
        private bool lastShopPanelInput = false;
        public void UpdateShopPanel(ThirdPersonController player) {

            // 暂时注释掉，之后要用网络同步实现商店面板的打开功能

            //if (starterAssetsInputs == null) {
            //    starterAssetsInputs = player.GetComponent<StarterAssetsInputs>();
            //}
            //if (starterAssetsInputs.shopPanel) {
            //    weaponShopPanel.ToggleActive(player);
            //    starterAssetsInputs.shopPanel = false; // 重置输入状态，防止重复触发
            //}
        }
    }
}