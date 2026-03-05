using System.Collections;
using System.Collections.Generic;
using ThirdpersonGame;
using UnityEngine;
using UnityEngine.UI;

public class PlayerReferences : MonoBehaviour {
    [Header("玩家需要引用的组件")]
    public RectTransform JoystickRectTransform; // 虚拟摇杆的RectTransform

    // 订阅玩家加入事件
    public void OnPlayerJoined() {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players) {
            var playerInput = player.GetComponent<StarterAssetsInputs>();
            if (playerInput != null) {
                playerInput.joystickRectTransform = JoystickRectTransform;
                Debug.Log($"已为玩家 {player.name} 设置虚拟摇杆引用");
            }
        }
    }
}
