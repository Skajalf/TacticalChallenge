using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header(" Weapon Data Setting")]
    [SerializeField] protected float power; //무기 데미지
    [SerializeField] protected float megazine; //탄창 (근접무기는 사용 X)
    [SerializeField] protected float ammo; // 현재 잔탄수 (근접무기는 사용 X)
    [SerializeField] protected float reloadTime; // 재장전 시간 (근접무기는 사용 X)
    [SerializeField] protected float distance; // 사거리

    [Header(" Weapon Visuals")]
    [SerializeField] protected GameObject projectilePrefab; // 탄환 프리펩
    [SerializeField] protected GameObject cartrigeParticle; //탄피 프리펩
    [SerializeField] protected string weaponHolsterName = "Bip001_Weapon"; // 총의 위치 이름
    [SerializeField] protected string bulletTransformName = "fire_01"; // 총알이 소환되는 위치 이름
    [SerializeField] protected string cartrigeTransformName = "fire_02"; // 탄피가 소환되는 위치 이름
    [SerializeField] protected GameObject flameParticle; // 총구 화염 이펙트
    protected GameObject rootObject;
    protected Transform weaponTransform; // 총의 위치
    protected Transform bulletTransform; // 탄환의 발사 위치
    protected Transform cartrigePoint; // 탄피의 발사 위치

    [Header(" Impulse Setting")] // 카메라 쉐이크 효과 구현
    [SerializeField] protected Vector3 impulseDirection;
    [SerializeField] protected Cinemachine.NoiseSettings impulseSettings;

    [Header(" Impact Setting")] // 피격 대상 이펙트 구현
    [SerializeField] protected int hitImpactIndex; // 피격타입 - 권총인지, 소총인지
    [SerializeField] protected GameObject hitParticle; // 피격 표시 뜨는거
    [SerializeField] protected GameObject damageParticle; //피격 파티클 데미지 67, 105 뜨는 그거
    [SerializeField] protected Vector3 hitParticlePositionOffset; // 피격 파티클의 위치
    [SerializeField] protected Vector3 hitParticleScaleOffset = Vector3.one; // 피격 파티클의 사이즈

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
