using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Cinemachine;

[Serializable]
public class WeaponData // ���� ������
{
    public bool bCanMove;

    [Header(" Data Setting")]
    public float Power; // ������
    public float Ammo; // �ִ� ź�෮
    public float currentAmmo; // ���� ��ź
    public float ReloadTime; // ������ �ð�
    public float Distance; // ��Ÿ�
    public int StopFrame; // ������ �Ѿ��� �¾����� ��Ʈ��ž

    [Header(" Visuals")]
    public GameObject projectilePrefab; // �߻�ü ������
    public Transform firePoint;         // �߻� ��ġ
    public GameObject Particle; // �ѱ� ȭ��
    public GameObject CartrigeCase; // ź��

    [Header(" Impulse Setting")] // ī�޶� ����ŷ(�ݵ�?)
    public Vector3 ImpulseDirection;
    public Cinemachine.NoiseSettings ImpulseSettings;

    [Header(" Impact Setting")] // �� ������ �¾����� ����Ʈ
    public int HitImpactIndex;
    public GameObject HitParticle;
    public Vector3 HitParticlePositionOffset;
    public Vector3 HitParticleScaleOffset = Vector3.one;
}

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] public WeaponData weapondata;

    public bool IsEquip { protected set; get; }
    public bool IsReload { protected set; get; }

    protected CinemachineImpulseSource impulse;
    protected CinemachineBrain brain;
    protected GameObject rootObject;
    protected StateComponent state;
    protected Animator animator;

    protected virtual void Reset()
    {

    }

    protected virtual void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        rootObject = transform.root.gameObject;
        Debug.Assert(rootObject != null);

        state = rootObject.GetComponent<StateComponent>();
        animator = rootObject.GetComponent<Animator>();
        impulse = GetComponent<CinemachineImpulseSource>();
        brain = Camera.main.GetComponent<CinemachineBrain>();

        // �߰��� �κ� fire_01�� ������.
        weapondata.firePoint = rootObject.transform.FindChildByName("fire_01").GetComponent<Transform>();
    }

    public virtual void Equip()
    {

    }

    public virtual void UnEquip()
    {

    }

    public virtual void DoAction()
    {

    }

    public virtual void EndDoAction()
    {

    }

    public virtual void Reload()
    {

    }

    public virtual void CheckAmmo()
    {

    }

    protected void Move()
    {
        MovingComponent moving = rootObject.GetComponent<MovingComponent>();

        if (moving != null)
            moving.Move();
    }

    protected void CheckStop(int index)
    {
        if (weapondata.bCanMove == false)
        {
            MovingComponent moving = rootObject.GetComponent<MovingComponent>();

            if (moving != null)
                moving.Stop();
        }
    }


}