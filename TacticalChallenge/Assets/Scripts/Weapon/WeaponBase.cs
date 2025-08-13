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
    [SerializeField] public Stat magazine;       // źâ ũ��
    [SerializeField] public Stat ammo;           // ���� ź��
    [SerializeField] protected Stat reloadTime;     // ������ �ð�
    [SerializeField] protected Stat damageDelay;    // ź�� �ð�
    [SerializeField] protected Stat range;          // ��Ÿ�
    [SerializeField] protected LayerMask hitLayerMask; // �ǰ� ���� ��� (���� ���� �������)

    [SerializeField] public int RandomReload = 1;

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
            Debug.LogError($"[{name}] magazine Stat�� �Ҵ���� �ʾҽ��ϴ�! Inspector���� magazine�� �����ϼ���.");
            currentAmmo = 0;
            return;
        }

        int magSize = MagazineSize;
        if (magSize <= 0)
        {
            Debug.LogError($"[{name}] MagazineSize�� 0�Դϴ�. Stat�� DefaultValue �Ǵ� weaponStats ���� Ȯ���ϼ���.");
            currentAmmo = 0;
            return;
        }

        if (ammo != null)
            currentAmmo = Mathf.Clamp(Mathf.RoundToInt(ammo.Value), 0, magSize);
        else
            currentAmmo = magSize;

        Debug.Log($"[{name}] InitializeAmmo -> {currentAmmo}/{magSize}");
    }

    // ź�� �Һ�
    public virtual bool AmmoUse(int amount = 1)
    {
        if (amount <= 0) return false;
        if (CurrentAmmo <= 0) return false;
        CurrentAmmo = Mathf.Max(0, CurrentAmmo - amount);
        return true;
    }

    // ź�� �߰� (����: ���� �߰��� ��)
    public virtual int AddAmmo(int amount)
    {
        if (amount <= 0) return 0;
        int before = CurrentAmmo;
        CurrentAmmo = Mathf.Min(MagazineSize, CurrentAmmo + amount);
        return CurrentAmmo - before;
    }

    protected virtual void Attack()
    {
        Debug.Log($"{this.name} Attack() �⺻ ���� (�ڽĿ��� �������̵��ϼ���).");
    }

    public virtual void Reload()
    {
        Debug.Log($"{this.name} Reload() �⺻ ���� (�ڽĿ��� �������̵��ϼ���).");
    }

    public virtual void Equip()
    {
        // �Ϲ������� WeaponComponent�� �θ�/transform ���� �� ȣ��
        Debug.Log($"{this.name} Equip() �⺻ ����.");
    }

    public virtual void UnEquip()
    {
        Debug.Log($"{this.name} UnEquip() �⺻ ����.");
    }

    protected virtual void Impulse() { }
    protected virtual void Sound() { }
    protected virtual void Particle() { }

    // �ִϸ��̼� �̺�Ʈ���� WeaponComponent�� ȣ�� -> �ڽĿ��� ����
    public virtual void AmmoLeft()
    {
    }
}
