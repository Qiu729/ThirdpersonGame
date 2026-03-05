using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton object pool manager for bullets to optimize performance and memory usage.
/// Handles bullet instantiation, pooling, and recycling to prevent garbage collection spikes.
/// 子弹对象池管理的单例类，用于优化性能和内存使用。
/// 处理子弹实例化、池化和回收，防止垃圾回收峰值。
/// </summary>
public class BulletPoolManager : MonoBehaviour {
    public static BulletPoolManager Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int initialPoolSize = 30;
    [SerializeField] private int maxPoolSize = 100;

    private Queue<GameObject> bulletPool = new Queue<GameObject>();
    private List<GameObject> activeBullets = new List<GameObject>();

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            InitializePool();
        } else {
            Destroy(gameObject);
        }
    }

    private void InitializePool() {
        for (int i = 0; i < initialPoolSize; i++) {
            CreateNewBullet();
        }
    }

    private GameObject CreateNewBullet() {
        GameObject bullet = Instantiate(bulletPrefab);
        bullet.SetActive(false);
        bullet.transform.SetParent(transform);
        bulletPool.Enqueue(bullet);
        return bullet;
    }

    public GameObject GetBullet() {
        if (bulletPool.Count == 0 && activeBullets.Count < maxPoolSize) {
            CreateNewBullet();
        }

        if (bulletPool.Count > 0) {
            GameObject bullet = bulletPool.Dequeue();
            activeBullets.Add(bullet);
            return bullet;
        }

        return null; // Pool exhausted
    }

    public void ReturnBullet(GameObject bullet) {
        if (activeBullets.Contains(bullet)) {
            activeBullets.Remove(bullet);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }

    public int GetActiveBulletCount() {
        return activeBullets.Count;
    }

    public int GetPooledBulletCount() {
        return bulletPool.Count;
    }
}