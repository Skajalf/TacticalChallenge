using System;
using UnityEngine;

[Serializable]
public class WeaponData
{
    [Header(" Weapon Setting")]
    public bool bCanMove = true; // 움직일 수 있나요?
    public float Power; // 데미지
    public float Ammo; // 최대 탄창
    public float currentAmmo; // 현재 잔탄
    public float ReloadTime; // 재장전 시간
    public float Distance; // 사거리
    public int StopFrame; // 적 맞출때마다 1~2프레임정도 히트스탑

    public GameObject Particle; // 파티클 출력

    [Header(" Impulse Setting")]
    public Vector3 ImpulseDirection;
    public Cinemachine.NoiseSettings ImpulseSettings;

    [Header(" Impact Setting")]
    public int HitImpactIndex;

    public GameObject HitParticle; // 이 총으로 맞았을때 출력될 파티클
    public Vector3 HitParticlePositionOffset;
    public Vector3 HitParticleScaleOffset = Vector3.one;
}

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected WeaponType type;
    [SerializeField] protected WeaponData weapondata;
    [SerializeField] private int damage;

    public WeaponType Type { get => type; }

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