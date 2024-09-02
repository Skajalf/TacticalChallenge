using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;

public class Ranged : Weapon
{
    [SerializeField] protected GameObject projectilePrefab; // 발사체 프리팹
    [SerializeField] private Transform firePoint;         // 발사 위치

    private bool isReloading = false;

    protected CinemachineImpulseSource impulse;
    protected CinemachineBrain brain;

    private MovingComponent moving;
    private WeaponData data;

    protected override void Awake()
    {
        base.Awake();

        impulse = GetComponent<CinemachineImpulseSource>();
        brain = Camera.main.GetComponent<CinemachineBrain>();

        moving = rootObject.GetComponent<MovingComponent>();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    private void FireProjectile()
    {
        // 발사체를 발사 위치에서 생성
        GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // 발사체가 `Projectile` 스크립트를 가지고 있는지 확인
        Projectile projectile = projectileInstance.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.OnProjectileHit += OnProjectileHit;
        }

        weapondata.currentAmmo--;
    }

    private void OnProjectileHit(Collider projectileCollider, Collider hitCollider, Vector3 hitPoint)
    {
        // 충돌한 오브젝트에 대한 데미지 처리
        IDamagable damageable = hitCollider.GetComponent<IDamagable>();
        if (damageable != null)
        {
            damageable.OnDamage(rootObject, this, hitPoint, weapondata);
        }
        else
        {
            if (weapondata.HitParticle != null)
                Instantiate<GameObject>(weapondata.HitParticle, hitPoint, rootObject.transform.rotation);
        }

        // 충돌 이펙트 실행
        PlayHitEffect(hitPoint);
    }

    private void PlayHitEffect(Vector3 hitPoint)
    {
        if (weapondata.HitParticle != null)
        {
            Instantiate(weapondata.HitParticle, hitPoint + weapondata.HitParticlePositionOffset, Quaternion.identity);
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(data.ReloadTime);

        weapondata.currentAmmo = weapondata.Ammo;
        isReloading = false;
    }
}
