using Cinemachine;
using System;
using System.Collections;
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
    [SerializeField] private float power;
    [SerializeField] private int maxAmmo;
    [SerializeField] private float armorPiercing;
    [SerializeField] private float reloadTime;
    [SerializeField] private ATKType attackType;
    [SerializeField] private float roundPerMinute = 1f;

    [SerializeField] public int RandomReload = 1;

    [Header("Weapon Visuals")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject cartridgeParticle;
    [SerializeField] private string weaponHolsterName = "WeaponPivot";
    [SerializeField] private string bulletTransformName = "fire_01";
    [SerializeField] private string cartridgeTransformName = "fire_02";
    [SerializeField] private GameObject flameParticle;

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
        if (bulletTransform == null)
        {
            bulletTransform = transform.FindChildByName(bulletTransformName);
            Debug.Assert(bulletTransform != null, $"{GetInstanceID()}에 {bulletTransformName}이(가) 없습니다.");
        }
    }

    public virtual void InitializeAmmo()
    {
        Debug.Assert(maxAmmo > 0, $"{gameObject.GetInstanceID()}의 탄약 크기가 {maxAmmo} 입니다.");
        currentAmmo = maxAmmo;
    }

    public virtual bool AmmoUse(int amount = 1)
    {
        if (amount <= 0) return false;
        if (IsEmpty) return false;
        CurrentAmmo = Mathf.Max(0, CurrentAmmo - amount);
        return true;
    }

    public int AddAmmo(int amount)
    {
        if (amount <= 0) return 0;
        int before = CurrentAmmo;
        CurrentAmmo = Mathf.Min(maxAmmo, CurrentAmmo + amount);
        return CurrentAmmo - before;
    }

    public void Attack()
    {
        if (IsFiring || IsReloading || IsEmpty)
            return;

        StartCoroutine(FireCoroutine());
    }

    private IEnumerator FireCoroutine()
    {
        IsFiring = true;

        Fire(1000f, 0, 11, this);

        yield return new WaitForSeconds(roundPerMinute);

        IsFiring = false;
    }

    public void Fire(float range, float damageDelay, LayerMask hitLayerMask, MonoBehaviour caller)
    {
        if (AmmoUse(1) != true)
            return;

        RaycastHit hit;
        Vector3 fireDirection = Camera.main.transform.forward;
        Vector3 rayStartPoint = Camera.main.transform.position;

        Vector3 targetPoint = rayStartPoint + fireDirection * range;

        if (Physics.Raycast(rayStartPoint, fireDirection, out hit, range, hitLayerMask))
        {
            targetPoint = hit.point;

            Debug.Log($"데미지 판정: 타격된 객체: {hit.collider.name}");

            caller.StartCoroutine(ApplyDamageWithDelay(hit, damageDelay, power));
        }
        else
        {
            Debug.Log("데미지 판정: 타겟을 맞추지 못했습니다.");
        }

        FireProjectile(targetPoint);
    }

    private void FireProjectile(Vector3 targetPoint)
    {
        if (projectilePrefab == null || bulletTransform == null)
        {
            Debug.LogWarning("투사체 프리팹 또는 발사 위치가 설정되지 않았습니다.");
            return;
        }

        Vector3 startPosition = bulletTransform.position;

        // 총알의 이동 방향: 총구에서 카메라 Raycast 타겟을 향하도록 계산
        Vector3 projectileDirection = (targetPoint - startPosition).normalized;

        // ⭐ 총알이 뒤로 날아가는 문제를 해결하기 위해 회전 시 방향 벡터를 반전
        // 이는 프리팹 모델의 로컬 Z축이 앞을 향하지 않을 때 발생합니다.
        Vector3 rotationDirection = projectileDirection * -1f;

        // 투사체를 총구 위치에서 생성하고 반전된 방향으로 회전시킵니다.
        var projectileInstance = ObjectPoolingManager.Instance.GetFromPool(
            projectilePrefab,
            startPosition,
            Quaternion.LookRotation(rotationDirection) // 회전 시 반전된 방향 사용
        );

        if (projectileInstance != null)
        {
            var projectile = projectileInstance.GetComponent<Projectile>();
            if (projectile != null)
            {
                // 실제 이동 방향은 반전 없이 정방향을 사용합니다.
                projectile.Shoot(startPosition, projectileDirection, 75f, 10f);
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
                Debug.Log($"데미지 {power} 적용 완료.");
            else
                Debug.Log($"데미지 적용 실패");
        }
        else
        {
            Debug.LogWarning("데미지를 줄 수 있는 대상이 아닙니다.");
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
            Debug.Log("재장전이 필요하지 않습니다.");
            return false;
        }
        return true;
    }

    private IEnumerator ReloadCoroutine()
    {
        IsReloading = true;

        Sound();
        Particle();

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;

        Debug.Log("재장전 완료!");

        IsReloading = false;
    }

    public virtual void Equip()
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

        if (cartridgePoint == null)
        {
            cartridgePoint = weaponTransform.FindChildByName(cartridgeTransformName);
            if (cartridgePoint == null)
            {
                Debug.LogError($"탄피 배출 위치를 찾을 수 없습니다: {cartridgeTransformName}");
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
}