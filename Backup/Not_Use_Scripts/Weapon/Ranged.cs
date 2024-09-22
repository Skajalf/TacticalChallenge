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

        FireProjectile(); // 발사체 발사
        Particle();       // 총구 파티클
        CartrigeDrop();   // 탄피 배출
    }

    public override void EndDoAction()
    {
        base.EndDoAction();
    }

    private void FireProjectile()
    {
        // 발사체 생성 및 초기화
        var projectileInstance = Instantiate(weapondata.projectilePrefab, weapondata.firePoint.position, weapondata.firePoint.rotation);
        var projectile = projectileInstance.GetComponent<Projectile>();
        projectile.owner = this;

        weapondata.currentAmmo--; // 발사 후 탄약 감소
    }

    public override void Reload()
    {
        base.Reload();

        if (weapondata.currentAmmo < weapondata.Ammo)  // 잔탄이 부족할 때만 재장전
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

    public override void makeImpulse()
    {
        base.makeImpulse();
        Debug.Log("makeImpulse called");
        FireRecoil();
    }

    private IEnumerator ReloadCoroutine()  // 재장전 코루틴
    {
        IsReload = true;
        yield return new WaitForSeconds(weapondata.ReloadTime);  // 재장전 시간 대기
        weapondata.currentAmmo = weapondata.Ammo;  // 잔탄 충전
        IsReload = false;
    }

    private void Particle()
    {
        if (weapondata.Particle != null)
        {
            Instantiate(weapondata.Particle, weapondata.firePoint.position, weapondata.firePoint.rotation);
        }
    }

    private void CartrigeDrop() //탄피 떨구기
    {
        if (weapondata.CartrigeCase != null)
        {
            Instantiate(weapondata.CartrigeCase, weapondata.firePoint.position, Quaternion.identity);
        }
    }

    public void FireRecoil()
    {
        if (impulse != null)
        {
            // 방향 크기에 따라 진폭 설정
            impulse.m_ImpulseDefinition.m_AmplitudeGain = weapondata.ImpulseDirection.magnitude;

            // 임펄스 발생
            impulse.GenerateImpulse(weapondata.ImpulseDirection);
            Debug.Log("Impulse generated with direction: " + weapondata.ImpulseDirection);
        }
        else
        {
            Debug.LogWarning("CinemachineImpulseSource is not assigned.");
        }
    }


}
