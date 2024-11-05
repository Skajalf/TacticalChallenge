using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Quis_ut_Deus : Test_WeaponBase
{
    private Coroutine damageCoroutine;  // 코루틴 핸들

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Init()
    {
        base.Init();
    }

    public override void Test_Attack()
    {
        base.Test_Attack();

        FireHitScan();
        Test_Particle();
        //CartrigeDrop();
    }

    private void FireHitScan()
    {
        // 히트스캔 레이캐스트 실행 후 적이 있다면 코루틴으로 데미지 처리
        RaycastHit hit;
        Vector3 origin = bulletTransform.position;
        Vector3 direction = bulletTransform.forward;

        if (Physics.Raycast(origin, direction, out hit, range, hitLayerMask))
        {
            Debug.Log($"{hit.collider.gameObject.name}에 잠시 동안 타격을 유지합니다.");

            // 이미 데미지 코루틴이 실행 중이라면 중지
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
            }

            // 일정 시간 동안 타격이 유지되면 데미지 적용
            damageCoroutine = StartCoroutine(DamageCoroutine(hit.collider));
        }
    }

    private IEnumerator DamageCoroutine(Collider target)
    {
        float elapsedTime = 0f;
        bool targetHit = true;

        // 지정된 시간 동안 지속적으로 적이 레이캐스트 내에 있는지 확인
        while (elapsedTime < damageDelay)
        {
            RaycastHit hitCheck;
            Vector3 origin = bulletTransform.position;
            Vector3 direction = bulletTransform.forward;

            if (Physics.Raycast(origin, direction, out hitCheck, range, hitLayerMask))
            {
                if (hitCheck.collider != target)
                {
                    targetHit = false;
                    break;
                }
            }
            else
            {
                targetHit = false;
                break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 지연 시간 동안 대상이 레이캐스트 내에 있었다면 데미지 적용
        if (targetHit)
        {
            //IDamageable damageable = target.GetComponent<IDamageable>();
            //if (damageable != null)
            //{
            //    damageable.TakeDamage(damage);
            //    Debug.Log($"{target.gameObject.name}에 {damage}의 데미지를 입혔습니다.");
            //}
        }

        damageCoroutine = null;
    }

    public override void Test_Reload()
    {
        base.Test_Reload();

        if (ammo < magazine)
        {
            StartCoroutine(ReloadCoroutine());
        }
    }

    //public override void CheckAmmo()
    //{
    //    base.CheckAmmo();

    //    if (ammo > 0)
    //    {
    //        Test_Attack();
    //    }
    //    else
    //    {
    //        Test_Reload();
    //    }
    //}

    public override void Test_Impulse()
    {
        base.Test_Impulse();
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
        //IsReload = true;
        yield return new WaitForSeconds(reloadTime);
        ammo = magazine;
        //IsReload = false;
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
        //transform.localPosition = Vector3.zero; // 로컬 위치 초기화
        //transform.localRotation = Quaternion.identity; // 로컬 회전 초기화
        gameObject.SetActive(true);
    }

    public override void Test_UnEquip()
    {
        base.Test_UnEquip();
        transform.SetParent(null);
        //transform.localPosition = Vector3.zero; // 필요에 따라 초기화
        //transform.localRotation = Quaternion.identity; // 필요에 따라 초기화
        //gameObject.SetActive(false);
    }
}
