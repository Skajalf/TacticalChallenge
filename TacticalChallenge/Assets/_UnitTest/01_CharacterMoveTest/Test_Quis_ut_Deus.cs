using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Quis_ut_Deus : Test_WeaponBase
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Init()
    {
        base.Init();
    }

    protected override void Test_Attack()
    {
        base.Test_Attack();

        Fire();
    }

    public override void Test_Reload()
    {
        base.Test_Reload();

        // 재장전 중이 아니고, 탄약이 부족할 때만 재장전을 시도
        if (!IsReload && ammo < magazine)
        {
            StartCoroutine(ReloadCoroutine());
        }
        else
        {
            Debug.Log("재장전이 필요하지 않습니다.");
        }
    }

    public override void Test_Equip()
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

        base.Test_Equip();
        transform.SetParent(weaponTransform, false);
        gameObject.SetActive(true);

    }

    public override void Test_UnEquip()
    {
        base.Test_UnEquip();
        transform.SetParent(null);
        //gameObject.SetActive(false);
    }

    protected override void Test_Impulse()
    {
        base.Test_Impulse();
    }

    protected override void Test_Sound()
    {
        base.Test_Sound();

    }

    public override void AmmoLeft()
    {
        base.AmmoLeft();

        if (ammo > 0)
        {
            Test_Attack();
        }
        else
        {
            Test_Reload();
        }
    }

    private void Fire()
    {
        ammo--;

        WeaponUtility.Fire(transform, bulletTransform, range, damageDelay, power, hitLayerMask, this);
    }

    private IEnumerator ReloadCoroutine()
    {
        IsReload = true;

        // 재장전 사운드나 파티클 이펙트 호출 (추가 효과)
        Test_Sound();
        Test_Particle();

        // 재장전 시간 대기
        yield return new WaitForSeconds(reloadTime);

        ammo = magazine; // 탄약을 가득 채움

        // 재장전 완료 후 사운드/효과 처리 (선택 사항)
        Debug.Log("재장전 완료!");

        IsReload = false;
    }
}
