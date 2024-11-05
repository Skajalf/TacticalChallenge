using Cinemachine;
using UnityEngine;

public abstract class Test_WeaponBase : MonoBehaviour
{
    [Header("Weapon Data Setting")]
    [SerializeField] protected float power;                // 무기 데미지
    [SerializeField] protected float range;                  // 사거리
    [SerializeField] public float magazine;                // 탄창 크기
    [SerializeField] public float ammo;                    // 현재 잔탄 수
    [SerializeField] protected float reloadTime;           // 재장전 시간
    [SerializeField] protected LayerMask hitLayerMask;         // 타격 대상 레이어 설정
    [SerializeField] protected float damageDelay;       // 데미지 적용 전 지연 시간

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

    protected virtual void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        rootObject = transform.root.gameObject;
        Debug.Assert(rootObject != null);
    }

    // 장착 메서드
    public virtual void Test_Equip()
    {
        Debug.Log($"{this.name} 장착 완료.");
    }

    // 장착 해제 메서드
    public virtual void Test_UnEquip()
    {
        Debug.Log($"{this.name} 장착 해제.");
    }

    // 공격 메서드
    public virtual void Test_Attack()
    {
        Debug.Log($"{this.name} 공격 실행.");
    }

    // 재장전 메서드
    public virtual void Test_Reload()
    {
        Debug.Log($"{this.name} 재장전 시작.");
    }

    public virtual void Test_Impulse()
    {

    }

    protected virtual void Test_Sound()
    {

    }

    protected virtual void Test_Particle()
    {

    }
}
