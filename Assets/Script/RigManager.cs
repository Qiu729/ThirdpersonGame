using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Manages animation rigging weights for character IK systems including weapon aiming and hand positioning.
/// Controls the blending of different rig constraints for smooth character animation transitions.
/// 管理角色IK系统的动画装配权重，包括武器瞄准和手部定位。
/// 控制不同装配约束的混合，实现平滑的角色动画过渡。
/// </summary>
public class RigManager : MonoBehaviour {
    [SerializeField] private MultiAimConstraint weaponAimRig;
    [SerializeField] private MultiAimConstraint upperBodyAimRig;
    [SerializeField] private MultiAimConstraint headAimRig;
    [SerializeField] private TwoBoneIKConstraint leftHandGridRig;

    private Animator animator;
    private float weaponAimRigWeight;
    private float upperBodyAimRigWeight;
    private float headAimRigWeight;
    private float leftHandGridWeight;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    public void SetWeaponAimRigWeight(float newWeaponAimRigWeight) {
        weaponAimRigWeight = newWeaponAimRigWeight;
        weaponAimRig.weight = newWeaponAimRigWeight;
    }

    public void SetUpperBodyAimRigWeight(float newUpperBodyAimRigWeight) {
        upperBodyAimRigWeight = newUpperBodyAimRigWeight;
        upperBodyAimRig.weight = newUpperBodyAimRigWeight;
    }
    public void SetHeadAimRigWeight(float newHeadAimRigWeight) {
        headAimRigWeight = newHeadAimRigWeight;
        headAimRig.weight = newHeadAimRigWeight;
    }

    public void SetLeftHandGridRig(float newLeftHandGridRig) {
        leftHandGridWeight = newLeftHandGridRig;
        leftHandGridRig.weight = newLeftHandGridRig;
    }
}
