using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Quis_ut_Deus : Test_WeaponBase
{
    private Coroutine damageCoroutine;  // �ڷ�ƾ �ڵ�

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
        // ��Ʈ��ĵ ����ĳ��Ʈ ���� �� ���� �ִٸ� �ڷ�ƾ���� ������ ó��
        RaycastHit hit;
        Vector3 origin = bulletTransform.position;
        Vector3 direction = bulletTransform.forward;

        if (Physics.Raycast(origin, direction, out hit, range, hitLayerMask))
        {
            Debug.Log($"{hit.collider.gameObject.name}�� ��� ���� Ÿ���� �����մϴ�.");

            // �̹� ������ �ڷ�ƾ�� ���� ���̶�� ����
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
            }

            // ���� �ð� ���� Ÿ���� �����Ǹ� ������ ����
            damageCoroutine = StartCoroutine(DamageCoroutine(hit.collider));
        }
    }

    private IEnumerator DamageCoroutine(Collider target)
    {
        float elapsedTime = 0f;
        bool targetHit = true;

        // ������ �ð� ���� ���������� ���� ����ĳ��Ʈ ���� �ִ��� Ȯ��
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

        // ���� �ð� ���� ����� ����ĳ��Ʈ ���� �־��ٸ� ������ ����
        if (targetHit)
        {
            //IDamageable damageable = target.GetComponent<IDamageable>();
            //if (damageable != null)
            //{
            //    damageable.TakeDamage(damage);
            //    Debug.Log($"{target.gameObject.name}�� {damage}�� �������� �������ϴ�.");
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
                Debug.LogError($"���� Ȧ���͸� ã�� �� �����ϴ�: {weaponHolsterName}");
                return;
            }
        }

        if (bulletTransform == null)
        {
            bulletTransform = weaponTransform.FindChildByName(bulletTransformName);
            if (bulletTransform == null)
            {
                Debug.LogError($"źȯ �߻� ��ġ�� ã�� �� �����ϴ�: {bulletTransformName}");
                return;
            }
        }

        if (cartrigePoint == null)
        {
            cartrigePoint = weaponTransform.FindChildByName(cartridgeTransformName);
            if (cartrigePoint == null)
            {
                Debug.LogError($"ź�� �߻� ��ġ�� ã�� �� �����ϴ�: {cartridgeTransformName}");
                return;
            }
        }

        base.Test_Equip();
        transform.SetParent(weaponTransform, false);
        //transform.localPosition = Vector3.zero; // ���� ��ġ �ʱ�ȭ
        //transform.localRotation = Quaternion.identity; // ���� ȸ�� �ʱ�ȭ
        gameObject.SetActive(true);
    }

    public override void Test_UnEquip()
    {
        base.Test_UnEquip();
        transform.SetParent(null);
        //transform.localPosition = Vector3.zero; // �ʿ信 ���� �ʱ�ȭ
        //transform.localRotation = Quaternion.identity; // �ʿ信 ���� �ʱ�ȭ
        //gameObject.SetActive(false);
    }
}
