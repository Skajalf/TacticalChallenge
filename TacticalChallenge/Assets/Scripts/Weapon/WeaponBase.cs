using Cinemachine;
using System;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Data Setting")]
    [SerializeField] protected string weaponName;    // 무기 이름
    [SerializeField] protected Stat power;          // 무기 데미지
    [SerializeField] protected Stat armorPiercing; // 방어 관통
    [SerializeField] protected Stat specialPiercing; // 특수 관통
    [SerializeField] protected Stat critRate;       // 치명타 확률
    [SerializeField] protected Stat critScale;      // 치명타 배율
    [SerializeField] protected Stat lifeSteal;      // 생명력 흡수
    [SerializeField] protected Stat speed;          // 추가 이동속도
    [SerializeField] public Stat magazine;       // 탄창 크기
    [SerializeField] public Stat ammo;           // 현재 탄수
    [SerializeField] protected Stat reloadTime;     // 재장전 시간
    [SerializeField] protected Stat damageDelay;    // 탄착 시간
    [SerializeField] protected Stat range;          // 사거리
    [SerializeField] protected LayerMask hitLayerMask; // 피격 가능 대상 (폭발 같은 경우라던지)

    [SerializeField] public int RandomReload = 1;

    [SerializeField] protected WeaponStatData weaponStats;

    [Header("Weapon Visuals")]
    [SerializeField] protected GameObject projectilePrefab;   // 탄환 프리팹
    [SerializeField] protected GameObject cartridgeParticle;  // 탄피 프리팹
    [SerializeField] protected string weaponHolsterName = "WeaponPivot"; // 총의 위치 이름
    [SerializeField] protected string bulletTransformName = "fire_01";   // 총알이 소환되는 위치 이름
    [SerializeField] protected string cartridgeTransformName = "fire_02"; // 탄피가 소환되는 위치 이름
    [SerializeField] protected GameObject flameParticle;       // 총구 화염 이펙트

    [Header("Impulse Setting")]
    [SerializeField] protected Vector3 impulseDirection;
    [SerializeField] protected Cinemachine.NoiseSettings impulseSettings;
    protected CinemachineImpulseSource impulse;

    [Header("Impact Setting")]
    [SerializeField] protected int hitImpactIndex;
    [SerializeField] protected GameObject hitParticle;
    [SerializeField] protected GameObject damageParticle;
    [SerializeField] protected Vector3 hitParticlePositionOffset;
    [SerializeField] protected Vector3 hitParticleScaleOffset = Vector3.one;

    [Header("Weapon Offset Setting")]
    protected GameObject rootObject;
    protected Transform weaponTransform;
    protected Transform bulletTransform;
    protected Transform cartridgePoint;
    public Vector3 weaponPoseOffset;
    public Vector3 weaponAimingOffset;

    //private Stat[] stats;


    protected bool IsReload { get; set; }
    protected bool IsFiring { get; set; }
    [SerializeField] protected float animationWaitTime;

    public Animator animator;

    protected int currentAmmo = 0;
    public int CurrentAmmo
    {
        get => currentAmmo;
        set => currentAmmo = Mathf.Clamp(value, 0, MagazineSize);
    }

    public int MagazineSize => magazine != null ? Mathf.RoundToInt(magazine.Value) : 0;
    public bool IsEmpty => CurrentAmmo <= 0;
    public bool IsFull => CurrentAmmo >= MagazineSize;

    protected virtual void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        rootObject = transform.root != null ? transform.root.gameObject : gameObject;

        if (weaponStats != null)
        {
            weaponName = weaponStats.WeaponName;
            if (power != null) power.DefaultValue = weaponStats.Power;
            if (armorPiercing != null) armorPiercing.DefaultValue = weaponStats.ArmorPiercing;
            if (specialPiercing != null) specialPiercing.DefaultValue = weaponStats.SpecialPiercing;
            if (critRate != null) critRate.DefaultValue = weaponStats.CritRate;
            if (critScale != null) critScale.DefaultValue = weaponStats.CritScale;
            if (lifeSteal != null) lifeSteal.DefaultValue = weaponStats.LifeSteal;
            if (speed != null) speed.DefaultValue = weaponStats.Speed;
            if (magazine != null) magazine.DefaultValue = weaponStats.Megazine;
            if (reloadTime != null) reloadTime.DefaultValue = weaponStats.ReloadTime;
            if (damageDelay != null) damageDelay.DefaultValue = weaponStats.DamageDelay;
            if (range != null) range.DefaultValue = weaponStats.Range;
            hitLayerMask = weaponStats.HitLayerMask;
        }
    }

    public virtual void InitializeAmmo()
    {
        Debug.Log($"[{name}] InitializeAmmo called. magazine assigned? {(magazine != null)} ammo assigned? {(ammo != null)} weaponStats assigned? {(weaponStats != null)}");

        if (magazine == null)
        {
            Debug.LogError($"[{name}] magazine Stat이 할당되지 않았습니다! Inspector에서 magazine을 설정하세요.");
            currentAmmo = 0;
            return;
        }

        int magSize = MagazineSize;
        if (magSize <= 0)
        {
            Debug.LogError($"[{name}] MagazineSize가 0입니다. Stat의 DefaultValue 또는 weaponStats 값을 확인하세요.");
            currentAmmo = 0;
            return;
        }

        if (ammo != null)
            currentAmmo = Mathf.Clamp(Mathf.RoundToInt(ammo.Value), 0, magSize);
        else
            currentAmmo = magSize;

        Debug.Log($"[{name}] InitializeAmmo -> {currentAmmo}/{magSize}");
    }

    // 탄약 소비
    public virtual bool AmmoUse(int amount = 1)
    {
        if (amount <= 0) return false;
        if (CurrentAmmo <= 0) return false;
        CurrentAmmo = Mathf.Max(0, CurrentAmmo - amount);
        return true;
    }

    // 탄약 추가 (리턴: 실제 추가된 수)
    public virtual int AddAmmo(int amount)
    {
        if (amount <= 0) return 0;
        int before = CurrentAmmo;
        CurrentAmmo = Mathf.Min(MagazineSize, CurrentAmmo + amount);
        return CurrentAmmo - before;
    }

    protected virtual void Attack()
    {
        Debug.Log($"{this.name} Attack() 기본 실행 (자식에서 오버라이드하세요).");
    }

    public virtual void Reload()
    {
        Debug.Log($"{this.name} Reload() 기본 실행 (자식에서 오버라이드하세요).");
    }

    public virtual void Equip()
    {
        // 일반적으로 WeaponComponent가 부모/transform 연결 후 호출
        Debug.Log($"{this.name} Equip() 기본 실행.");
    }

    public virtual void UnEquip()
    {
        Debug.Log($"{this.name} UnEquip() 기본 실행.");
    }

    protected virtual void Impulse() { }
    protected virtual void Sound() { }
    protected virtual void Particle() { }

    // 애니메이션 이벤트에서 WeaponComponent가 호출 -> 자식에서 구현
    public virtual void AmmoLeft()
    {
    }
}
