using Fusion;
using Fusion.Menu;

namespace ThirdpersonGame
{
	/// <summary>
	/// Main menu UI controller that manages menu navigation and game mode selection.
	/// Handles menu state transitions and provides callbacks for game start/stop events.
	/// 主菜单UI控制器，管理菜单导航和游戏模式选择。
	/// 处理菜单状态转换并提供游戏开始/停止事件的回调。
	/// </summary>
	public class MenuUIController : FusionMenuUIController<FusionMenuConnectArgs>
	{
		public FusionMenuConfig Config => _config;

		public GameMode SelectedGameMode { get; protected set; } = GameMode.AutoHostOrClient;

		public virtual void OnGameStarted() {}
		public virtual void OnGameStopped() {}
	}
}
