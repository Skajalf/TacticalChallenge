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

public abstract class WeaponBase : Entity
{
    [Header("Weapon Data Setting")]
    [SerializeField] private float power;         // 무기 데미지
    [SerializeField] private int maxAmmo;         // 탄창 크기
    [SerializeField] private float armorPiercing; // 방어력 관통
    [SerializeField] private float reloadTime;    // 재장전 시간
    [SerializeField] private ATKType attackType;  // 공격타입
    [SerializeField] private float roundPerMinute = 1f;// 탄 발사주기

    [SerializeField] public int RandomReload = 1; // 애니메이션 타입 (장전 모션이 복수 있는 캐릭터의 경우 1 이외의 숫자)

    [Header("Weapon Visuals")]
    [SerializeField] private GameObject projectilePrefab;   // 탄환 프리팹
    [SerializeField] private GameObject cartridgeParticle;  // 탄피 프리팹
    [SerializeField] private string weaponHolsterName = "WeaponPivot"; // 총의 위치 이름
    [SerializeField] private string bulletTransformName = "fire_01";   // 총알이 소환되는 위치 이름
    [SerializeField] private string cartridgeTransformName = "fire_02"; // 탄피가 소환되는 위치 이름
    [SerializeField] private GameObject flameParticle;       // 총구 화염 이펙트

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
            Debug.Assert(bulletTransform != null, $"{GetInstanceID()}의 BulletTransform - fire02가 null입니다.");
        }
    }

    public virtual void InitializeAmmo() // 게임이 시작하면서 총이 초기화 될 때 호출
    {
        Debug.Assert(maxAmmo > 0, $"{gameObject.GetInstanceID()}의 탄창이 {maxAmmo} 입니다.");
        currentAmmo = maxAmmo;
    }

    // 탄약 소비
    public virtual bool AmmoUse(int amount = 1)
    {
        if (amount <= 0) return false;
        if (IsEmpty) return false;
        CurrentAmmo = Mathf.Max(0, CurrentAmmo - amount);
        return true;
    }

    // 탄약 추가 (리턴: 실제 추가된 수)
    public int AddAmmo(int amount)
    {
        if (amount <= 0) return 0;
        int before = CurrentAmmo;
        CurrentAmmo = Mathf.Min(maxAmmo, CurrentAmmo + amount);
        return CurrentAmmo - before;
    }

    public void Attack()
    {
        // 발사 중 혹은 탄창이 비었다면 중복 발사 방지
        if (IsFiring || IsReloading || IsEmpty)
            return;

        StartCoroutine(FireCoroutine());  // 코루틴 호출
    }

    private IEnumerator FireCoroutine()
    {
        IsFiring = true;  // 발사 시작 및 Firing Flag True 

        Fire(1, 0, 11, this);         // 발사 실행

        // RPM 시간만큼 대기
        yield return new WaitForSeconds(roundPerMinute);

        // 발사가 완료되었으므로 발사 중 상태 초기화
        IsFiring = false;
    }

    public void Fire(float range, float damageDelay, LayerMask hitLayerMask, MonoBehaviour caller)
    {
        RaycastHit hit;
        Vector3 fireDirection = weaponTransform.forward; // 발사 방향
        Vector3 startPoint = bulletTransform.localToWorldMatrix.GetPosition();   // 총알 발사 위치

        if (AmmoUse(1) != true)
            return;

        // 레이캐스트로 타격 확인
        if (Physics.Raycast(startPoint, fireDirection, out hit, range, hitLayerMask))
        {
            // 타격된 객체의 이름 출력
            Debug.Log($"명중한 객체 이름: {hit.collider.name}");

            // MonoBehaviour를 가진 caller가 코루틴 실행
            caller.StartCoroutine(ApplyDamageWithDelay(hit, damageDelay, power));
        }
        else
        {
            Debug.Log("목표에 명중하지 않았습니다.");
        }

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
        var projectileInstance = ObjectPoolingManager.Instance.GetFromPool(projectilePrefab, bulletTransform.localToWorldMatrix.GetPosition(), bulletTransform.rotation, weaponTransform);
        Debug.Log($"{bulletTransform.localToWorldMatrix.GetPosition()} : 총알 위치 / {bulletTransform.rotation} : 총구방향"); 

        if (projectileInstance != null)
        {
            // 무기 정보 전달
            projectileInstance.GetComponent<Projectile>().Shoot(bulletTransform.localToWorldMatrix.GetPosition(), bulletTransform.rotation.eulerAngles, 75f, 10f);
        }
    }

    // 데미지 콜 하는 부분. 나중엔 서버측의 메서드를 가져다 이용해야할 것.
    private IEnumerator ApplyDamageWithDelay(RaycastHit hit, float delay, float power)
    {
        yield return new WaitForSeconds(delay);

        var target = hit.collider.GetComponent<Character>();
        if (target != null)
        {
            if (target.GetDamage(power))
                Debug.Log($"데미지 {power} 적용 완료.");
            else
                Debug.Log($"데미지 미적용");
        }
        else 
        {
            Debug.LogWarning("데미지를 적용할 수 없는 대상입니다.");
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

        // 재장전 사운드나 파티클 이펙트 호출 (추가 효과)
        Sound();
        Particle();

        // 재장전 시간 대기
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo; // 탄약을 가득 채움

        // 재장전 완료 후 사운드/효과 처리 (선택 사항)
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
                Debug.LogError($"탄피 발사 위치를 찾을 수 없습니다: {cartridgeTransformName}");
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

    // 애니메이션 이벤트에서 WeaponComponent가 호출 -> 자식에서 구현
}
