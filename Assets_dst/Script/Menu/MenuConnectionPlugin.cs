using UnityEngine;
using Fusion.Menu;

namespace ThirdpersonGame
{
	/// <summary>
	/// Abstract base class for menu connection plugins that provide custom connection implementations.
	/// Allows for extensible connection behavior through plugin architecture.
	/// 菜单连接插件的抽象基类，提供自定义连接实现。
	/// 通过插件架构允许可扩展的连接行为。
	/// </summary>
	public abstract class MenuConnectionPlugin : MonoBehaviour
	{
		public abstract IFusionMenuConnection Create(MenuConnectionBehaviour connectionBehaviour);
	}
}
