using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// 早期白盒阶段测试脚本，现在已经弃用
/// </summary>
public class Enemy : NetworkBehaviour, IDamageable {

    private Coroutine restoreColorCoroutine; // 从受伤的颜色中恢复过来的协程

    // 引用
    private Renderer Renderer;

    // 网络同步参数
    [Networked] private Color _Color { get; set; }

    private void Awake() {
        Renderer = GetComponent<MeshRenderer>();
    }

    public override void Render() {
        Renderer.material.color = _Color;
    }

    public void TakeDamage() {
        //Debug.Log("敌人收到伤害");
        _Color = Color.red;

        if (restoreColorCoroutine != null) {
            StopCoroutine(restoreColorCoroutine);
        }
        restoreColorCoroutine = StartCoroutine(RestoreColorAfterDelay());
    }

    private IEnumerator RestoreColorAfterDelay() {
        yield return new WaitForSeconds(0.02f);
        _Color = Color.white;
        restoreColorCoroutine = null; // 重置引用
    }
}
