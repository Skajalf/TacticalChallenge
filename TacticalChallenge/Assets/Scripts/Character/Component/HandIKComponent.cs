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
        // WeaponComponent �� �ֿ� IK ���� ������Ʈ�� ã��
        weaponComponent = GetComponent<WeaponComponent>();
        weaponPivot = transform.FindChildByName("WeaponPivot");
        rigLayerHandIK = transform.FindChildByName("RigLayer_HandIK");
        rigBuilder = GetComponent<RigBuilder>(); // RigBuilder �ʱ�ȭ

        if (rigLayerHandIK != null)
        {
            leftHandIK = rigLayerHandIK.FindChildByName("LeftHandIK")?.GetComponent<TwoBoneIKConstraint>();
            rightHandIK = rigLayerHandIK.FindChildByName("RightHandIK")?.GetComponent<TwoBoneIKConstraint>();
        }
        else
        {
            Debug.LogError("RigLayer_HandIK�� ã�� �� �����ϴ�.");
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

    // �� �޼���� ���� ���� �� ��ü �� ȣ��Ǿ�� ��
    public void BuildRig()
    {
        rigBuilder.Build();
    }
}
