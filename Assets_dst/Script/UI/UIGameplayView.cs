using UnityEngine;

namespace ThirdpersonGame
{
	/// <summary>
	/// Main gameplay view container that holds gameplay-related UI elements.
	/// Manages the display of kill feed and other in-game UI components.
	/// 主游戏视图容器，包含游戏相关的UI元素。
	/// 管理击杀信息和其他游戏内UI组件的显示。
	/// </summary>
	public class UIGameplayView : MonoBehaviour
	{
		public UIKillFeed KillFeed;
	}
}
