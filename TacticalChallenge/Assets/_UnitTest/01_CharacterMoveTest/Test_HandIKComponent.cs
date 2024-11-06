using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Test_HandIKComponent : MonoBehaviour
{
    private Transform weaponPivot;
    private Transform rigLayerHandIK;
    private TwoBoneIKConstraint leftHandIK;
    private TwoBoneIKConstraint rightHandIK;
    private RigBuilder rigBuilder;

    private Rig rigLayerWeaponAiming;
    private Coroutine aimWeightCoroutine;

    private MultiPositionConstraint weaponPoseConstraint;
    private MultiPositionConstraint weaponAimingConstraint;

    private Test_WeaponComponent weaponComponent;

    private void Awake()
    {
        // WeaponComponent 및 주요 IK 관련 오브젝트를 찾음
        weaponComponent = GetComponent<Test_WeaponComponent>();
        rigBuilder = GetComponent<RigBuilder>(); // RigBuilder 초기화

        weaponPivot = transform.FindChildByName("WeaponPivot");
        rigLayerHandIK = transform.FindChildByName("RigLayer_HandIK");
        rigLayerWeaponAiming = transform.FindChildByName("RigLayer_WeaponAiming")?.GetComponent<Rig>();

        weaponPoseConstraint = transform.FindChildByName("RigLayer_WeaponPose")?.GetComponentInChildren<MultiPositionConstraint>();
        weaponAimingConstraint = transform.FindChildByName("RigLayer_WeaponAiming")?.GetComponentInChildren<MultiPositionConstraint>();

        if (weaponPoseConstraint == null || weaponAimingConstraint == null)
        {
            Debug.LogError("WeaponPose 또는 WeaponAiming MultiPositionConstraint를 찾을 수 없습니다.");
        }

        if (rigLayerHandIK != null)
        {
            leftHandIK = rigLayerHandIK.FindChildByName("LeftHandIK")?.GetComponent<TwoBoneIKConstraint>();
            rightHandIK = rigLayerHandIK.FindChildByName("RightHandIK")?.GetComponent<TwoBoneIKConstraint>();
        }
        else
        {
            Debug.LogError("RigLayer_HandIK를 찾을 수 없습니다.");
        }

        if (rigLayerWeaponAiming == null)
        {
            Debug.LogError("RigLayer_WeaponAiming을 찾을 수 없습니다.");
        }
    }

    public void UpdateWeaponIKTargets()
    {
        // 현재 활성화된 무기 가져오기
        var activeWeapon = weaponComponent.GetActiveWeapon();

        if (activeWeapon != null)
        {
            Transform leftHandGrip = activeWeapon.transform.FindChildByName("LeftHandGrip");
            Transform rightHandGrip = activeWeapon.transform.FindChildByName("RightHandGrip");

            if (leftHandGrip != null && rightHandGrip != null)
            {
                // 왼손과 오른손 IK 타겟 설정
                if (leftHandIK != null) leftHandIK.data.target = leftHandGrip;
                if (rightHandIK != null) rightHandIK.data.target = rightHandGrip;

                // 무기가 교체될 때 RigBuilder 빌드 호출
                rigBuilder.Build(); // 무기 교체 시에만 호출
            }
            else
            {
                Debug.LogError("LeftHandGrip 또는 RightHandGrip이 활성화된 무기에서 발견되지 않았습니다.");
            }
        }
    }

    // RigLayer_WeaponAiming의 Weight 값을 부드럽게 변경하는 메서드
    public void UpdateAimRigWeight(bool isAiming)
    {
        if (rigLayerWeaponAiming == null)
        {
            Debug.LogError("RigLayer_WeaponAiming을 찾을 수 없습니다.");
            return;
        }

        // 이미 실행 중인 코루틴이 있다면 중지
        if (aimWeightCoroutine != null)
        {
            StopCoroutine(aimWeightCoroutine);
        }

        // 목표 weight 설정: 에이밍 중일 때는 1, 그렇지 않을 때는 0
        float targetWeight = isAiming ? 1f : 0f;

        // 목표 weight로 부드럽게 변경하는 코루틴 시작
        aimWeightCoroutine = StartCoroutine(SmoothlyTransitionAimWeight(targetWeight));
    }

    // 목표 weight로 부드럽게 전환하는 코루틴
    private IEnumerator SmoothlyTransitionAimWeight(float targetWeight)
    {
        float currentWeight = rigLayerWeaponAiming.weight;
        float duration = 0.2f; // 변경 시간 (조정 가능)
        float elapsed = 0f;

        while (elapsed < duration)
        {
            rigLayerWeaponAiming.weight = Mathf.Lerp(currentWeight, targetWeight, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rigLayerWeaponAiming.weight = targetWeight; // 정확하게 목표 weight로 설정
    }

    public void ApplyWeaponOffsets()
    {
        // 현재 활성화된 무기를 가져옴
        var activeWeapon = weaponComponent.GetActiveWeapon();

        if (activeWeapon != null)
        {
            // 무기의 오프셋 값을 MultiPositionConstraint에 적용
            if (weaponPoseConstraint != null)
            {
                // 초기화: Offset 값을 Vector3.zero로 설정
                var poseData = weaponPoseConstraint.data;
                poseData.offset = Vector3.zero;
                weaponPoseConstraint.data = poseData;

                // 무기별 오프셋 값으로 설정
                poseData.offset = activeWeapon.weaponPoseOffset;
                weaponPoseConstraint.data = poseData;
            }

            if (weaponAimingConstraint != null)
            {
                // 초기화: Offset 값을 Vector3.zero로 설정
                var aimingData = weaponAimingConstraint.data;
                aimingData.offset = Vector3.zero;
                weaponAimingConstraint.data = aimingData;

                // 무기별 오프셋 값으로 설정
                aimingData.offset = activeWeapon.weaponAimingOffset;
                weaponAimingConstraint.data = aimingData;
            }
        }
    }

    // 이 메서드는 무기 장착 및 교체 시 호출되어야 함
    public void BuildRig()
    {
        rigBuilder.Build();
    }
}
