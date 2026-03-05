using UnityEngine;

namespace ThirdpersonGame
{
	/// <summary>
	/// Manages the kill feed display showing recent player eliminations with weapon icons and critical kill indicators.
	/// Handles kill feed item instantiation, lifetime management, and visual effects.
	/// 管理击杀信息显示，显示最近的玩家淘汰信息，包括武器图标和暴击击杀指示器。
	/// 处理击杀信息项实例化、生命周期管理和视觉效果。
	/// </summary>
	public class UIKillFeed : MonoBehaviour
	{
		public UIKillFeedItem KillFeedItemPrefab;
		public float          ItemLifetime = 6f;
		public Sprite[]       WeaponIcons;

		public void ShowKill(string killer, string victim, EWeaponType weaponType, bool isCriticalKill)
		{
			var item = Instantiate(KillFeedItemPrefab, transform);

			item.Killer.text = killer;
			item.Victim.text = victim;
			item.WeaponIcon.sprite = WeaponIcons[(int)weaponType];
			item.CriticalKillGroup.SetActive(isCriticalKill);


			// Kill feed item is fading in time automatically via animation component.
			// Make sure the item gets destroyed after the animation is done.
			Destroy(item.gameObject, ItemLifetime);
		}
	}
}
