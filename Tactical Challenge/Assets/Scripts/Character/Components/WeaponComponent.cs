using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponComponent : MonoBehaviour
{
    private Transform weaponModel; // CH0137_weapon���� ���� ��
    private Transform weaponTransform; // Bip001_weapon ���� ���� ���������� ��� ������Ʈ
    private Player character; // ���⸦ ��� �ִ� ĳ���͸� ����

    private PlayerInput playerInput;

    private void Awake()
    {
        Init();
    }

    private void Init() // ���⿡ ���� ������ �����´�. ���ٸ�, �߰��ؼ� �����´�.
    {
        playerInput = GetComponent<PlayerInput>();

        weaponModel = transform.FindChildByName($"{gameObject.name}_Weapon");
        if( weaponModel == null )
        {
            GameObject go = Resources.Load<GameObject>($"Prefabs/{gameObject.name}_Weapon");
            GameObject gopre = Instantiate<GameObject>(go, gameObject.transform);

            int index = gopre.name.IndexOf("(Clone)");
            if (index > 0)
                gopre.name = gopre.name.Substring(0, index);

            weaponModel = gopre.transform;
        }

        character = GetComponent<Player>();
        weaponTransform = transform.FindChildByName($"{character.CodeName}_Weapon");
    }

    /*
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
        animator = GetComponent<Animator>(); // ������� �߰���.
        state = GetComponent<StateComponent>();
    }

    private void Start()
    {

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

    public void Begin_Equip()
    {
        weaponTable[type].Begin_Equip();
    }

    public void End_Equip()
    {
        //TODO : Ʈ������ Ȯ�� �ʿ�
        //animator.SetBool("IsEquipping", false);

        weaponTable[type].End_Equip();
        OnEndEquip?.Invoke();
    }
*/
}
