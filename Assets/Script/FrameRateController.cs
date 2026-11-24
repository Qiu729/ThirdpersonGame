using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameRateController : MonoBehaviour {
    void Start() {
#if UNITY_EDITOR
        // 设置目标帧率为 -1，表示无限制
        Application.targetFrameRate = -1;

        // 重要：关闭垂直同步 (VSync)，否则帧率会被限制在显示器刷新率
        QualitySettings.vSyncCount = 0;
#else
        QualitySettings.vSyncCount = 1;
#endif
    }
}
