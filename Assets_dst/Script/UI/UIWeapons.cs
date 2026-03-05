using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThirdpersonGame {
    /// <summary>
    /// Manages weapon UI display including weapon icon, name, ammo count, and reload progress.
    /// Handles weapon switching, ammo updates, and visual state indicators for different weapon types.
    /// 管理武器UI显示，包括武器图标、名称、弹药数量和装弹进度。
    /// 处理武器切换、弹药更新和不同武器类型的视觉状态指示器。
    /// </summary>
    public class UIWeapons : MonoBehaviour {
        public Image WeaponIcon;
        public Image WeaponIconShadow;
        public TextMeshProUGUI WeaponName;
        public TextMeshProUGUI ClipAmmo;
        public TextMeshProUGUI RemainingAmmo;
        public Image AmmoProgress;
        public GameObject NoAmmoGroup;
        public CanvasGroup[] WeaponThumbnails;

        private WeaponData _weapon;
        private int _lastClipAmmo;
        private int _lastRemainingAmmo;

        public void UpdateWeapons(WeaponManager weaponManager) {
            SetWeapon(weaponManager.CurrentWeapon);

            // Update weapon thumbnails.
            for (int i = 0; i < weaponManager.weapons.Length; i++) {
                var weapon = weaponManager.weapons[i];
                //WeaponThumbnails[i].alpha = weapon.IsCollected && weapon.HasAmmo ? 1f : 0.2f;
                WeaponThumbnails[i].alpha = weapon.HasAmmo ? 1f : 0.2f;
            }

            if (_weapon == null)
                return;

            UpdateAmmoProgress();

            // 只有值变化时才更新UI，减少不必要的UI更新。
            if (_weapon.clipAmmo == _lastClipAmmo && _weapon.remainingAmmo == _lastRemainingAmmo)
                return;

            ClipAmmo.text = _weapon.clipAmmo.ToString();
            RemainingAmmo.text = _weapon.remainingAmmo < 1000 ? _weapon.remainingAmmo.ToString() : "-";

            NoAmmoGroup.SetActive(_weapon.clipAmmo == 0 && _weapon.remainingAmmo == 0);

            _lastClipAmmo = _weapon.clipAmmo;
            _lastRemainingAmmo = _weapon.remainingAmmo;
        }

        private void SetWeapon(WeaponData weapon) {
            if (weapon == _weapon)
                return;

            _weapon = weapon;

            if (weapon == null)
                return;

            WeaponIcon.sprite = weapon.Icon;
            WeaponIconShadow.sprite = weapon.Icon;
            WeaponName.text = weapon.weaponName;
        }

        private void UpdateAmmoProgress() {
            if (_weapon.isReloading) {
                // During reloading the ammo progress bar slowly fills.
                AmmoProgress.fillAmount = _weapon.GetReloadProgress();
            } else {
                AmmoProgress.fillAmount = _weapon.clipAmmo / (float)_weapon.maxClipAmmo;
            }
        }
    }
}
