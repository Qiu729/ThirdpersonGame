using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 为主摄像头添加一个进入游戏时从上往下移动的动画
/// </summary>
public class MainCamera : MonoBehaviour {

    [SerializeField] private float MotionDuration;
    [SerializeField] private float MoveDistance;

    [SerializeField] private GameObject UIGameMenu;

    private Transform _transform;

    private void Awake() {
        _transform = transform;
    }

    void Start() {
        StartCoroutine(StartGameMotion());
    }

    private IEnumerator StartGameMotion() {
        // 先隐藏开始菜单，然后开始动画
        UIGameMenu.SetActive(false);

        float elapsedTime = 0f;
        var startPosOffset = _transform.position;
        var endPos = new Vector3(startPosOffset.x, startPosOffset.y - MoveDistance, startPosOffset.z);

        while (elapsedTime < MotionDuration) {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / MotionDuration; // 归一化时间 [0,1]
            _transform.position = Vector3.Lerp(startPosOffset, endPos, t);
            yield return null;
        }

        // 确保最终位置精确
        _transform.position = endPos;

        // 显示开始菜单
        UIGameMenu.SetActive(true);
    }
}
