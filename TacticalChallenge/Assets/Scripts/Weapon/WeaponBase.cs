using Cinemachine;
using System;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Data Setting")]
    [SerializeField] protected string weaponName;    // ���� �̸�
    [SerializeField] protected Stat power;          // ���� ������
    [SerializeField] protected Stat armorPiercing; // ��� ����
    [SerializeField] protected Stat specialPiercing; // Ư�� ����
    [SerializeField] protected Stat critRate;       // ġ��Ÿ Ȯ��
    [SerializeField] protected Stat critScale;      // ġ��Ÿ ����
    [SerializeField] protected Stat lifeSteal;      // ����� ���
    [SerializeField] protected Stat speed;          // �߰� �̵��ӵ�
    [SerializeField] public Stat megazine;       // źâ ũ��
    [SerializeField] public Stat ammo;           // ���� ź��
    [SerializeField] protected Stat reloadTime;     // ������ �ð�
    [SerializeField] protected Stat damageDelay;    // ź�� �ð�
    [SerializeField] protected Stat range;          // ��Ÿ�
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

    // ���� �޼���
    protected virtual void Attack()
    {
        Debug.Log($"{this.name} ���� ����.");
    }

    // ������ �޼���
    public virtual void Reload()
    {
        Debug.Log($"{this.name} ������ ����.");
    }

    // ���� �޼���
    public virtual void Equip()
    {
        Debug.Log($"{this.name} ���� �Ϸ�.");
    }

    // ���� ���� �޼���
    public virtual void UnEquip()
    {
        Debug.Log($"{this.name} ���� ����.");
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
