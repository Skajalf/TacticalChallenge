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
        // WeaponComponent �� �ֿ� IK ���� ������Ʈ�� ã��
        weaponComponent = GetComponent<Test_WeaponComponent>();
        rigBuilder = GetComponent<RigBuilder>(); // RigBuilder �ʱ�ȭ

        weaponPivot = transform.FindChildByName("WeaponPivot");
        rigLayerHandIK = transform.FindChildByName("RigLayer_HandIK");
        rigLayerWeaponAiming = transform.FindChildByName("RigLayer_WeaponAiming")?.GetComponent<Rig>();

        weaponPoseConstraint = transform.FindChildByName("RigLayer_WeaponPose")?.GetComponentInChildren<MultiPositionConstraint>();
        weaponAimingConstraint = transform.FindChildByName("RigLayer_WeaponAiming")?.GetComponentInChildren<MultiPositionConstraint>();

        if (weaponPoseConstraint == null || weaponAimingConstraint == null)
        {
            Debug.LogError("WeaponPose �Ǵ� WeaponAiming MultiPositionConstraint�� ã�� �� �����ϴ�.");
        }

        if (rigLayerHandIK != null)
        {
            leftHandIK = rigLayerHandIK.FindChildByName("LeftHandIK")?.GetComponent<TwoBoneIKConstraint>();
            rightHandIK = rigLayerHandIK.FindChildByName("RightHandIK")?.GetComponent<TwoBoneIKConstraint>();
        }
        else
        {
            Debug.LogError("RigLayer_HandIK�� ã�� �� �����ϴ�.");
        }

        if (rigLayerWeaponAiming == null)
        {
            Debug.LogError("RigLayer_WeaponAiming�� ã�� �� �����ϴ�.");
        }
    }

    public void UpdateWeaponIKTargets()
    {
        // ���� Ȱ��ȭ�� ���� ��������
        var activeWeapon = weaponComponent.GetActiveWeapon();

        if (activeWeapon != null)
        {
            Transform leftHandGrip = activeWeapon.transform.FindChildByName("LeftHandGrip");
            Transform rightHandGrip = activeWeapon.transform.FindChildByName("RightHandGrip");

            if (leftHandGrip != null && rightHandGrip != null)
            {
                // �޼հ� ������ IK Ÿ�� ����
                if (leftHandIK != null) leftHandIK.data.target = leftHandGrip;
                if (rightHandIK != null) rightHandIK.data.target = rightHandGrip;

                // ���Ⱑ ��ü�� �� RigBuilder ���� ȣ��
                rigBuilder.Build(); // ���� ��ü �ÿ��� ȣ��
            }
            else
            {
                Debug.LogError("LeftHandGrip �Ǵ� RightHandGrip�� Ȱ��ȭ�� ���⿡�� �߰ߵ��� �ʾҽ��ϴ�.");
            }
        }
    }

    // RigLayer_WeaponAiming�� Weight ���� �ε巴�� �����ϴ� �޼���
    public void UpdateAimRigWeight(bool isAiming)
    {
        if (rigLayerWeaponAiming == null)
        {
            Debug.LogError("RigLayer_WeaponAiming�� ã�� �� �����ϴ�.");
            return;
        }

        // �̹� ���� ���� �ڷ�ƾ�� �ִٸ� ����
        if (aimWeightCoroutine != null)
        {
            StopCoroutine(aimWeightCoroutine);
        }

        // ��ǥ weight ����: ���̹� ���� ���� 1, �׷��� ���� ���� 0
        float targetWeight = isAiming ? 1f : 0f;

        // ��ǥ weight�� �ε巴�� �����ϴ� �ڷ�ƾ ����
        aimWeightCoroutine = StartCoroutine(SmoothlyTransitionAimWeight(targetWeight));
    }

    // ��ǥ weight�� �ε巴�� ��ȯ�ϴ� �ڷ�ƾ
    private IEnumerator SmoothlyTransitionAimWeight(float targetWeight)
    {
        float currentWeight = rigLayerWeaponAiming.weight;
        float duration = 0.2f; // ���� �ð� (���� ����)
        float elapsed = 0f;

        while (elapsed < duration)
        {
            rigLayerWeaponAiming.weight = Mathf.Lerp(currentWeight, targetWeight, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rigLayerWeaponAiming.weight = targetWeight; // ��Ȯ�ϰ� ��ǥ weight�� ����
    }

    public void ApplyWeaponOffsets()
    {
        // ���� Ȱ��ȭ�� ���⸦ ������
        var activeWeapon = weaponComponent.GetActiveWeapon();

        if (activeWeapon != null)
        {
            // ������ ������ ���� MultiPositionConstraint�� ����
            if (weaponPoseConstraint != null)
            {
                // �ʱ�ȭ: Offset ���� Vector3.zero�� ����
                var poseData = weaponPoseConstraint.data;
                poseData.offset = Vector3.zero;
                weaponPoseConstraint.data = poseData;

                // ���⺰ ������ ������ ����
                poseData.offset = activeWeapon.weaponPoseOffset;
                weaponPoseConstraint.data = poseData;
            }

            if (weaponAimingConstraint != null)
            {
                // �ʱ�ȭ: Offset ���� Vector3.zero�� ����
                var aimingData = weaponAimingConstraint.data;
                aimingData.offset = Vector3.zero;
                weaponAimingConstraint.data = aimingData;

                // ���⺰ ������ ������ ����
                aimingData.offset = activeWeapon.weaponAimingOffset;
                weaponAimingConstraint.data = aimingData;
            }
        }
    }

    // �� �޼���� ���� ���� �� ��ü �� ȣ��Ǿ�� ��
    public void BuildRig()
    {
        rigBuilder.Build();
    }
}
