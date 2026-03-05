using Fusion;
using UnityEngine;

namespace ThirdpersonGame {
    /// <summary>
    /// Enum defining different weapon types such as Rifle, Pistol, and Knife.
    /// Used to categorize weapons and apply specific behaviors based on type.
    /// 定义不同武器类型的枚举，如步枪、手枪和匕首。
    /// 用于对武器进行分类，并根据类型应用特定行为。
    /// </summary>

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

        // 通用属性
        public string weaponName; // 武器名称
        public WeaponType weaponType; // 武器类型
        public bool HasAmmo = true; // 是否有弹药
        public bool isReloading = false; // 是否正在装弹
        public Sprite Icon; // 武器图标

        // 射击类武器专用属性
        public Transform firePoint; // 步枪/手枪等有
        public float fireRate = 0.2f;
        private TickTimer _fireCooldown { get; set; } // 射击冷却时间
        public float hipFireSpread = 0.05f;
        public float aimedFireSpread = 0.05f;
        public GameObject bulletPrefab;
        public int clipAmmo = 30; // 当前弹夹中的弹药数量
        public int remainingAmmo = 90; // 剩余弹药数量
        public int maxClipAmmo = 30; // 最大弹夹容量
        public float reloadTime = 2f; // 装弹时间

        // 近战类武器（如匕首）专用属性
        public float meleeRange = 1.5f; // 匕首攻击距离
        public float meleeDamage = 20f; // 匕首伤害

        public float GetReloadProgress() {
            // 计算装弹进度，返回0到1之间的值
            if (isReloading == false)
                return 1f;

            return 1f - _fireCooldown.RemainingTime(Runner).GetValueOrDefault() / reloadTime;
        }
    }
}