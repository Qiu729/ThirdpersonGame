using UnityEngine;

namespace ThirdpersonGame
{
	/// <summary>
	/// Main menu UI controller that manages menu visibility and cursor state during game transitions.
	/// Handles menu display logic and ensures proper UI state when starting or stopping the game.
	/// 主菜单UI控制器，管理游戏转换期间的菜单可见性和光标状态。
	/// 处理菜单显示逻辑，确保开始或停止游戏时UI状态正确。
	/// </summary>
	public class MenuUI : MenuUIController
	{
		public override void OnGameStarted()
		{
			base.OnGameStarted();

			foreach (Transform child in transform)
			{
				child.gameObject.SetActive(false);
			}
		}

		public override void OnGameStopped()
		{
			foreach (Transform child in transform)
			{
				child.gameObject.SetActive(true);
			}

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			base.OnGameStopped();
		}

		protected override void Awake()
		{
			base.Awake();

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}
}
