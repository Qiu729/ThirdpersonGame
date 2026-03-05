using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ThirdpersonGame {
    /// <summary>
    /// Settings UI panel for configuring game options like volume and mouse sensitivity.
    /// Handles settings persistence using PlayerPrefs and real-time updates to game systems.
    /// 设置UI面板，用于配置游戏选项，如音量和鼠标灵敏度。
    /// 使用PlayerPrefs处理设置持久化，并实时更新游戏系统。
    /// </summary>
    public class UISettingsView : MonoBehaviour {
        public Slider Volume;
        public Slider Sensitivity;
        public TextMeshProUGUI SensitivityValue;

        public void LoadSettings() {
            float volume = PlayerPrefs.GetFloat("Volume", 1f);
            Volume.value = volume;

            float sensitivity = PlayerPrefs.GetFloat("Sensitivity", 3f);
            Sensitivity.value = sensitivity;

            AudioListener.volume = volume;
            StarterAssetsInputs.LookSensitivity = sensitivity;
        }

        // Called from slider OnChanged event.
        public void SensitivityChanged(float value) {
            StarterAssetsInputs.LookSensitivity = value;
            PlayerPrefs.SetFloat("Sensitivity", value);

            SensitivityValue.text = $"{value:F1}";
        }

        // Called from slider OnChanged event.
        public void VolumeChanged(float value) {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat("Volume", value);
        }

        // Called from button OnClick event.
        public void CloseView() {
            gameObject.SetActive(false);
            GetComponentInParent<GameUI>().MenuView.SetActive(false);
        }

        private void Update() {
            if (Keyboard.current.escapeKey.wasPressedThisFrame) {
                gameObject.SetActive(false);
            }
        }
    }
}
