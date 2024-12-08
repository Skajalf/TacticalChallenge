using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Quis_ut_Deus : Test_WeaponBase
{
    protected override void Awake()
    {
        base.Awake();
        IsFiring = false;
    }

    protected override void Init()
    {
        base.Init();
    }

    protected override void Test_Attack()
    {
        // �߻� ���̶�� �ߺ� �߻� ����
        if (IsFiring || IsReload || ammo <= 0)
        {
            Debug.Log("�߻� ���̰ų� ź���� �����ϰų� ������ ���Դϴ�.");
            return;
        }

        base.Test_Attack();
        StartCoroutine(FireCoroutine());  // �ڷ�ƾ ȣ��
    }

    public override void Test_Reload()
    {
        base.Test_Reload();

        animator = FindAnyObjectByType<Test_WeaponComponent>().GetComponent<Animator>();
        // TODO :: ��Ʈ ������Ʈ�� ��Ʈ (CHxxxx_Weapon�� ��Ʈ�� Weapon_pivot, Weapon_pivot�� ��Ʈ��
        // CHxxxx�� �ִϸ����͸� ã�ƿ��� �ڵ带 init() �κп� �߰��ؾ� �Ѵ�.) �� ã�� ����� ã��.

        if (animator == null)
        {
            Debug.LogError("Animator�� �������� �ʾҽ��ϴ�. Animator ������Ʈ�� �ùٸ��� ����Ǿ� �ִ��� Ȯ���ϼ���.");
            return;
        }
        

        if (!IsReload && ammo < magazine)
        {
            animator.SetTrigger("Reload");
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

        if (ammo > 0 && !IsReload)
        {
            Test_Attack(); // ź���� �������� ��� �߻� �õ�
        }
        else
        {
            Test_Reload(); // ź���� ������ ��� ������ �õ�
        }
    }

    private void Fire()
    {
        ammo--;

        WeaponUtility.Fire(transform, bulletTransform, range, damageDelay, power, hitLayerMask, this);

        // ����ü �߻� ó��
        FireProjectile();
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || bulletTransform == null)
        {
            Debug.LogWarning("����ü ������ �Ǵ� �߻� ��ġ�� �������� �ʾҽ��ϴ�.");
            return;
        }

        // ����ü �ν��Ͻ� ����
        var projectileInstance = Instantiate(projectilePrefab, bulletTransform.position, bulletTransform.rotation);

        // ������ ����ü���� Test_Projectile ��ũ��Ʈ�� ������
        var projectile = projectileInstance.GetComponent<Test_Projectile>();

        if (projectile != null)
        {
            // ���� ���� ����
            projectile.weapon = this;
        }
    }

    private IEnumerator FireCoroutine()
    {
        IsFiring = true;  // �߻� ����
        Fire();           // �߻� ����

        // �ִϸ��̼� ��� �ð���ŭ ���
        yield return new WaitForSeconds(animationWaitTime);

        // �߻簡 �Ϸ�Ǿ����Ƿ� �߻� �� ���� �ʱ�ȭ
        IsFiring = false;
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

    private void OnDrawGizmos()
    {
        if (bulletTransform == null) return; // �Ѿ� �߻� ��ġ�� �������� �ʾҴٸ� �׸��� ����

        // �߻� ���� ���
        Vector3 fireDirection = bulletTransform.forward;
        Vector3 startPoint = bulletTransform.position;

        // Gizmo ���� ���� (������)
        Gizmos.color = Color.red;

        // �ѱ� ��ġ���� �߻� �������� range��ŭ ���� �׸���
        Gizmos.DrawRay(startPoint, fireDirection * range);
    }
}
