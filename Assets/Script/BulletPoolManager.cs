using System.Collections.Generic;
using UnityEngine;

public class BulletPoolManager : MonoBehaviour {
    public static BulletPoolManager Instance { get; private set; }

    [Header("池设置")]
    public GameObject bulletPrefab; // 指向挂载了 BulletVisual 的 Prefab
    public int initialPoolSize = 50;
    public int maxPoolSize = 200;

    private Queue<BulletVisual> _pool = new Queue<BulletVisual>();

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializePool();
    }

    private void InitializePool() {
        for (int i = 0; i < initialPoolSize; i++) {
            CreateNewBullet(false);
        }
    }

    private BulletVisual CreateNewBullet(bool startActive = false) {
        GameObject obj = Instantiate(bulletPrefab, transform); // 设置父节点整洁一点
        BulletVisual bullet = obj.GetComponent<BulletVisual>();

        // 如果没有 BulletVisual 组件，报错提示
        if (bullet == null) {
            Debug.LogError("Bullet Prefab 缺少 BulletVisual 组件！");
            return null;
        }

        obj.SetActive(startActive);

        if (!startActive) {
            _pool.Enqueue(bullet);
        }

        return bullet;
    }

    /// <summary>
    /// 发射视觉子弹（供 WeaponData 调用）
    /// </summary>
    public void SpawnBullet(Vector3 position, Vector3 direction) {
        BulletVisual bullet = null;

        if (_pool.Count > 0) {
            bullet = _pool.Dequeue();
        } else {
            // 如果池空了，且没到上限，就生成新的；否则暂时不生成（或者强制扩容）
            // 这里简单的逻辑是：如果不够用就扩容
            bullet = CreateNewBullet(true);
        }

        if (bullet != null) {
            bullet.Initialize(position, direction);
        }
    }

    /// <summary>
    /// 回收子弹（供 BulletVisual 调用）
    /// </summary>
    public void ReturnBullet(BulletVisual bullet) {
        bullet.gameObject.SetActive(false);
        _pool.Enqueue(bullet);
    }
}