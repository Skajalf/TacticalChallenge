using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    Unarmed = 0, HG, SMG, AR, SR, SG, MG, GL, RL, RG, MT, FT, Max
}

public class WeaponComponent : MonoBehaviour
{
    [SerializeField] private GameObject[] originPrefabs;

    private Animator animator;
    private StateComponent state;

    private WeaponType type = WeaponType.Unarmed;
    public WeaponType Type { get => type; }

    private Dictionary<WeaponType, Weapon> weaponTable;
    public event Action<WeaponType, WeaponType> OnWeaponTyeChanged;

    public event Action OnEndEquip;
    public event Action OnEndDoAction;

    public bool UnarmedMode { get => type == WeaponType.Unarmed; }
    public bool HGMode { get => type == WeaponType.HG; }
    public bool SMGMode { get => type == WeaponType.SMG; }
    public bool ARMode { get => type == WeaponType.AR; }
    public bool SRMode { get => type == WeaponType.SR; }
    public bool SGMode { get => type == WeaponType.SG; }
    public bool MGMode { get => type == WeaponType.MG; }
    public bool GLMode { get => type == WeaponType.GL; }
    public bool RLMode { get => type == WeaponType.RL; }
    public bool RGMode { get => type == WeaponType.RG; }
    public bool MTMode { get => type == WeaponType.MT; }
    public bool FTMode { get => type == WeaponType.FT; }

    public bool IsEquippingMode()
    {
        if (UnarmedMode) return false;

        Weapon weapon = weaponTable[type];
        if(weapon == null) return false;

        return weapon.Equipping;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
    }

    private void Start()
    {
        weaponTable = new Dictionary<WeaponType, Weapon>();

        for (int i = 0; i < originPrefabs.Length; i++)
        {
            GameObject obj = Instantiate<GameObject>(originPrefabs[i], transform);
            Weapon weapon = obj.GetComponent<Weapon>();
            obj.name = weapon.Type.ToString();

            weaponTable[weapon.Type] = weapon;
        }
    }

    private void SetMode(WeaponType type)
    {
        if (this.type == type)
        {
            SetUnarmedMode();

            return;
        }
        else if (UnarmedMode == false)
        {
            weaponTable[this.type].UnEquip();
        }

        if (weaponTable[type] == null)
        {
            SetUnarmedMode();

            return;
        }

        //TODO: 상호작용키로 무기 교체 가능하도록 변경.
        //animator.SetBool("IsEquipping", true);
        //animator.SetInteger("WeaponType", (int)type);

        weaponTable[type].Equip();


        ChangeType(type);
    }

    private void ChangeType(WeaponType type)
    {
        if (this.type == type)
            return;


        WeaponType prevType = this.type;
        this.type = type;

        OnWeaponTyeChanged?.Invoke(prevType, type);
    }

    public void SetUnarmedMode()
    {
        if (state.IdleMode == false)
            return;


        animator.SetInteger("WeaponType", (int)WeaponType.Unarmed);

        if (weaponTable[type] != null)
            weaponTable[type].UnEquip();


        ChangeType(WeaponType.Unarmed);
    }

    public void SetHGMode()
    {
        if(state.IdleMode == false) return;

        SetMode(WeaponType.HG);
    }

    public void SetSMGMode()
    {
        if (state.IdleMode == false) return;

        SetMode(WeaponType.SMG);
    }

    public void SetARMode()
    {
        if (state.IdleMode == false) return;

        SetMode(WeaponType.AR);
    }

    public void SetSRMode()
    {
        if (state.IdleMode == false) return;

        SetMode(WeaponType.SR);
    }

    public void SetSGMode()
    {
        if (state.IdleMode == false) return;

        SetMode(WeaponType.SG);
    }

    public void SetMGMode()
    {
        if (state.IdleMode == false) return;

        SetMode(WeaponType.MG);
    }

    public void SetGLMode()
    {
        if (state.IdleMode == false) return;

        SetMode(WeaponType.GL);
    }

    public void SetRLMode()
    {
        if (state.IdleMode == false) return;

        SetMode(WeaponType.RL);
    }

    public void SetRGMode()
    {
        if (state.IdleMode == false) return;

        SetMode(WeaponType.RG);
    }

    public void SetMTMode()
    {
        if (state.IdleMode == false) return;

        SetMode(WeaponType.MT);
    }

    public void SetFTMode()
    {
        if (state.IdleMode == false) return;

        SetMode(WeaponType.FT);
    }

    public void Begin_Equip()
    {
        weaponTable[type].Begin_Equip();
    }

    public void End_Equip()
    {
        //TODO : 트랜지션 확인 필요
        //animator.SetBool("IsEquipping", false);

        weaponTable[type].End_Equip();
        OnEndEquip?.Invoke();
    }

    public void DoAction()
    {
        if (weaponTable[type] == null) return;

        if (weaponTable[type].CanDoAction() == false) return;

        //animator.SetBool("IsAction", true);
        weaponTable[type].DoAction();
    }

    public void Begin_DoAction()
    {
        weaponTable[type].Begin_DoAction();
    }

    public void End_DoAction()
    {
        //animator.SetBool("IsAction", false);

        weaponTable[type].End_DoAction();
        OnEndDoAction?.Invoke();
    }
}
