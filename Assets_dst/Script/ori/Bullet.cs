using UnityEngine;

/// <summary>
/// Handles bullet physics, movement, collision detection, and visual effects.
/// Manages bullet lifetime, damage dealing, and integration with the bullet pool system.
/// 处理子弹物理、移动、碰撞检测和视觉效果。
/// 管理子弹生命周期、伤害计算和与子弹池系统的集成。
/// </summary>
public class Bullet : MonoBehaviour {
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private int damage = 10;
    [SerializeField] private ParticleSystem impactEffect;
    [SerializeField] private TrailRenderer trailRenderer;
    [Header("Collision Settings")]
    [SerializeField] private float collisionCheckDistance = 1.0f;

    private float timer;
    private bool isActive;
    private Vector3 targetDirection;

    public void Initialize(Vector3 direction) {
        targetDirection = direction.normalized;
        isActive = true;
        timer = lifeTime;

        if (trailRenderer != null) {
            trailRenderer.Clear();
            trailRenderer.emitting = true;
        }

        if (impactEffect != null)
            impactEffect.Stop();
    }

    private void Update() {
        if (!isActive) return;

        // ��Ŀ�귽���ƶ��ӵ�
        transform.position += targetDirection * speed * Time.deltaTime;

        // �������ڵ���ʱ
        timer -= Time.deltaTime;
        if (timer <= 0f) {
            ReturnToPool();
        }
    }
    private void FixedUpdate() {
        if (!isActive) return;

        // ʹ�����߼�������ٴ�͸����
        RaycastHit hit;
        if (Physics.Raycast(
            transform.position,
            targetDirection,
            out hit,
            speed * Time.fixedDeltaTime + collisionCheckDistance)) {
            if (hit.collider != null) {
                // Debug.Log($"Raycast hit: {hit.collider.name}");
                transform.position = hit.point; // ��ȷ�ƶ�����ײ��
                OnTriggerEnter(hit.collider); // �ֶ����ô�������
            }
        } else {
            // �����ƶ�
            transform.position += targetDirection * speed * Time.fixedDeltaTime;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (!isActive) return;

        //// �����߼�
        //if (other.CompareTag("ShootTarget")) {
        //    // ����������ӵ����˺��߼�
        //    Debug.Log($"Hit enemy: {other.name}");
        //}

        // ��������Ч��
        if (impactEffect != null) {
            // Debug.Log("Bullet hit something, playing impact effect.");
            impactEffect.transform.position = transform.position;
            impactEffect.transform.rotation = Quaternion.LookRotation(targetDirection);
            impactEffect.Play();
        }

        ReturnToPool();
    }

    private void ReturnToPool() {
        isActive = false;

        if (trailRenderer != null)
            trailRenderer.emitting = false;

        BulletPoolManager.Instance.ReturnBullet(gameObject);
    }
}