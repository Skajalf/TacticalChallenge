using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Quis_ut_Deus : Test_WeaponBase
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Init()
    {
        base.Init();
    }

    protected override void Test_Attack()
    {
        base.Test_Attack();

        Fire();
    }

    public override void Test_Reload()
    {
        base.Test_Reload();

        // ������ ���� �ƴϰ�, ź���� ������ ���� �������� �õ�
        if (!IsReload && ammo < magazine)
        {
            StartCoroutine(ReloadCoroutine());
        }
        else
        {
            Debug.Log("�������� �ʿ����� �ʽ��ϴ�.");
        }
    }

    public override void Test_Equip()
    {
        if (weaponTransform == null)
        {
            weaponTransform = transform.root.FindChildByName(weaponHolsterName);
            if (weaponTransform == null)
            {
                Debug.LogError($"���� Ȧ���͸� ã�� �� �����ϴ�: {weaponHolsterName}");
                return;
            }
        }

        if (bulletTransform == null)
        {
            bulletTransform = weaponTransform.FindChildByName(bulletTransformName);
            if (bulletTransform == null)
            {
                Debug.LogError($"źȯ �߻� ��ġ�� ã�� �� �����ϴ�: {bulletTransformName}");
                return;
            }
        }

        if (cartrigePoint == null)
        {
            cartrigePoint = weaponTransform.FindChildByName(cartridgeTransformName);
            if (cartrigePoint == null)
            {
                Debug.LogError($"ź�� �߻� ��ġ�� ã�� �� �����ϴ�: {cartridgeTransformName}");
                return;
            }
        }

        base.Test_Equip();
        transform.SetParent(weaponTransform, false);
        gameObject.SetActive(true);

    }

    public override void Test_UnEquip()
    {
        base.Test_UnEquip();
        transform.SetParent(null);
        //gameObject.SetActive(false);
    }

    protected override void Test_Impulse()
    {
        base.Test_Impulse();
    }

    protected override void Test_Sound()
    {
        base.Test_Sound();

    }

    public override void AmmoLeft()
    {
        base.AmmoLeft();

        if (ammo > 0)
        {
            Test_Attack();
        }
        else
        {
            Test_Reload();
        }
    }

    private void Fire()
    {
        ammo--;

        WeaponUtility.Fire(transform, bulletTransform, range, damageDelay, power, hitLayerMask, this);
    }

    private IEnumerator ReloadCoroutine()
    {
        IsReload = true;

        // ������ ���峪 ��ƼŬ ����Ʈ ȣ�� (�߰� ȿ��)
        Test_Sound();
        Test_Particle();

        // ������ �ð� ���
        yield return new WaitForSeconds(reloadTime);

        ammo = magazine; // ź���� ���� ä��

        // ������ �Ϸ� �� ����/ȿ�� ó�� (���� ����)
        Debug.Log("������ �Ϸ�!");

        IsReload = false;
    }
}
