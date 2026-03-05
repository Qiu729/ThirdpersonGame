using TMPro;
using UnityEngine;

namespace ThirdpersonGame
{
	/// <summary>
	/// Individual row component for the scoreboard displaying a single player's statistics.
	/// Contains UI elements for position, name, kills, deaths, and visual state indicators.
	/// 记分牌中显示单个玩家统计信息的单独行组件。
	/// 包含位置、姓名、击杀、死亡和视觉状态指示器的UI元素。
	/// </summary>
	public class UIScoreboardRow : MonoBehaviour
	{
		public CanvasGroup     CanvasGroup;
		public TextMeshProUGUI Position;
		public TextMeshProUGUI Name;
		public TextMeshProUGUI Kills;
		public TextMeshProUGUI Deaths;
		public GameObject      LocalPlayerGroup;
		public GameObject      DeadGroup;
	}
}
