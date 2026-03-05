using Fusion;
using Newtonsoft.Json.Bson;
using UnityEngine;

namespace ThirdpersonGame {

    public enum WeaponType {
        Rifle,
        Pistol,
        Knife, // 新增匕首类型
               // 可扩展：Shotgun, Sniper, SMG, 等
    }

    /// <summary>
    /// Data structure that defines weapon properties including damage, fire rate, spread, and visual effects.
    /// Used to configure different weapon types with their specific characteristics.
    /// 定义武器属性的数据结构，包括伤害、射速、散布和视觉效果。
    /// 用于配置不同武器类型及其特定特征。
    /// </summary>
    [System.Serializable]
    public class WeaponData : NetworkBehaviour {
        public string weaponName;
        public WeaponType weaponType;
        public bool HasAmmo = true; // 是否有弹药
        public bool isReloading = false; // 是否正在换弹
        //public bool isShooting = false; // 是否正在射击
        public Sprite Icon; // 武器图标

        // 射击类武器专用属性
        public Transform firePoint; // 步枪/手枪等有
        public float fireRate = 10f; // 每秒射出的子弹数量
        public float hipFireSpread = 0.05f;
        public float aimedFireSpread = 0.05f;
        public GameObject bulletPrefab;
        public int ClipAmmo = 30; // 当前弹夹弹药
        public int MaxClipAmmo = 30; // 弹夹最大弹药
        public int RemainingAmmo = 90; // 剩余总弹药   ClipAmmo/RemainingAmmo
        public float ReloadTime = 2f;
        //private float nextFireTime = 0f; // 下次允许开火的时间

        // 近战类武器（如匕首）专用属性
        public float meleeRange = 1.5f; // 匕首攻击距离
        public float meleeDamage = 20f; // 匕首伤害

        // 网络参数
        [Networked] private TickTimer _fireCooldown { get; set; } // 网络计时器，用于跟踪时间
        [Networked] private TickTimer _shootCooldown { get; set; }
        [Networked] private NetworkBool IsShooting { get; set; }

        // 引用
        public ThirdPersonController thirdPersonController; // 获取玩家瞄准的位置

        public float GetReloadProgress() {
            if (isReloading == false)
                return 1f;

            return 1f - _fireCooldown.RemainingTime(Runner).GetValueOrDefault() / ReloadTime;
        }

        public bool Reload() {
            if (isReloading || ClipAmmo >= MaxClipAmmo || RemainingAmmo <= 0)
                return false;

            isReloading = true;
            _fireCooldown = TickTimer.CreateFromSeconds(Runner, ReloadTime);
            return true;
        }

        // 开始开火
        public void StartFire() {
            IsShooting = true;
        }

        // 停止开火
        public void StopFire() {
            IsShooting = false;
        }


        // 播放空弹夹声音的占位方法
        public void PlayEmptyClipSound(bool fireJustPressed) {

        }

        public override void FixedUpdateNetwork() {
            // 处理换弹完成
            if (isReloading && _fireCooldown.ExpiredOrNotRunning(Runner)) {
                int ammoNeeded = MaxClipAmmo - ClipAmmo;
                int ammoToLoad = Mathf.Min(ammoNeeded, RemainingAmmo);
                ClipAmmo += ammoToLoad;
                RemainingAmmo -= ammoToLoad;
                isReloading = false;
            }

            //if (!(IsShooting && HasInputAuthority)) return;
            if (GetInput(out NetworkedInput input)) {
                if (IsShooting) {
                    Runner.LagCompensation.Raycast(
                        firePoint.position,
                        (input.AimPosition - firePoint.position).normalized,
                        999f,
                        Runner.LocalPlayer,
                        out var hit
                    );
                    if (hit.Hitbox != null) {
                        var obj = hit.Hitbox.Root.transform.GetComponent<IDamageable>();
                        if (obj != null) {
                            obj.TakeDamage();
                        }
                    }
                }


                // 射击 测试中，先不限制子弹数量
                var aimPosition = input.AimPosition;
                var targetDirection = (aimPosition - firePoint.position).normalized;
                if (isReloading == false && IsShooting && _shootCooldown.ExpiredOrNotRunning(Runner)) {
                    //Debug.Log("isShoting...");
                    ClipAmmo--;
                    // 重启计时器
                    _shootCooldown = TickTimer.CreateFromSeconds(Runner, 1f / fireRate);

                    // 通知子弹池管理器发射子弹
                    BulletPoolManager.Instance.SpawnBullet(firePoint.position, targetDirection);
                }
            }

        }
    }
}