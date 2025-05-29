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
    [SerializeField] protected LayerMask hitLayerMask; // �ǰ� ���� ��� (���� ���� �������)

    [Space]
    [Header("Weapon Stats")]
    [SerializeField] protected StatOverride[] statOverrides;
    protected Stat[] stats;

    [Space]
    [Header("Weapon Visuals")]
    [SerializeField] protected GameObject projectilePrefab;   // źȯ ������
    [SerializeField] protected GameObject cartridgeParticle;  // ź�� ������
    [SerializeField] protected GameObject flameParticle;       // �ѱ� ȭ�� ����Ʈ
    protected string weaponHolsterName = "WeaponPivot"; // ���� ��ġ �̸�
    protected string bulletTransformName = "fire_01";   // �Ѿ��� ��ȯ�Ǵ� ��ġ �̸�
    protected string cartridgeTransformName = "fire_02"; // ź�ǰ� ��ȯ�Ǵ� ��ġ �̸�


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
    protected Transform weaponTransform; // ���� ��ġ
    protected Transform bulletTransform; // źȯ�� �߻� ��ġ
    protected Transform cartrigePoint; // ź���� �߻� ��ġ
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
        Debug.Assert(rootObject != null, "�ù߷þ�");
    }

    protected abstract void Attack(); // ���� �޼���
    public abstract void Reload(); // ������ �޼���
    public abstract void Equip(); // ���� �޼���
    public abstract void UnEquip(); // ���� ���� �޼���
    protected abstract void Impulse(); // �θ��� �޼���
    protected abstract void Sound(); // �Ҹ� �޼���
    protected abstract void Particle(); // ��ƼŬ �޼���
    #endregion
}
