using Cinemachine;
using System;
using UnityEngine;

public abstract class Test_WeaponBase : MonoBehaviour
{
    [Header("Weapon Data Setting")]
    [SerializeField] protected string weaponName;    // ���� �̸�
    [SerializeField] protected float power;          // ���� ������
    [SerializeField] protected float armorPiercing; // ��� ����
    [SerializeField] protected float specialPiercing; // Ư�� ����
    [SerializeField] protected float critRate;       // ġ��Ÿ Ȯ��
    [SerializeField] protected float critScale;      // ġ��Ÿ ����
    [SerializeField] protected float lifeSteal;      // ����� ���
    [SerializeField] protected float speed;          // �߰� �̵��ӵ�
    [SerializeField] public float megazine;       // źâ ũ��
    [SerializeField] public float ammo;           // ���� ź��
    [SerializeField] protected float reloadTime;     // ������ �ð�
    [SerializeField] protected float damageDelay;    // ź�� �ð�
    [SerializeField] protected float range;          // ��Ÿ�
    [SerializeField] protected LayerMask hitLayerMask; // �ǰ� ���� ��� (���� ���� �������)

    [SerializeField] protected WeaponStatData weaponStats;

    [Header("Weapon Visuals")]
    [SerializeField] protected GameObject projectilePrefab;   // źȯ ������
    [SerializeField] protected GameObject cartridgeParticle;  // ź�� ������
    [SerializeField] protected string weaponHolsterName = "WeaponPivot"; // ���� ��ġ �̸�
    [SerializeField] protected string bulletTransformName = "fire_01";   // �Ѿ��� ��ȯ�Ǵ� ��ġ �̸�
    [SerializeField] protected string cartridgeTransformName = "fire_02"; // ź�ǰ� ��ȯ�Ǵ� ��ġ �̸�
    [SerializeField] protected GameObject flameParticle;       // �ѱ� ȭ�� ����Ʈ

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
    protected Transform weaponTransform; // ���� ��ġ
    protected Transform bulletTransform; // źȯ�� �߻� ��ġ
    protected Transform cartrigePoint; // ź���� �߻� ��ġ
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
        power = weaponStats.Power;
        armorPiercing = weaponStats.ArmorPiercing;
        specialPiercing = weaponStats.SpecialPiercing;
        critRate = weaponStats.CritRate;
        critScale = weaponStats.CritScale;
        lifeSteal = weaponStats.LifeSteal;
        speed = weaponStats.Speed;
        megazine = weaponStats.Megazine;
        reloadTime = weaponStats.ReloadTime;
        damageDelay = weaponStats.DamageDelay;
        range = weaponStats.Range;
        hitLayerMask = weaponStats.HitLayerMask;
    }

    // ���� �޼���
    protected virtual void Test_Attack()
    {
        Debug.Log($"{this.name} ���� ����.");
    }

    // ������ �޼���
    public virtual void Test_Reload()
    {
        Debug.Log($"{this.name} ������ ����.");
    }

    // ���� �޼���
    public virtual void Test_Equip()
    {
        Debug.Log($"{this.name} ���� �Ϸ�.");
    }

    // ���� ���� �޼���
    public virtual void Test_UnEquip()
    {
        Debug.Log($"{this.name} ���� ����.");
    }

    protected virtual void Test_Impulse()
    {

    }

    protected virtual void Test_Sound()
    {

    }

    protected virtual void Test_Particle()
    {

    }

    public virtual void AmmoLeft()
    {
        
    }
}
