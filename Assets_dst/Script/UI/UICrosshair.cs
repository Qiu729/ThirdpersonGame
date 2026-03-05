using UnityEngine;

namespace ThirdpersonGame
{
	/// <summary>
	/// Manages crosshair hit feedback UI elements and sound effects.
	/// Displays different hit indicators (regular, critical, fatal) with corresponding audio feedback.
	/// 管理准星命中反馈UI元素和音效。
	/// 显示不同的命中指示器（普通、暴击、致命）并提供相应的音频反馈。
	/// </summary>
	public class UICrosshair : MonoBehaviour
	{
		[Header("Hit UI")]
		public GameObject  RegularHit;
		public GameObject  CriticalHit;
		public GameObject  FatalHit;

		[Header("Hit Sounds")]
		public AudioSource RegularHitSound;
		public AudioSource CriticalHitSound;
		public AudioSource FatalHitSound;

		public void ShowHit(bool isFatal, bool isCritical)
		{
			var hitObject = isFatal ? FatalHit : (isCritical ? CriticalHit : RegularHit);

			// Restart hit animation
			hitObject.SetActive(false);
			hitObject.SetActive(true);

			var hitSound = isFatal ? FatalHitSound : (isCritical ? CriticalHitSound : RegularHitSound);
			if (hitSound != null)
			{
				hitSound.PlayOneShot(hitSound.clip);
			}
		}

		private void OnEnable()
		{
			RegularHit.SetActive(false);
			CriticalHit.SetActive(false);
			FatalHit.SetActive(false);
		}
	}
}
