using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Cinemachine;

[Serializable]
public class WeaponData // 무기 데이터
{
    public bool bCanMove;

    [Header(" Data Setting")]
    public float Power; // 데미지
    public float Ammo; // 최대 탄약량
    public float currentAmmo; // 현재 잔탄
    public float ReloadTime; // 재장전 시간
    public float Distance; // 사거리
    public int StopFrame; // 적한테 총알이 맞았을때 히트스탑

    [Header(" Visuals")]
    public GameObject projectilePrefab; // 발사체 프리팹
    public Transform firePoint;         // 발사 위치
    public GameObject Particle; // 총구 화염
    public GameObject CartrigeCase; // 탄피

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

        // 추가된 부분 fire_01을 가져옴.
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