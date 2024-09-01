using System;
using UnityEngine;

[Serializable]
public class WeaponData
{
    public bool bCanMove;

    public float Power;
    public float Ammo;
    public float currentAmmo;
    public float ReloadTime;
    public float Distance;
    public int StopFrame;

    public GameObject Particle;

    [Header(" Impulse Setting")]
    public Vector3 ImpulseDirection;
    public Cinemachine.NoiseSettings ImpulseSettings;

    [Header(" Impact Setting")]
    public int HitImpactIndex;

    public GameObject HitParticle;
    public Vector3 HitParticlePositionOffset;
    public Vector3 HitParticleScaleOffset = Vector3.one;
}

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected WeaponData weapondata;
    [SerializeField] private int damage;

    public int Damage
    {
        get { return damage; }
    }

    public bool Equipping { get => bEquipping; }
    private bool bEquipping;
    protected bool bEquipped;

    protected GameObject rootObject;
    protected StateComponent state;
    protected Animator animator;

    protected virtual void Reset()
    {

    }

    protected virtual void Awake()
    {
        rootObject = transform.root.gameObject;
        Debug.Assert(rootObject != null);

        state = rootObject.GetComponent<StateComponent>();
        animator = rootObject.GetComponent<Animator>();
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {

    }

    public void Equip()
    {
        state.SetEquipMode();
    }

    public virtual void Begin_Equip()
    {
        bEquipping = true;
    }

    public virtual void End_Equip()
    {
        bEquipping = false;
        bEquipped = true;

        state.SetIdleMode();
    }

    public virtual void UnEquip()
    {
        bEquipped = false;
    }

    public virtual bool CanDoAction()
    {
        return true;
    }

    public virtual void DoAction()
    {
        state.SetActionMode();

        CheckStop(0);
    }

    public virtual void Begin_DoAction()
    {

    }

    public virtual void End_DoAction()
    {
        state.SetIdleMode();

        Move();
    }

    public virtual void Play_Particle()
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