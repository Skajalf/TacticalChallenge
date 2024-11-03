using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HandIKComponent : MonoBehaviour
{
    private Transform weaponPivot;
    private Transform rigLayerHandIK;
    private TwoBoneIKConstraint leftHandIK;
    private TwoBoneIKConstraint rightHandIK;
    private RigBuilder rigBuilder;

    private WeaponComponent weaponComponent;

    private void Awake()
    {
        // WeaponComponent 및 주요 IK 관련 오브젝트를 찾음
        weaponComponent = GetComponent<WeaponComponent>();
        weaponPivot = transform.FindChildByName("WeaponPivot");
        rigLayerHandIK = transform.FindChildByName("RigLayer_HandIK");
        rigBuilder = GetComponent<RigBuilder>(); // RigBuilder 초기화

        if (rigLayerHandIK != null)
        {
            leftHandIK = rigLayerHandIK.FindChildByName("LeftHandIK")?.GetComponent<TwoBoneIKConstraint>();
            rightHandIK = rigLayerHandIK.FindChildByName("RightHandIK")?.GetComponent<TwoBoneIKConstraint>();
        }
        else
        {
            Debug.LogError("RigLayer_HandIK를 찾을 수 없습니다.");
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

    // 이 메서드는 무기 장착 및 교체 시 호출되어야 함
    public void BuildRig()
    {
        rigBuilder.Build();
    }
}
