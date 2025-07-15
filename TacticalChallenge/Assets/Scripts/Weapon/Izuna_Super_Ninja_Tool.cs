using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Izuna_Super_Ninja_Tool : WeaponBase
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

    protected override void Attack()
    {
        // �߻� ���̶�� �ߺ� �߻� ����
        if (IsFiring || IsReload || ammo.Value <= 0)
        {
            Debug.Log("�߻� ���̰ų� ź���� �����ϰų� ������ ���Դϴ�.");
            return;
        }

        base.Attack();
        StartCoroutine(FireCoroutine());  // �ڷ�ƾ ȣ��
    }

    public override void Reload()
    {
        base.Reload();

        animator = FindAnyObjectByType<WeaponComponent>().GetComponent<Animator>();
        // TODO :: ��Ʈ ������Ʈ�� ��Ʈ (CHxxxx_Weapon�� ��Ʈ�� Weapon_pivot, Weapon_pivot�� ��Ʈ��
        // CHxxxx�� �ִϸ����͸� ã�ƿ��� �ڵ带 init() �κп� �߰��ؾ� �Ѵ�.) �� ã�� ����� ã��.

        if (animator == null)
        {
            Debug.LogError("Animator�� �������� �ʾҽ��ϴ�. Animator ������Ʈ�� �ùٸ��� ����Ǿ� �ִ��� Ȯ���ϼ���.");
            return;
        }


        if (!IsReload && ammo.Value < megazine.Value)
        {
            animator.SetTrigger("Reload");
            StartCoroutine(ReloadCoroutine());
        }
        else
        {
            Debug.Log("�������� �ʿ����� �ʽ��ϴ�.");
        }
    }


    public override void Equip()
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

        base.Equip();
        transform.SetParent(weaponTransform, false);
        gameObject.SetActive(true);

    }

    public override void UnEquip()
    {
        base.UnEquip();
        transform.SetParent(null);
        //gameObject.SetActive(false);
    }

    protected override void Impulse()
    {
        base.Impulse();
    }

    protected override void Sound()
    {
        base.Sound();

    }

    public override void AmmoLeft()
    {
        base.AmmoLeft();

        if (ammo.Value > 0 && !IsReload)
        {
            Attack(); // ź���� �������� ��� �߻� �õ�
        }
        else
        {
            Reload(); // ź���� ������ ��� ������ �õ�
        }
    }

    private void Fire()
    {
        ammo.DefaultValue--;

        WeaponUtility.Fire(transform, bulletTransform, range.Value, damageDelay.Value, power.Value, hitLayerMask, this);

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
        var projectile = projectileInstance.GetComponent<Projectile>();

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
        Sound();
        Particle();

        // ������ �ð� ���
        yield return new WaitForSeconds(reloadTime.Value);

        ammo = megazine; // ź���� ���� ä��

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
        Gizmos.DrawRay(startPoint, fireDirection * range.Value);
    }
}
