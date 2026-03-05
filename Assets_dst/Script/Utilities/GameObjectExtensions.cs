using UnityEngine;

namespace ThirdpersonGame
{
	/// <summary>
	/// Extension methods for GameObject and Transform to simplify layer management operations.
	/// Provides convenient methods for setting layers on game objects and their children.
	/// GameObject和Transform的扩展方法，简化层管理操作。
	/// 提供在游戏对象及其子对象上设置层的便捷方法。
	/// </summary>
	public static class GameObjectExtensions
	{
		public static void SetLayer(this GameObject gameObject, int layer, bool includeChildren = false)
		{
			if (includeChildren == false)
			{
				gameObject.layer = layer;
			}
			else
			{
				gameObject.transform.SetLayer(layer, includeChildren);
			}
		}

		public static void SetLayer(this Transform parent, int layer, bool includeChildren = false)
		{
			parent.gameObject.layer = layer;

			if (includeChildren == false)
				return;

			for (int i = 0, count = parent.childCount; i < count; i++)
			{
				parent.GetChild(i).SetLayer(layer, true);
			}
		}
	}
}
