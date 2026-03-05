using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThirdpersonGame;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

/// <summary>
/// Handles additional input processing for character actions like crouching and weapon handling.
/// Manages input state changes and animator parameter updates for character behaviors.
/// 处理角色的额外输入处理，如蹲下和武器操作。
/// 管理输入状态变化和角色行为的动画器参数更新。
/// </summary>
public class HandleInput : MonoBehaviour {
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;

    private bool isCrouch;

    //public bool isArmed = false; // �Ƿ��ֳ�����
    public Rig leftHandIK; // �����ֳ�������IK Rig�������ǰ״̬���ֳ�������������IK Rig���������IK Rig
    public GameObject Weapon; // ��������


    void Awake() {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        animator = GetComponent<Animator>();
    }
    void Update() {
        //animator.SetBool("ShiftArmed", starterAssetsInputs.armed);
        //if (starterAssetsInputs.armed) {
        //    isArmed = !isArmed; // ��ת�ֳ�����״̬
        //    starterAssetsInputs.armed = false; // ��������״̬����ֹ�ظ�����
        //}

        if (starterAssetsInputs.crouch) {
            isCrouch = !isCrouch; // ��ת����״̬
            animator.SetBool("Crouch", isCrouch);
            starterAssetsInputs.crouch = false; // ��������״̬����ֹ�ظ�����
        }

        //if (weaponManager.isArmed) {
        //    leftHandIK.weight = 1f; // ����IK Rig
        //    //if (Weapon != null) {
        //    //    Weapon.SetActive(true); // ������������
        //    //} else {
        //    //    Debug.LogWarning("Weapon object is not assigned in HandleInput script.");
        //    //}
        //    starterAssetsInputs.EnableFire(); // ����fire����״̬
        //    starterAssetsInputs.EnableAim(); // ����aim����״̬
        //} else {
        //    leftHandIK.weight = 0f; // ����IK Rig
        //    //if (Weapon != null) {
        //    //    Weapon.SetActive(false); // ������������
        //    //}
        //    starterAssetsInputs.DisableFire(); // ����fire����״̬
        //    starterAssetsInputs.DisableAim(); // ����aim����״̬
        //}
    }

    public bool GetIsCrouch() {
        return isCrouch;
    }
}
