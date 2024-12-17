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
    [SerializeField] public Stat megazine;       // 탄창 크기
    [SerializeField] public Stat ammo;           // 현재 탄수
    [SerializeField] protected Stat reloadTime;     // 재장전 시간
    [SerializeField] protected Stat damageDelay;    // 탄착 시간
    [SerializeField] protected Stat range;          // 사거리
    [SerializeField] protected LayerMask hitLayerMask; // 피격 가능 대상 (폭발 같은 경우라던지)

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
    protected Transform weaponTransform; // 총의 위치
    protected Transform bulletTransform; // 탄환의 발사 위치
    protected Transform cartrigePoint; // 탄피의 발사 위치
    public Vector3 weaponPoseOffset;
    public Vector3 weaponAimingOffset;

    protected bool IsReload { get; set; }
    protected bool IsFiring { get; set; }
    [SerializeField] protected float animationWaitTime;
    protected Animator animator;

    protected virtual void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        rootObject = transform.root.gameObject;
        Debug.Assert(rootObject != null);

        weaponName = weaponStats.WeaponName;
        power.DefaultValue = weaponStats.Power;
        armorPiercing.DefaultValue = weaponStats.ArmorPiercing;
        specialPiercing.DefaultValue = weaponStats.SpecialPiercing;
        critRate.DefaultValue = weaponStats.CritRate;
        critScale.DefaultValue = weaponStats.CritScale;
        lifeSteal.DefaultValue = weaponStats.LifeSteal;
        speed.DefaultValue = weaponStats.Speed;
        megazine.DefaultValue = weaponStats.Megazine;
        reloadTime.DefaultValue = weaponStats.ReloadTime;
        damageDelay.DefaultValue = weaponStats.DamageDelay;
        range.DefaultValue = weaponStats.Range;
        hitLayerMask = weaponStats.HitLayerMask;
    }

    // 공격 메서드
    protected virtual void Attack()
    {
        Debug.Log($"{this.name} 공격 실행.");
    }

    // 재장전 메서드
    public virtual void Reload()
    {
        Debug.Log($"{this.name} 재장전 시작.");
    }

    // 장착 메서드
    public virtual void Equip()
    {
        Debug.Log($"{this.name} 장착 완료.");
    }

    // 장착 해제 메서드
    public virtual void UnEquip()
    {
        Debug.Log($"{this.name} 장착 해제.");
    }

    protected virtual void Impulse()
    {

    }

    protected virtual void Sound()
    {

    }

    protected virtual void Particle()
    {

    }

    public virtual void AmmoLeft()
    {
        
    }
}
