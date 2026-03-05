using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThirdpersonGame {
    /// <summary>
    /// Singleton on Runner used to obtain scene object references using lazy getters.
    /// Runner上的单例，用于使用延迟获取器获取场景对象引用。
    /// </summary>
    public class SceneObjects : SimulationBehaviour {
        // Use Runner.GetSingleton<SceneObjects>() to get SceneObjects instance.
        // 使用Runner.GetSingleton<SceneObjects>()获取SceneObjects实例。

        public Gameplay Gameplay {
            get {
                if (_gameplay == null && Runner != null && Runner.SceneManager != null && Runner.SceneManager.MainRunnerScene.IsValid()) {
                    var gameplays = Runner.SceneManager.MainRunnerScene.GetComponents<Gameplay>(true);
                    if (gameplays.Length > 0) {
                        _gameplay = gameplays[0];
                    }
                }

                return _gameplay;
            }
        }

        public GameUI GameUI {
            get {
                if (_gameUI == null && Runner != null && Runner.SceneManager != null && Runner.SceneManager.MainRunnerScene.IsValid()) {
                    var gameUIs = Runner.SceneManager.MainRunnerScene.GetComponents<GameUI>(true);
                    if (gameUIs.Length > 0) {
                        _gameUI = gameUIs[0];
                    }
                }

                return _gameUI;
            }
        }

        private Gameplay _gameplay;
        private GameUI _gameUI;
    }
}
