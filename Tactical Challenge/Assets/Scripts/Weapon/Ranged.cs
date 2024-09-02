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

    protected CinemachineImpulseSource impulse;
    protected CinemachineBrain brain;


    protected override void Awake()
    {
        base.Awake();

        impulse = GetComponent<CinemachineImpulseSource>();
        brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    private void FireProjectile() // 총알을 발사함
    {
        // 발사체를 발사 위치에서 생성
        GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        if (projectileInstance == null) // 없으면 실행이 안되도록 함.
            return;

        // 발사체가 `Projectile` 스크립트를 가지고 있는지 확인
        Projectile projectile = projectileInstance.GetComponent<Projectile>();
        if (projectile == null)
                projectileInstance.AddComponent<Projectile>();

        projectile.owner = this;

        weapondata.currentAmmo--;
    }

    private void OnProjectileHit(Collider projectileCollider, Collider hitCollider, Vector3 hitPoint)
    {
        // 충돌한 오브젝트에 대한 데미지 처리
        IDamagable damageable = hitCollider.GetComponent<IDamagable>();
        StatComponent stat = hitCollider.GetComponent<StatComponent>();
        float damageAmount = weapondata.Power;

        // `IDamagable.OnDamage` 호출

        // 충돌 이펙트 실행
        //PlayHitEffect(hitPoint);
    }

    public override void Begin_DoAction()
    {
        muzzlePosition = muzzleTransform.position;

        GameObject obj = Instantiate<GameObject>(projectilePrefab, muzzlePosition, rootObject.transform.rotation);
        Projectile projectile = obj.GetComponent<Projectile>();
        projectile.owner = this;

        obj.SetActive(true);
    }
}
