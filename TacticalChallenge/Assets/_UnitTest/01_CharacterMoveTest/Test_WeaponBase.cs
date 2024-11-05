using Cinemachine;
using UnityEngine;

public abstract class Test_WeaponBase : MonoBehaviour
{
    [Header("Weapon Data Setting")]
    [SerializeField] protected float power;                // ���� ������
    [SerializeField] protected float range;                  // ��Ÿ�
    [SerializeField] public float magazine;                // źâ ũ��
    [SerializeField] public float ammo;                    // ���� ��ź ��
    [SerializeField] protected float reloadTime;           // ������ �ð�
    [SerializeField] protected LayerMask hitLayerMask;         // Ÿ�� ��� ���̾� ����
    [SerializeField] protected float damageDelay;       // ������ ���� �� ���� �ð�

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

    protected virtual void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        rootObject = transform.root.gameObject;
        Debug.Assert(rootObject != null);
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

    // ���� �޼���
    public virtual void Test_Attack()
    {
        Debug.Log($"{this.name} ���� ����.");
    }

    // ������ �޼���
    public virtual void Test_Reload()
    {
        Debug.Log($"{this.name} ������ ����.");
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
