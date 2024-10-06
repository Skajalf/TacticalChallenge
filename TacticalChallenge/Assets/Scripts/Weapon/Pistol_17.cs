using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol_17 : WeaponBase
{
    protected override void Awake()
    {
        base.Awake();

        Init();
    }

    protected override void Init()
    {
        base.Init();
    }

    public override void Action()
    {
        base.Action();

        FireProjectile();
        Particle();
        CartrigeDrop();
    }

    private void FireProjectile()
    {
        var projectileInstance = Instantiate(projectilePrefab, bulletTransform.position, bulletTransform.rotation);
        var projectile = projectileInstance.GetComponent<Projectile>();
        projectile.weapon = this;

        ammo--;
    }

    public override void Reload()
    {
        base.Reload();

        if (ammo < megazine)
        {
            StartCoroutine(ReloadCoroutine());
        }
    }

    public override void CheckAmmo()
    {
        base.CheckAmmo();

        if (ammo > 0)
        {
            Action();
        }
        else
        {
            Reload();
        }
    }

    public override void makeImpulse()
    {
        base.makeImpulse();
        Debug.Log("makeImpulse called");
        FireRecoil();
    }

    private void CartrigeDrop()
    {
        if (cartrigePoint != null)
        {
            Instantiate(cartrigePoint, bulletTransform.position, Quaternion.identity);
        }
    }

    public void FireRecoil()
    {
        if (impulse != null)
        {
            impulse.m_ImpulseDefinition.m_AmplitudeGain = impulseDirection.magnitude;

            impulse.GenerateImpulse(impulseDirection);
            Debug.Log("Impulse generated with direction: " + impulseDirection);
        }
        else
        {
            Debug.LogWarning("CinemachineImpulseSource is not assigned.");
        }
    }

    private IEnumerator ReloadCoroutine()
    {
        IsReload = true;
        yield return new WaitForSeconds(reloadTime);
        ammo = megazine;
        IsReload = false;
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
            cartrigePoint = weaponTransform.FindChildByName(cartrigeTransformName);
            if (cartrigePoint == null)
            {
                Debug.LogError($"ź�� �߻� ��ġ�� ã�� �� �����ϴ�: {cartrigeTransformName}");
                return;
            }
        }

        base.Equip();
        transform.SetParent(weaponTransform, false);
        transform.localPosition = Vector3.zero; // ���� ��ġ �ʱ�ȭ
        transform.localRotation = Quaternion.identity; // ���� ȸ�� �ʱ�ȭ
        gameObject.SetActive(true);
    }

    public override void UnEquip()
    {
        base.UnEquip();
        transform.SetParent(null);
        //transform.localPosition = Vector3.zero; // �ʿ信 ���� �ʱ�ȭ
        //transform.localRotation = Quaternion.identity; // �ʿ信 ���� �ʱ�ȭ
        //gameObject.SetActive(false);
    }
}
