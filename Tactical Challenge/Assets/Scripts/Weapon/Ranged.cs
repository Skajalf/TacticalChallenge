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

    private void FireProjectile() // �Ѿ��� �߻���
    {
        // �߻�ü�� �߻� ��ġ���� ����
        GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        if (projectileInstance == null) // ������ ������ �ȵǵ��� ��.
            return;

        // �߻�ü�� `Projectile` ��ũ��Ʈ�� ������ �ִ��� Ȯ��
        Projectile projectile = projectileInstance.GetComponent<Projectile>();
        if (projectile == null)
                projectileInstance.AddComponent<Projectile>();

        projectile.owner = this;

        weapondata.currentAmmo--;
    }

    private void OnProjectileHit(Collider projectileCollider, Collider hitCollider, Vector3 hitPoint)
    {
        // �浹�� ������Ʈ�� ���� ������ ó��
        IDamagable damageable = hitCollider.GetComponent<IDamagable>();
        StatComponent stat = hitCollider.GetComponent<StatComponent>();
        float damageAmount = weapondata.Power;

        // `IDamagable.OnDamage` ȣ��

        // �浹 ����Ʈ ����
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
