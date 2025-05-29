using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    #region Fields
    [Header("Weapon Datas")]
    [SerializeField] protected string codeName="CH_";
    [SerializeField] protected string displayName="";
    [SerializeField] protected LayerMask hitLayerMask; // 피격 가능 대상 (폭발 같은 경우라던지)

    [Space]
    [Header("Weapon Stats")]
    [SerializeField] protected StatOverride[] statOverrides;
    protected Stat[] stats;

    [Space]
    [Header("Weapon Visuals")]
    [SerializeField] protected GameObject projectilePrefab;   // 탄환 프리팹
    [SerializeField] protected GameObject cartridgeParticle;  // 탄피 프리팹
    [SerializeField] protected GameObject flameParticle;       // 총구 화염 이펙트
    protected string weaponHolsterName = "WeaponPivot"; // 총의 위치 이름
    protected string bulletTransformName = "fire_01";   // 총알이 소환되는 위치 이름
    protected string cartridgeTransformName = "fire_02"; // 탄피가 소환되는 위치 이름


    [Space]
    [Header("Impulse Setting")]
    [SerializeField] protected Vector3 impulseDirection;
    [SerializeField] protected Cinemachine.NoiseSettings impulseSettings;
    protected CinemachineImpulseSource impulse;

    [Space]
    [Header("Impact Setting")]
    [SerializeField] protected int hitImpactIndex;
    [SerializeField] protected GameObject hitParticle;
    [SerializeField] protected GameObject damageParticle;
    [Space]
    [SerializeField] protected Vector3 hitParticlePositionOffset;
    [SerializeField] protected Vector3 hitParticleScaleOffset = Vector3.one;

    [Space]
    [Header("Weapon Offset Setting")]
    protected GameObject rootObject;
    protected Transform weaponTransform; // 총의 위치
    protected Transform bulletTransform; // 탄환의 발사 위치
    protected Transform cartrigePoint; // 탄피의 발사 위치
    public Vector3 weaponPoseOffset;
    public Vector3 weaponAimingOffset;

    protected bool IsReload { get; set; }
    protected bool IsFiring { get; set; }
    protected Animator animator;
    protected Character character;

    protected Stat ammo;
    protected Stat magazine;
    #endregion

    #region Methods
    protected virtual void Awake() { init(); }
    protected virtual void init() 
    { 
        rootObject = transform.root.gameObject;
        Debug.Assert(rootObject != null, "시발련아");
    }

    protected abstract void Attack(); // 공격 메서드
    public abstract void Reload(); // 재장전 메서드
    public abstract void Equip(); // 장착 메서드
    public abstract void UnEquip(); // 장착 해제 메서드
    protected abstract void Impulse(); // 부르르 메서드
    protected abstract void Sound(); // 소리 메서드
    protected abstract void Particle(); // 파티클 메서드
    #endregion
}
