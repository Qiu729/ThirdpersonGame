using UnityEngine;
using System.Collections;

public class BulletVisual : MonoBehaviour {
    [Header("视觉设置")]
    public float speed = 50f;
    public float maxLifeTime = 2f;
    public TrailRenderer trailRenderer; // 拖尾特效（可选）
    public GameObject impactPrefab;     // 命中特效（可选）

    private Vector3 _targetDerection;
    private bool _isActive;
    private float _timer;

    // 初始化方法：由对象池管理器调用
    public void Initialize(Vector3 startPos, Vector3 direction) {
        transform.position = startPos;
        transform.forward = direction; // 设置朝向
        transform.Rotate(0, 90, 0);
        _targetDerection = direction;
        _timer = maxLifeTime;
        _isActive = true;

        // 处理拖尾：由于是复用对象，必须重置拖尾，否则会有一条线连着上一个位置
        if (trailRenderer != null) {
            trailRenderer.Clear();
            trailRenderer.emitting = true;
        }

        gameObject.SetActive(true);
    }

    private void Update() {
        if (!_isActive) return;

        float moveDistance = speed * Time.deltaTime;

        // 1. 简单的视觉碰撞检测 (防止穿墙)
        // 这一步只是为了视觉效果，实际伤害逻辑在 WeaponData 里做
        if (Physics.Raycast(transform.position, _targetDerection, out RaycastHit hit, moveDistance)) {
            // 移动到碰撞点
            transform.position = hit.point;
            HitObject(hit);
            return;
        }

        // 2. 移动
        transform.position += _targetDerection * moveDistance;

        // 3. 超时回收
        _timer -= Time.deltaTime;
        if (_timer <= 0) {
            ReturnToPool();
        }
    }

    private void HitObject(RaycastHit hit) {
        // 生成命中特效（火花等）
        if (impactPrefab != null) {
            Instantiate(impactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }

        // 在这里可以添加击中声音

        // 回收子弹
        ReturnToPool();
    }

    private void ReturnToPool() {
        _isActive = false;
        if (trailRenderer != null) {
            trailRenderer.emitting = false;
        }
        // 通知管理器回收自己
        BulletPoolManager.Instance.ReturnBullet(this);
    }
}