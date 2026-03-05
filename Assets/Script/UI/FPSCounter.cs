using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour {
    [Header("调试设置")]
    public bool showFPS = true;          // 是否显示FPS
    public float updateInterval = 0.5f;  // 更新间隔（秒），避免数字跳动太剧烈

    [Header("显示方式")]
    public Text fpsText;                 // UI Text 组件（可选）
    public bool useDebugLog = false;     // 是否使用控制台输出

    private float accum = 0.0f; // 累积时间
    private int frames = 0;     // 帧数
    private float timeLeft;     // 剩余更新时间
    private string fpsLabel = "";

    void Start() {
        timeLeft = updateInterval;
        // 如果未指定UI Text，自动查找Canvas下的Text组件（调试用）
        if (fpsText == null && showFPS) {
            var texts = FindObjectsOfType<Text>();
            foreach (var t in texts) {
                if (t.name.Contains("FPS")) fpsText = t;
            }
        }
    }

    void Update() {
        if (!showFPS) return;

        timeLeft -= Time.unscaledDeltaTime; // 使用unscaled避免受Time.timeScale影响
        accum += Time.timeScale / Time.unscaledDeltaTime;
        frames++;

        if (timeLeft <= 0.0f) {
            float fps = accum / frames;
            fpsLabel = $"{fps:F1} FPS";

            if (fpsText != null) fpsText.text = fpsLabel;
            if (useDebugLog) Debug.Log(fpsLabel);

            // 重置
            timeLeft = updateInterval;
            accum = 0.0f;
            frames = 0;
        }
    }

    // 可通过代码动态开关（适合调试/发布模式切换）
    public void ToggleFPS(bool enable) => showFPS = enable;
}