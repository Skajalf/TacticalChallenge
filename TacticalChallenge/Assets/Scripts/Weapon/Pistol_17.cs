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
                Debug.LogError($"무기 홀스터를 찾을 수 없습니다: {weaponHolsterName}");
                return;
            }
        }

        if (bulletTransform == null)
        {
            bulletTransform = weaponTransform.FindChildByName(bulletTransformName);
            if (bulletTransform == null)
            {
                Debug.LogError($"탄환 발사 위치를 찾을 수 없습니다: {bulletTransformName}");
                return;
            }
        }

        if (cartrigePoint == null)
        {
            cartrigePoint = weaponTransform.FindChildByName(cartrigeTransformName);
            if (cartrigePoint == null)
            {
                Debug.LogError($"탄피 발사 위치를 찾을 수 없습니다: {cartrigeTransformName}");
                return;
            }
        }

        base.Equip();
        transform.SetParent(weaponTransform, false);
        transform.localPosition = Vector3.zero; // 로컬 위치 초기화
        transform.localRotation = Quaternion.identity; // 로컬 회전 초기화
        gameObject.SetActive(true);
    }

    public override void UnEquip()
    {
        base.UnEquip();
        transform.SetParent(null);
        //transform.localPosition = Vector3.zero; // 필요에 따라 초기화
        //transform.localRotation = Quaternion.identity; // 필요에 따라 초기화
        //gameObject.SetActive(false);
    }
}
