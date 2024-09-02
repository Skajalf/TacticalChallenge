using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[Serializable]
public class WeaponData // ���� ������
{
    public bool bCanMove;

    public float Power; // ������
    public float Ammo; // �ִ� ź�෮
    public float currentAmmo; // ���� ��ź
    public float ReloadTime; // ������ �ð�
    public float Distance; // ��Ÿ�
    public int StopFrame; // ������ �Ѿ��� �¾����� ��Ʈ��ž

    //public GameObject Particle; // �ѱ� ȭ��

    //[Header(" Impulse Setting")] // ī�޶� ����ŷ(�ݵ�?)
    //public Vector3 ImpulseDirection;
    //public Cinemachine.NoiseSettings ImpulseSettings;

    //[Header(" Impact Setting")] // �� ������ �¾����� ����Ʈ
    //public int HitImpactIndex;

    //public GameObject HitParticle;
    //public Vector3 HitParticlePositionOffset;
    //public Vector3 HitParticleScaleOffset = Vector3.one;
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
    protected bool bEquipping;
    protected bool bEquipped;

    public bool IsReload { protected set; get; }

    protected GameObject rootObject;
    protected StateComponent state;
    protected Animator animator;

    protected Transform muzzleTransform;
    protected Vector3 muzzlePosition;

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

    public void CheckAmmoWhileShoot() // ��ź Ȯ���ϰ� ��� ��!
    {
        if (weapondata.currentAmmo > 0)
        {
            weapondata.currentAmmo -= 1;
            Begin_DoAction();
        }
        else
            animator.SetBool("IsAttack", false);
    }

    public virtual void Reload()
    {
        StartCoroutine(OnReload());
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

    private IEnumerator OnReload()
    {
        if (state.CurrentState == StateType.Reload)
            yield break;

        state.SetReloadMode();

        animator.SetTrigger("Reload");

        yield return new WaitForSeconds(weapondata.ReloadTime);

        weapondata.currentAmmo = weapondata.Ammo;
        state.SetIdleMode();
    }
}