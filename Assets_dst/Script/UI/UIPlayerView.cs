using TMPro;
using UnityEngine;

namespace ThirdpersonGame {
    /// <summary>
    /// Main player UI view container that displays player-specific information during gameplay.
    /// Manages health display, weapon information, crosshair, and player nickname.
    /// 主玩家UI视图容器，在游戏过程中显示玩家特定信息。
    /// 管理生命值显示、武器信息、准星和玩家昵称。
    /// </summary>
    public class UIPlayerView : MonoBehaviour {
        public TextMeshProUGUI Nickname;
        public UIHealth Health;
        public UIWeapons Weapons;
        public UICrosshair Crosshair;

        public void UpdatePlayer(ThirdPersonController player, PlayerData playerData) {

            if (Health == null) {
                Debug.LogWarning("UIPlayerView�е�HealthΪ��");
                return;
            }
            if (player.health == null) {
                Debug.LogWarning("UIPlayerView.UpdatePlayer�����player��HealthΪ��");
                return;
            }
            Nickname.text = playerData.Nickname;

            Health.UpdateHealth(player.health);
            //Weapons.UpdateWeapons(player.Weapons);

            Crosshair.gameObject.SetActive(player.health.IsAlive);
        }
    }
}
