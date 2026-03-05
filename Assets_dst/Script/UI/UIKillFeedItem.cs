using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThirdpersonGame
{
	/// <summary>
	/// Individual kill feed item component displaying a single kill event.
	/// Contains UI elements for killer name, victim name, weapon icon, and critical kill indicator.
	/// 显示单个击杀事件的击杀信息项组件。
	/// 包含击杀者姓名、受害者姓名、武器图标和暴击击杀指示器的UI元素。
	/// </summary>
	public class UIKillFeedItem : MonoBehaviour
	{
		public TextMeshProUGUI Killer;
		public TextMeshProUGUI Victim;
		public Image           WeaponIcon;
		public GameObject      CriticalKillGroup;
	}
}
