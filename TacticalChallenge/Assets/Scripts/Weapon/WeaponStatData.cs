using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapon System/Weapon Data")]
public class WeaponStatData : ScriptableObject
{
    [Header("Weapon Stats")]
    [SerializeField] private string weaponName;    // ���� �̸�
    [SerializeField] private float power;          // ���� ������
    [SerializeField] private float armorPiercing; // ��� ����
    [SerializeField] private float specialPiercing; // Ư�� ����
    [SerializeField] private float critRate;       // ġ��Ÿ Ȯ��
    [SerializeField] private float critScale;      // ġ��Ÿ ����
    [SerializeField] private float lifeSteal;      // ����� ���
    [SerializeField] private float speed;          // �߰� �̵��ӵ�
    [SerializeField] private float megazine;       // źâ ũ��
    [SerializeField] private float reloadTime;     // ������ �ð�
    [SerializeField] private float damageDelay;    // ź�� �ð�
    [SerializeField] private float range;          // ��Ÿ�
    [SerializeField] private LayerMask hitLayerMask; // �ǰ� ���� ��� (���� ���� �������)

    [Header("Animation Settings")]
    [SerializeField] private int randomReload = 1; // ������ �ִϸ��̼� ����

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
