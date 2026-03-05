using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using Fusion;
using UnityEngine.UIElements;

namespace ThirdpersonGame {

    /// <summary>
    /// Controls third-person aiming mechanics including camera switching, sensitivity adjustment, and character rotation.
    /// Manages aim state, virtual camera activation, and smooth character rotation towards aim target.
    /// 控制第三人称瞄准机制，包括相机切换、灵敏度调整和角色旋转。
    /// 管理瞄准状态、虚拟相机激活和角色向瞄准目标的平滑旋转。
    /// </summary>
    public class ThirdPersonAimController : NetworkBehaviour {
        [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
        [SerializeField] private float normalSensitivity;
        [SerializeField] private float aimSensitivity;
        [SerializeField] private LayerMask mouseColliderLayerMask = new LayerMask();
        //[SerializeField] private Transform debugTransform;

        private Vector3 rayPoint;

        private StarterAssetsInputs starterAssetsInputs;
        private ThirdPersonController thirdPersonController;
        private RigManager rigManager;
        private Animator animator;
        private int aimLayerIndex;
        private bool isAimming;

        private void Awake() {
            starterAssetsInputs = GetComponent<StarterAssetsInputs>();
            thirdPersonController = GetComponent<ThirdPersonController>();
            rigManager = GetComponent<RigManager>();
            animator = GetComponent<Animator>();
            aimLayerIndex = animator.GetLayerIndex("AimingLayer");
        }

        private void FixedUpdate() {

            // 先停用，之后要用网络同步实现瞄准功能

            //if (Camera.main == null) {
            //    Debug.LogWarning("ThirdPersonAimController: No main camera found");
            //    return;
            //}
            //Vector3 mouseWorldPosition = Vector3.zero;
            //Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);
            //Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            //if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, mouseColliderLayerMask)) {
            //    //debugTransform.position = raycastHit.point;
            //    rayPoint = raycastHit.point;
            //    mouseWorldPosition = raycastHit.point;
            //}

            //if (starterAssetsInputs.aim) {
            //    EnableAim();

            //    Vector3 worldAimTarget = mouseWorldPosition;
            //    worldAimTarget.y = transform.position.y; // Keep the aim target at the same height as the player
            //    Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            //    //// Only rotate the character body when there is movement input; rotating the camera alone should not spin the player
            //    //if (starterAssetsInputs.move != Vector2.zero) {
            //    //    transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
            //    //}

            //    Vector3 velocityDirection = new Vector3(starterAssetsInputs.move.x, 0, starterAssetsInputs.move.y);
            //    float angle = Mathf.Lerp(CalculateAngle(velocityDirection), animator.GetFloat("Angle"), Time.deltaTime * 10f);
            //    animator.SetFloat("Angle", angle);
            //} else {
            //    DisableAim();
            //}

        }

        public Vector3 GetRayPoint() {
            return rayPoint;
        }

        private void DisableAim() {
            isAimming = false;
            aimVirtualCamera.gameObject.SetActive(false);
            animator.SetLayerWeight(aimLayerIndex, 0);

            // ����rig���Ȩ��
            rigManager.SetWeaponAimRigWeight(0);
        }

        private void EnableAim() {
            isAimming = true;
            aimVirtualCamera.gameObject.SetActive(true);
            animator.SetLayerWeight(aimLayerIndex, 0.8f);

            // ����rig���Ȩ��
            rigManager.SetWeaponAimRigWeight(1f);
        }

        private float CalculateAngle(Vector3 direction) {
            float angle = Mathf.Atan2(direction.x, direction.z);
            // Debug.Log(angle);
            return angle;
        }

        public bool GetIsAimming() {
            return isAimming;
        }
    }
}
