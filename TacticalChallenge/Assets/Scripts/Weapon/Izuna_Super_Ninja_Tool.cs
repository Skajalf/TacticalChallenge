using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Izuna_Super_Ninja_Tool : WeaponBase
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
        // 발사 중이라면 중복 발사 방지
        if (IsFiring || IsReload || ammo.Value <= 0)
        {
            Debug.Log("발사 중이거나 탄약이 부족하거나 재장전 중입니다.");
            return;
        }

        base.Attack();
        StartCoroutine(FireCoroutine());  // 코루틴 호출
    }

    public override void Reload()
    {
        base.Reload();

        animator = FindAnyObjectByType<WeaponComponent>().GetComponent<Animator>();
        // TODO :: 루트 오브젝트의 루트 (CHxxxx_Weapon의 루트는 Weapon_pivot, Weapon_pivot의 루트인
        // CHxxxx의 애니메이터를 찾아오는 코드를 init() 부분에 추가해야 한다.) 를 찾는 방법을 찾기.

        if (animator == null)
        {
            Debug.LogError("Animator가 설정되지 않았습니다. Animator 컴포넌트가 올바르게 연결되어 있는지 확인하세요.");
            return;
        }


        if (!IsReload && ammo.Value < megazine.Value)
        {
            animator.SetTrigger("Reload");
            StartCoroutine(ReloadCoroutine());
        }
        else
        {
            Debug.Log("재장전이 필요하지 않습니다.");
        }
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
            cartrigePoint = weaponTransform.FindChildByName(cartridgeTransformName);
            if (cartrigePoint == null)
            {
                Debug.LogError($"탄피 발사 위치를 찾을 수 없습니다: {cartridgeTransformName}");
                return;
            }
        }

        base.Equip();
        transform.SetParent(weaponTransform, false);
        gameObject.SetActive(true);

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

        if (ammo.Value > 0 && !IsReload)
        {
            Attack(); // 탄약이 남아있을 경우 발사 시도
        }
        else
        {
            Reload(); // 탄약이 부족할 경우 재장전 시도
        }
    }

    private void Fire()
    {
        ammo.DefaultValue--;

        WeaponUtility.Fire(transform, bulletTransform, range.Value, damageDelay.Value, power.Value, hitLayerMask, this);

        // 투사체 발사 처리
        FireProjectile();
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
        IsFiring = true;  // 발사 시작
        Fire();           // 발사 실행

        // 애니메이션 재생 시간만큼 대기
        yield return new WaitForSeconds(animationWaitTime);

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
        yield return new WaitForSeconds(reloadTime.Value);

        ammo = megazine; // 탄약을 가득 채움

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
