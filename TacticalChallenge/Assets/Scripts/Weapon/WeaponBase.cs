using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header(" Weapon Data Setting")]
    [SerializeField] protected float power; //���� ������
    [SerializeField] protected float megazine; //źâ (��������� ��� X)
    [SerializeField] protected float ammo; // ���� ��ź�� (��������� ��� X)
    [SerializeField] protected float reloadTime; // ������ �ð� (��������� ��� X)
    [SerializeField] protected float distance; // ��Ÿ�

    [Header(" Weapon Visuals")]
    [SerializeField] protected GameObject projectilePrefab; // źȯ ������
    [SerializeField] protected GameObject cartrigeParticle; //ź�� ������
    [SerializeField] protected string weaponHolsterName = "Bip001_Weapon"; // ���� ��ġ �̸�
    [SerializeField] protected string bulletTransformName = "fire_01"; // �Ѿ��� ��ȯ�Ǵ� ��ġ �̸�
    [SerializeField] protected string cartrigeTransformName = "fire_02"; // ź�ǰ� ��ȯ�Ǵ� ��ġ �̸�
    [SerializeField] protected GameObject flameParticle; // �ѱ� ȭ�� ����Ʈ
    protected GameObject rootObject;
    protected Transform weaponTransform; // ���� ��ġ
    protected Transform bulletTransform; // źȯ�� �߻� ��ġ
    protected Transform cartrigePoint; // ź���� �߻� ��ġ

    [Header(" Impulse Setting")] // ī�޶� ����ũ ȿ�� ����
    [SerializeField] protected Vector3 impulseDirection;
    [SerializeField] protected Cinemachine.NoiseSettings impulseSettings;

    [Header(" Impact Setting")] // �ǰ� ��� ����Ʈ ����
    [SerializeField] protected int hitImpactIndex; // �ǰ�Ÿ�� - ��������, ��������
    [SerializeField] protected GameObject hitParticle; // �ǰ� ǥ�� �ߴ°�
    [SerializeField] protected GameObject damageParticle; //�ǰ� ��ƼŬ ������ 67, 105 �ߴ� �װ�
    [SerializeField] protected Vector3 hitParticlePositionOffset; // �ǰ� ��ƼŬ�� ��ġ
    [SerializeField] protected Vector3 hitParticleScaleOffset = Vector3.one; // �ǰ� ��ƼŬ�� ������

    protected virtual void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        rootObject = transform.root.gameObject;
        Debug.Assert(rootObject != null);
    }

    protected virtual void Attack()
    {

    }

    protected virtual void Action()
    {

    }

    public virtual void Equip()
    {

    }

    public virtual void UnEquip()
    {

    }

    protected virtual void Sound()
    {

    }

    protected virtual void Particle()
    {

    }
}
