using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapon System/Weapon Data")]
public class WeaponStatData : ScriptableObject
{
    [Header("Weapon Stats")]
    [SerializeField] private string weaponName;    // 무기 이름
    [SerializeField] private float power;          // 무기 데미지
    [SerializeField] private float armorPiercing; // 방어 관통
    [SerializeField] private float specialPiercing; // 특수 관통
    [SerializeField] private float critRate;       // 치명타 확률
    [SerializeField] private float critScale;      // 치명타 배율
    [SerializeField] private float lifeSteal;      // 생명력 흡수
    [SerializeField] private float speed;          // 추가 이동속도
    [SerializeField] private float megazine;       // 탄창 크기
    [SerializeField] private float reloadTime;     // 재장전 시간
    [SerializeField] private float damageDelay;    // 탄착 시간
    [SerializeField] private float range;          // 사거리
    [SerializeField] private LayerMask hitLayerMask; // 피격 가능 대상 (폭발 같은 경우라던지)

    [Header("Animation Settings")]
    [SerializeField] private int randomReload = 1; // 재장전 애니메이션 개수

    public int RandomReload => randomReload;

    public string WeaponName => weaponName;
    public float Power => power;
    public float ArmorPiercing => armorPiercing;
    public float SpecialPiercing => specialPiercing;
    public float CritRate => critRate;
    public float CritScale => critScale;
    public float LifeSteal => lifeSteal;
    public float Speed => speed;
    public float Megazine => megazine;
    public float ReloadTime => reloadTime;
    public float DamageDelay => damageDelay;
    public float Range => range;
    public LayerMask HitLayerMask => hitLayerMask;

    [Header("Visual Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject cartridgeParticle;
    [SerializeField] private string weaponHolsterName = "WeaponPivot";
    [SerializeField] private string bulletTransformName = "fire_01";
    [SerializeField] private string cartridgeTransformName = "fire_02";
    [SerializeField] private GameObject flameParticle;

    public GameObject ProjectilePrefab => projectilePrefab;
    public GameObject CartridgeParticle => cartridgeParticle;
    public string WeaponHolsterName => weaponHolsterName;
    public string BulletTransformName => bulletTransformName;
    public string CartridgeTransformName => cartridgeTransformName;
    public GameObject FlameParticle => flameParticle;
}
