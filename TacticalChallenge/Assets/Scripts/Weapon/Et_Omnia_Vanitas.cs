using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Et_Omnia_Vanitas : WeaponBase
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
        Debug.Log($"[Attack] Enter. CurrentAmmo={CurrentAmmo} IsFiring={IsFiring} IsReload={IsReload} IsEmpty={IsEmpty}");

        if (IsFiring || IsReload)
        {
            Debug.Log("[Et_Omnia_Vanitas] �߻� �Ұ�: IsFiring or IsReload");
            return;
        }

        if (IsEmpty)
        {
            Debug.Log("[Et_Omnia_Vanitas] ź�� ���� - ������ �õ�");
            Reload();
            return;
        }

        base.Attack();
        StartCoroutine(FireCoroutine());
    }

    public override void Reload()
    {
        base.Reload();

        if (MagazineSize <= 0)
        {
            Debug.LogError($"[{name}] Reload ȣ�������� MagazineSize�� 0�Դϴ�. Inspector ������ Ȯ���ϼ���.");
            return;
        }

        if (IsReload)
        {
            Debug.Log("[Et_Omnia_Vanitas] �̹� ������ ��.");
            return;
        }

        if (CurrentAmmo >= MagazineSize)
        {
            Debug.Log("[Et_Omnia_Vanitas] �̹� źâ�� �����մϴ�.");
            return;
        }

        StartCoroutine(ReloadCoroutine());
    }

    public override void Equip()
    {
        base.Equip();

        weaponTransform = this.transform;

        // ���� ����� ã��, ������ ��� Ž��
        bulletTransform = weaponTransform.Find(bulletTransformName) ?? weaponTransform.FindChildByName(bulletTransformName);
        if (bulletTransform == null)
        {
            Debug.LogError($"[{name}] źȯ �߻� ��ġ�� ã�� �� �����ϴ�: {bulletTransformName}");
            return;
        }

        cartridgePoint = weaponTransform.Find(cartridgeTransformName) ?? weaponTransform.FindChildByName(cartridgeTransformName);
        if (cartridgePoint == null)
        {
            Debug.LogWarning($"[{name}] ź�� �߻� ��ġ�� ã�� �� �����ϴ�: {cartridgeTransformName} (��� ���� ������ �� ����)");
        }

        gameObject.SetActive(true);
        Debug.Log($"[{name}] Equip �Ϸ�. bullet:{bulletTransform.name}, cartridge:{(cartridgePoint != null ? cartridgePoint.name : "null")}");
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

        Debug.Log($"[AmmoLeft] CurrentAmmo={CurrentAmmo} MagazineSize={MagazineSize} IsReload={IsReload} IsFiring={IsFiring}");

        if (MagazineSize <= 0)
        {
            Debug.LogError($"[{name}] MagazineSize�� 0�̹Ƿ� ������ �ߴ��մϴ�. (��������)");
            return;
        }

        if (CurrentAmmo > 0 && !IsReload)
        {
            Attack();
        }
        else
        {
            Reload();
        }
    }

    private void Fire()
    {
        bool used = AmmoUse(1);
        if (!used)
        {
            Debug.LogWarning("[Et_Omnia_Vanitas] Fire ȣ�������� ź�� ����");
            return;
        }

        // ���� ���� ó�� (������/����ĳ��Ʈ ��)
        WeaponUtility.Fire(transform, bulletTransform, range.Value, damageDelay.Value, power.Value, hitLayerMask, this);

        // ����ü(������) ����
        FireProjectile();

        // ź�� ����Ʈ ��
        if (cartridgeParticle != null && cartridgePoint != null)
        {
            Instantiate(cartridgeParticle, cartridgePoint.position, cartridgePoint.rotation);
        }

        // ȭ�� ����Ʈ
        if (flameParticle != null && bulletTransform != null)
        {
            Instantiate(flameParticle, bulletTransform.position, bulletTransform.rotation);
        }

        Debug.Log($"[Et_Omnia_Vanitas] Fired. Remaining {CurrentAmmo}/{MagazineSize}");
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
        Debug.Log("[FireCoroutine] started");

        IsFiring = true;  // �߻� ����
        Fire();           // �߻� ����

        // �ִϸ��̼� ��� �ð���ŭ ���
        yield return new WaitForSeconds(animationWaitTime);
        Debug.Log("[FireCoroutine] after wait - �Ѿ� �߻���.");

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
        yield return new WaitForSeconds(reloadTime != null ? reloadTime.Value : 1.0f);

        CurrentAmmo = MagazineSize;

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
