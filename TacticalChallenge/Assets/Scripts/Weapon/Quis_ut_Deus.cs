using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quis_ut_Deus : WeaponBase
{
    [Header(" Weapon Hitscan Settings")]
    [SerializeField] private float hitScanRange = 10.0f;  // ��Ʈ��ĵ ��Ÿ�
    [SerializeField] private float damage = 10.0f;         // ���� ������
    [SerializeField] private LayerMask hitLayerMask;       // Ÿ�� ��� ���̾� ����
    [SerializeField] private float damageDelay = 0.2f;     // ������ ���� �� ���� �ð�

    private Coroutine damageCoroutine;  // �ڷ�ƾ �ڵ�

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Init()
    {
        base.Init();
    }

    public override void Action()
    {
        base.Action();

        FireHitScan();
        Particle();
        //CartrigeDrop();
    }

    private void FireHitScan()
    {
        // ��Ʈ��ĵ ����ĳ��Ʈ ���� �� ���� �ִٸ� �ڷ�ƾ���� ������ ó��
        RaycastHit hit;
        Vector3 origin = bulletTransform.position;
        Vector3 direction = bulletTransform.forward;

        if (Physics.Raycast(origin, direction, out hit, hitScanRange, hitLayerMask))
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

            if (Physics.Raycast(origin, direction, out hitCheck, hitScanRange, hitLayerMask))
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

    public override void Reload()
    {
        base.Reload();

        if (ammo < megazine)
        {
            StartCoroutine(ReloadCoroutine());
        }
    }

    public override void CheckAmmo()
    {
        base.CheckAmmo();

        if (ammo > 0)
        {
            Action();
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
        IsReload = true;
        yield return new WaitForSeconds(reloadTime);
        ammo = megazine;
        IsReload = false;
    }

    public override void Equip()
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
            cartrigePoint = weaponTransform.FindChildByName(cartrigeTransformName);
            if (cartrigePoint == null)
            {
                Debug.LogError($"ź�� �߻� ��ġ�� ã�� �� �����ϴ�: {cartrigeTransformName}");
                return;
            }
        }

        base.Equip();
        transform.SetParent(weaponTransform, false);
        //transform.localPosition = Vector3.zero; // ���� ��ġ �ʱ�ȭ
        //transform.localRotation = Quaternion.identity; // ���� ȸ�� �ʱ�ȭ
        gameObject.SetActive(true);
    }

    public override void UnEquip()
    {
        base.UnEquip();
        transform.SetParent(null);
        //transform.localPosition = Vector3.zero; // �ʿ信 ���� �ʱ�ȭ
        //transform.localRotation = Quaternion.identity; // �ʿ信 ���� �ʱ�ȭ
        //gameObject.SetActive(false);
    }
}
