using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class WeaponData // 무기 데이터
{
    public bool bCanMove;

    public float Power; // 데미지
    public float Ammo; // 최대 탄약량
    public float currentAmmo; // 현재 잔탄
    public float ReloadTime; // 재장전 시간
    public float Distance; // 사거리
    public int StopFrame; // 적한테 총알이 맞았을때 히트스탑

    public GameObject Particle; // 총구 화염

    [Header(" Impulse Setting")] // 카메라 쉐이킹(반동?)
    public Vector3 ImpulseDirection;
    public Cinemachine.NoiseSettings ImpulseSettings;

    [Header(" Impact Setting")] // 이 총으로 맞았을때 이펙트
    public int HitImpactIndex;

    public GameObject HitParticle;
    public Vector3 HitParticlePositionOffset;
    public Vector3 HitParticleScaleOffset = Vector3.one;
}

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] public WeaponData weapondata;
    [SerializeField] private int damage;

    public int Damage
    {
        get { return damage; }
    }

    public bool Equipping { get => bEquipping; }
    private bool bEquipping;
    protected bool bEquipped;

    public bool IsReload { private set; get; }

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

    public void CheckAmmoWhileShoot() // 잔탄 확인하고 쏘는 것!
    {
        if (weapondata.currentAmmo > 0)
        {
            weapondata.currentAmmo -= 1;
            Begin_DoAction();
        }
        else
            animator.SetBool("IsAttack", false);
    }

    public void Reload()
    {
        //TODO: 탄창이 꽉찼는지 확인하고 맞으면 return, 상태이상 상태인지 확인하고 맞으면 return
        IsReload = true;
        animator.SetTrigger("Reload");
        weapondata.currentAmmo = weapondata.Ammo;
        IsReload = false;
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