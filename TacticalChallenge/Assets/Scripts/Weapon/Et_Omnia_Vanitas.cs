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
            Debug.Log("[Et_Omnia_Vanitas] 발사 불가: IsFiring or IsReload");
            return;
        }

        if (IsEmpty)
        {
            Debug.Log("[Et_Omnia_Vanitas] 탄약 없음 - 재장전 시도");
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
            Debug.LogError($"[{name}] Reload 호출했지만 MagazineSize가 0입니다. Inspector 설정을 확인하세요.");
            return;
        }

        if (IsReload)
        {
            Debug.Log("[Et_Omnia_Vanitas] 이미 재장전 중.");
            return;
        }

        if (CurrentAmmo >= MagazineSize)
        {
            Debug.Log("[Et_Omnia_Vanitas] 이미 탄창이 가득합니다.");
            return;
        }

        StartCoroutine(ReloadCoroutine());
    }

    public override void Equip()
    {
        base.Equip();

        weaponTransform = this.transform;

        // 먼저 직계로 찾고, 없으면 재귀 탐색
        bulletTransform = weaponTransform.Find(bulletTransformName) ?? weaponTransform.FindChildByName(bulletTransformName);
        if (bulletTransform == null)
        {
            Debug.LogError($"[{name}] 탄환 발사 위치를 찾을 수 없습니다: {bulletTransformName}");
            return;
        }

        cartridgePoint = weaponTransform.Find(cartridgeTransformName) ?? weaponTransform.FindChildByName(cartridgeTransformName);
        if (cartridgePoint == null)
        {
            Debug.LogWarning($"[{name}] 탄피 발사 위치를 찾을 수 없습니다: {cartridgeTransformName} (없어도 동작 가능할 수 있음)");
        }

        gameObject.SetActive(true);
        Debug.Log($"[{name}] Equip 완료. bullet:{bulletTransform.name}, cartridge:{(cartridgePoint != null ? cartridgePoint.name : "null")}");
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
            Debug.LogError($"[{name}] MagazineSize가 0이므로 동작을 중단합니다. (설정오류)");
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
            Debug.LogWarning("[Et_Omnia_Vanitas] Fire 호출했으나 탄약 부족");
            return;
        }

        // 실제 공격 처리 (데미지/레이캐스트 등)
        WeaponUtility.Fire(transform, bulletTransform, range.Value, damageDelay.Value, power.Value, hitLayerMask, this);

        // 투사체(프리팹) 생성
        FireProjectile();

        // 탄피 이펙트 등
        if (cartridgeParticle != null && cartridgePoint != null)
        {
            Instantiate(cartridgeParticle, cartridgePoint.position, cartridgePoint.rotation);
        }

        // 화염 이펙트
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
            Debug.LogWarning("투사체 프리팹 또는 발사 위치가 설정되지 않았습니다.");
            return;
        }

        // 투사체 인스턴스 생성
        var projectileInstance = Instantiate(projectilePrefab, bulletTransform.position, bulletTransform.rotation);

        // 생성된 투사체에서 Test_Projectile 스크립트를 가져옴
        var projectile = projectileInstance.GetComponent<Projectile>();

        if (projectile != null)
        {
            // 무기 정보 전달
            projectile.weapon = this;
        }
    }

    private IEnumerator FireCoroutine()
    {
        Debug.Log("[FireCoroutine] started");

        IsFiring = true;  // 발사 시작
        Fire();           // 발사 실행

        // 애니메이션 재생 시간만큼 대기
        yield return new WaitForSeconds(animationWaitTime);
        Debug.Log("[FireCoroutine] after wait - 총알 발사중.");

        // 발사가 완료되었으므로 발사 중 상태 초기화
        IsFiring = false;
    }

    private IEnumerator ReloadCoroutine()
    {
        IsReload = true;

        // 재장전 사운드나 파티클 이펙트 호출 (추가 효과)
        Sound();
        Particle();

        // 재장전 시간 대기
        yield return new WaitForSeconds(reloadTime != null ? reloadTime.Value : 1.0f);

        CurrentAmmo = MagazineSize;

        // 재장전 완료 후 사운드/효과 처리 (선택 사항)
        Debug.Log("재장전 완료!");

        IsReload = false;
    }

    private void OnDrawGizmos()
    {
        if (bulletTransform == null) return; // 총알 발사 위치가 설정되지 않았다면 그리지 않음

        // 발사 방향 계산
        Vector3 fireDirection = bulletTransform.forward;
        Vector3 startPoint = bulletTransform.position;

        // Gizmo 색상 설정 (빨간색)
        Gizmos.color = Color.red;

        // 총구 위치에서 발사 방향으로 range만큼 라인 그리기
        Gizmos.DrawRay(startPoint, fireDirection * range.Value);
    }
}
