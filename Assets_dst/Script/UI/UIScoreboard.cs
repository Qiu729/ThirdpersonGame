using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ThirdpersonGame
{
	/// <summary>
	/// Manages the scoreboard UI display showing player rankings, kills, deaths, and connection status.
	/// Handles dynamic row creation, player sorting, and visual state updates for disconnected players.
	/// 管理记分牌UI显示，显示玩家排名、击杀、死亡和连接状态。
	/// 处理动态行创建、玩家排序和断开连接玩家的视觉状态更新。
	/// </summary>
	public class UIScoreboard : MonoBehaviour
	{
		public TextMeshProUGUI TotalPlayersText;
		public UIScoreboardRow Row;
		public float           DisconnectedPlayerAlpha = 0.4f;

		List<UIScoreboardRow> _rows = new(32);
		List<PlayerData> _players = new(32);

		private GameUI _gameUI;

		private void Awake()
		{
			Row.gameObject.SetActive(false);
			_rows.Add(Row);

			_gameUI = GetComponentInParent<GameUI>();
		}

		private void OnEnable()
		{
			InvokeRepeating(nameof(UpdateScoreboard), 0f, 0.5f);
		}

		private void OnDisable()
		{
			CancelInvoke();
		}

		private void UpdateScoreboard()
		{
			if (_gameUI.Runner == null)
				return;

			_players.Clear();

			foreach (var record in _gameUI.Gameplay.PlayerData)
			{
				_players.Add(record.Value);
			}

			_players.Sort((a, b) => a.StatisticPosition.CompareTo(b.StatisticPosition));

			TotalPlayersText.text = $"PLAYERS ({_players.Count})";
			PrepareRows(_players.Count);
			UpdateRows();
		}

		private void PrepareRows(int playerCount)
		{
			// Add missing rows
			for (int i = _rows.Count; i < playerCount; i++)
			{
				var row = Instantiate(Row, Row.transform.parent);
				row.gameObject.SetActive(true);

				_rows.Add(row);
			}

			// Activate correct count of rows
			for (int i = 0; i < _rows.Count; i++)
			{
				_rows[i].gameObject.SetActive(i < playerCount);
			}
		}

		private void UpdateRows()
		{
			for (int i = 0; i < _players.Count; i++)
			{
				var row = _rows[i];
				var data = _players[i];

				row.Name.text = data.Nickname;
				row.Kills.text = data.Kills.ToString();
				row.Deaths.text = data.Deaths.ToString();
				row.Position.text = data.StatisticPosition < int.MaxValue ? $"#{data.StatisticPosition}" : "-";

				row.DeadGroup.SetActive(data.IsAlive == false || data.IsConnected == false);
				row.LocalPlayerGroup.SetActive(_gameUI.Runner.LocalPlayer == data.PlayerRef);

				row.CanvasGroup.alpha = data.IsConnected ? 1f : DisconnectedPlayerAlpha;
			}
		}
	}
}
