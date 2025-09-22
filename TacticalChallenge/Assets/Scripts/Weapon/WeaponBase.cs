using Cinemachine;
using System;
using System.Collections;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

enum ATKType
{
    Explosive,
    Piercing,
    Mystic
}

public class WeaponBase : Entity
{
    [Header("Weapon Data Setting")]
    [SerializeField] private float power;         // ���� ������
    [SerializeField] private int maxAmmo;         // źâ ũ��
    [SerializeField] private float armorPiercing; // ���� ����
    [SerializeField] private float reloadTime;    // ������ �ð�
    [SerializeField] private ATKType attackType;  // ����Ÿ��
    [SerializeField] private float roundPerMinute = 1f; // ź �߻��ֱ�

    [SerializeField] public int RandomReload = 1; // �ִϸ��̼� Ÿ�� (���� ����� ���� �ִ� ĳ������ ��� 1 �̿��� ����)

    [Header("Weapon Visuals")]
    [SerializeField] private GameObject projectilePrefab;   // źȯ ������
    [SerializeField] private GameObject cartridgeParticle;  // ź�� ������
    [SerializeField] private string weaponHolsterName = "WeaponPivot"; // ���� ��ġ �̸�
    [SerializeField] private string bulletTransformName = "fire_01";   // �Ѿ��� ��ȯ�Ǵ� ��ġ �̸�
    [SerializeField] private string cartridgeTransformName = "fire_02"; // ź�ǰ� ��ȯ�Ǵ� ��ġ �̸�
    [SerializeField] private GameObject flameParticle;       // �ѱ� ȭ�� ����Ʈ

    [Header("Impulse Setting")]
    [SerializeField] private Vector3 impulseDirection;
    [SerializeField] private Cinemachine.NoiseSettings impulseSettings;
    private CinemachineImpulseSource impulse;

    [Header("Impact Setting")]
    [SerializeField] private int hitImpactIndex;
    [SerializeField] private GameObject hitParticle;
    [SerializeField] private GameObject damageParticle;
    [SerializeField] private Vector3 hitParticlePositionOffset;
    [SerializeField] private Vector3 hitParticleScaleOffset = Vector3.one;

    [Header("Weapon Offset Setting")]
    [SerializeField] private Transform centerSphere;
    private Transform weaponTransform;
    private Transform bulletTransform;
    private Transform cartridgePoint;
    private Vector3 weaponPoseOffset;
    private Vector3 weaponAimingOffset;

    protected bool IsReloading { get; set; }
    protected bool IsFiring { get; set; }
    
    protected int currentAmmo = 0;

    public int CurrentAmmo
    {
        get => currentAmmo;
        set => currentAmmo = Mathf.Clamp(value, 0, maxAmmo);
    }

    public bool IsEmpty => CurrentAmmo <= 0;
    public bool IsFull => CurrentAmmo >= maxAmmo;

    protected void Awake()
    {
        Init();
    }

    protected void Init()
    {
        IsFiring = false;
        if(bulletTransform == null)
        {
            bulletTransform = transform.FindChildByName(bulletTransformName);
            Debug.Assert(bulletTransform != null, $"{GetInstanceID()}�� BulletTransform - fire02�� null�Դϴ�.");
        }
    }

    public virtual void InitializeAmmo() // ������ �����ϸ鼭 ���� �ʱ�ȭ �� �� ȣ��
    {
        Debug.Assert(maxAmmo > 0, $"{gameObject.GetInstanceID()}�� źâ�� {maxAmmo} �Դϴ�.");
        currentAmmo = maxAmmo;
    }

    // ź�� �Һ�
    public virtual bool AmmoUse(int amount = 1)
    {
        if (amount <= 0) return false;
        if (IsEmpty) return false;
        CurrentAmmo = Mathf.Max(0, CurrentAmmo - amount);
        return true;
    }

    // ź�� �߰� (����: ���� �߰��� ��)
    public int AddAmmo(int amount)
    {
        if (amount <= 0) return 0;
        int before = CurrentAmmo;
        CurrentAmmo = Mathf.Min(maxAmmo, CurrentAmmo + amount);
        return CurrentAmmo - before;
    }

    public void Attack()
    {
        // �߻� �� Ȥ�� źâ�� ����ٸ� �ߺ� �߻� ����
        if (IsFiring || IsReloading || IsEmpty)
            return;

        StartCoroutine(FireCoroutine());  // �ڷ�ƾ ȣ��
    }

