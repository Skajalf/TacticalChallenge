using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;

public class Ranged : Weapon
{
    [SerializeField] protected GameObject projectilePrefab; // �߻�ü ������
    [SerializeField] private Transform firePoint;         // �߻� ��ġ

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
        // �߻�ü�� �߻� ��ġ���� ����
        GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // �߻�ü�� `Projectile` ��ũ��Ʈ�� ������ �ִ��� Ȯ��
        Projectile projectile = projectileInstance.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.OnProjectileHit += OnProjectileHit;
        }

        weapondata.currentAmmo--;
    }

    private void OnProjectileHit(Collider projectileCollider, Collider hitCollider, Vector3 hitPoint)
    {
        // �浹�� ������Ʈ�� ���� ������ ó��
        IDamagable damageable = hitCollider.GetComponent<IDamagable>();
        StatComponent stat = hitCollider.GetComponent<StatComponent>();
        float damageAmount = weapondata.Power;

        // `IDamagable.OnDamage` ȣ��
        damageable.OnDamage(rootObject, this, hitPoint, damage);

        // �浹 ����Ʈ ����
        //PlayHitEffect(hitPoint);
    }

    //private void PlayHitEffect(Vector3 hitPoint)
    //{
    //    if (weapondata.HitParticle != null)
    //    {
    //        Instantiate(weapondata.HitParticle, hitPoint + weapondata.HitParticlePositionOffset, Quaternion.identity);
    //    }
    //}

    private IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(data.ReloadTime);

        weapondata.currentAmmo = weapondata.Ammo;
        isReloading = false;
    }
}
