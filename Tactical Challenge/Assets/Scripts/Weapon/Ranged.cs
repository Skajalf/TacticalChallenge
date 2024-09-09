using System.Collections;
using UnityEngine;

public class Ranged : Weapon
{

    protected override void Awake()
    {
        base.Awake();
    }

    public override void DoAction()
    {
        base.DoAction();

        FireProjectile(); // �߻�ü �߻�
        Particle();       // �ѱ� ��ƼŬ
        CartrigeDrop();   // ź�� ����
        FireRecoil();     // �ݵ� ó��
    }

    public override void EndDoAction()
    {
        base.EndDoAction();
    }

    private void FireProjectile()
    {
        // �߻�ü ���� �� �ʱ�ȭ
        var projectileInstance = Instantiate(weapondata.projectilePrefab, weapondata.firePoint.position, weapondata.firePoint.rotation);
        var projectile = projectileInstance.GetComponent<Projectile>();
        projectile.owner = this;

        weapondata.currentAmmo--; // �߻� �� ź�� ����
    }

    public override void Reload()
    {
        base.Reload();

        if (weapondata.currentAmmo < weapondata.Ammo)  // ��ź�� ������ ���� ������
        {
            StartCoroutine(ReloadCoroutine());
        }
    }

    public override void CheckAmmo()
    {
        base.CheckAmmo();

        if (weapondata.currentAmmo > 0)
        {
            DoAction();
        }
        else
        {
            Reload();
        }
    }

    private IEnumerator ReloadCoroutine()  // ������ �ڷ�ƾ
    {
        IsReload = true;
        yield return new WaitForSeconds(weapondata.ReloadTime);  // ������ �ð� ���
        weapondata.currentAmmo = weapondata.Ammo;  // ��ź ����
        IsReload = false;
    }

    private void Particle()
    {
        if (weapondata.Particle != null)
        {
            Instantiate(weapondata.Particle, weapondata.firePoint.position, weapondata.firePoint.rotation);
        }
    }

    private void CartrigeDrop() //ź�� ������
    {
        if (weapondata.CartrigeCase != null)
        {
            Instantiate(weapondata.CartrigeCase, weapondata.firePoint.position, Quaternion.identity);
        }
    }

    private void FireRecoil() // ī�޶� �ݵ��� �����ϴ� �ڵ�
    {
        if (impulse != null)
        {
            impulse.GenerateImpulse();
        }
    }
}
