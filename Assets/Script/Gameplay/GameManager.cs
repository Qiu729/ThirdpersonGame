using UnityEngine;
using Fusion;
using ThirdpersonGame;
using UnityEngine.Events;

namespace Starter.ThirdPersonCharacter {
    /// <summary>
    /// Handles player connections (spawning of Player instances).
    /// </summary>
    public sealed class GameManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft {

        [SerializeField] private Transform PlayerSpwanPoint;

        public float SpawnRadius = 3f;
        public NetworkPrefabRef PlayerPrefab;
        public NetworkRunner runner; // 公开给其他脚本引用
        public ThirdPersonController LocalPlayer { get; private set; }

        // Unity事件
        public UnityEvent PlayerJoin;

        public override void Spawned() {
            runner = Runner;
        }

        public override void Despawned(NetworkRunner runner, bool hasState) {
            LocalPlayer = null;
        }

        //public override void Render() {
        //    // 为UI准备本地玩家引用
        //    if (LocalPlayer == null && Runner != null && Runner.LocalPlayer != null) {
        //        var playerObject = Runner.GetPlayerObject(Runner.LocalPlayer);
        //        if (playerObject != null) {
        //            LocalPlayer = playerObject.GetComponent<ThirdPersonController>();
        //        }
        //    }
        //}

        public void PlayerJoined(PlayerRef playerRef) {
            if (HasStateAuthority == false)
                return;

            var randomPositionOffset = Random.insideUnitCircle * SpawnRadius;
            var spawnPosition = PlayerSpwanPoint.position + new Vector3(randomPositionOffset.x, PlayerSpwanPoint.position.y, randomPositionOffset.y);

            var player = Runner.Spawn(PlayerPrefab, spawnPosition, Quaternion.identity, playerRef);
            player.name = $"Player {playerRef.PlayerId}";
            Runner.SetPlayerObject(playerRef, player);

            if (LocalPlayer == null && Runner != null && Runner.LocalPlayer != null) {
                var playerObject = Runner.GetPlayerObject(Runner.LocalPlayer);
                if (playerObject != null) {
                    LocalPlayer = playerObject.GetComponent<ThirdPersonController>();
                }
            }

            PlayerJoin?.Invoke();
        }

        public void PlayerLeft(PlayerRef playerRef) {
            if (HasStateAuthority == false)
                return;

            var player = Runner.GetPlayerObject(playerRef);
            if (player != null) {
                Runner.Despawn(player);
            }
        }

        private void OnDrawGizmosSelected() {
            Gizmos.DrawWireSphere(transform.position, SpawnRadius);
        }
    }
}