    private IEnumerator FireCoroutine()
    {
        IsFiring = true;  // �߻� ���� �� Firing Flag True 

        Fire(1, 0, 11, this);         // �߻� ����

        // RPM �ð���ŭ ���
        yield return new WaitForSeconds(roundPerMinute);

        // �߻簡 �Ϸ�Ǿ����Ƿ� �߻� �� ���� �ʱ�ȭ
        IsFiring = false;
    }

    public void Fire(float range, float damageDelay, LayerMask hitLayerMask, MonoBehaviour caller)
    {
        RaycastHit hit;
        Vector3 fireDirection = weaponTransform.forward; // �߻� ����
        Vector3 startPoint = bulletTransform.localToWorldMatrix.GetPosition();   // �Ѿ� �߻� ��ġ

        if (AmmoUse(1) != true)
            return;

        // ����ĳ��Ʈ�� Ÿ�� Ȯ��
        if (Physics.Raycast(startPoint, fireDirection, out hit, range, hitLayerMask))
        {
            // Ÿ�ݵ� ��ü�� �̸� ���
            Debug.Log($"������ ��ü �̸�: {hit.collider.name}");

            // MonoBehaviour�� ���� caller�� �ڷ�ƾ ����
            caller.StartCoroutine(ApplyDamageWithDelay(hit, damageDelay, power));
        }
        else
        {
            Debug.Log("��ǥ�� �������� �ʾҽ��ϴ�.");
        }

        FireProjectile();
    }
    private void FireProjectile()
    {
        if (projectilePrefab == null || bulletTransform == null)
        {
            Debug.LogWarning("����ü ������ �Ǵ� �߻� ��ġ�� �������� �ʾҽ��ϴ�.");
            return;
        }

        RaycastHit hit;
        Vector3 fireDirection = Camera.main.transform.forward;
        Vector3 targetPoint;

        if (Physics.Raycast(bulletTransform.position, fireDirection, out hit, 1000f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = bulletTransform.position + fireDirection * 1000f;
        }

        Vector3 projectileDirection = (targetPoint - bulletTransform.position).normalized;

        var projectileInstance = ObjectPoolingManager.Instance.GetFromPool(projectilePrefab, bulletTransform.position, Quaternion.LookRotation(projectileDirection));

        if (projectileInstance != null)
        {
            var projectile = projectileInstance.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Shoot(bulletTransform.position, projectileDirection, 75f, 10f);
            }
        }
    }

    private IEnumerator ApplyDamageWithDelay(RaycastHit hit, float delay, float power)
    {
        yield return new WaitForSeconds(delay);

        var target = hit.collider.GetComponent<Character>();
        if (target != null)
        {
            if (target.GetDamage(power))
                Debug.Log($"������ {power} ���� �Ϸ�.");
            else
                Debug.Log($"������ ������");
        }
        else 
        {
            Debug.LogWarning("�������� ������ �� ���� ����Դϴ�.");
        }
    }


    public virtual bool Reload()
    {
        if (!IsReloading && currentAmmo < maxAmmo)
        {
            StartCoroutine(ReloadCoroutine());
        }
        else
        {
            Debug.Log("�������� �ʿ����� �ʽ��ϴ�.");
            return false;
        }
        return true;
    }

    private IEnumerator ReloadCoroutine()
    {
        IsReloading = true;

        // ������ ���峪 ��ƼŬ ����Ʈ ȣ�� (�߰� ȿ��)
        Sound();
        Particle();

        // ������ �ð� ���
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo; // ź���� ���� ä��

        // ������ �Ϸ� �� ����/ȿ�� ó�� (���� ����)
        Debug.Log("������ �Ϸ�!");

        IsReloading = false;
    }

    public virtual void Equip()
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

        if (cartridgePoint == null)
        {
            cartridgePoint = weaponTransform.FindChildByName(cartridgeTransformName);
            if (cartridgePoint == null)
            {
                Debug.LogError($"ź�� �߻� ��ġ�� ã�� �� �����ϴ�: {cartridgeTransformName}");
                return;
            }
        }

        transform.SetParent(weaponTransform, false);
        gameObject.SetActive(true);
    }

    public virtual void UnEquip()
    {
        
    }

    protected virtual void Impulse() { }
    protected virtual void Sound() { }
    protected virtual void Particle() { }

    // �ִϸ��̼� �̺�Ʈ���� WeaponComponent�� ȣ�� -> �ڽĿ��� ����
}
