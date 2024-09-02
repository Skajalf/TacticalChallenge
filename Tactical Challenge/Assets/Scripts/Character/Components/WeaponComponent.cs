using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponComponent : MonoBehaviour
{
    public Weapon weapon;

    private Transform weaponTransform; // CH0137_weapon같은 무기 모델 Transform
    private Transform BIPTransform; // Bip001_weapon 같은 무기 리깅정보가 담긴 Transform
    public Player Character { get; private set; } // 무기를 들고 있는 캐릭터모델 정보

    private bool bIsEquip = false;

    private PlayerInput playerInput;
    private InputActionMap playerInputActionMap;
    
    private Animator animator;

    private void Awake()
    {
        Init();
        EventInit();
    }

    private void Init()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInputActionMap = playerInput.actions.FindActionMap("Player");

        weaponTransform = transform.FindChildByName($"{gameObject.name}_Weapon");
        Character = GetComponent<Player>();

        animator = GetComponent<Animator>();

        if (weaponTransform == null) // 어째선지 비어있으면 추가해주는 것 
        {
            GameObject go = Resources.Load<GameObject>($"Prefabs/{gameObject.name}_Weapon");
            GameObject gopre = Instantiate<GameObject>(go, gameObject.transform);

            int index = gopre.name.IndexOf("(Clone)");
            if (index > 0)
                gopre.name = gopre.name.Substring(0, index);

            weaponTransform = gopre.transform;
        }

        weapon = weaponTransform.GetComponent<Weapon>(); // weapon 정보 가져오기

        BIPTransform = transform.FindChildByName($"{Character.CodeName}_Weapon"); // BIP 가져오기
        bIsEquip = true; // 장착완료
    }

    private void EventInit()
    {
        InputAction attack = playerInputActionMap.FindAction("Attack"); // Shoot으로 바꿀 것.
        attack.started += StartShoot;
        attack.canceled += CancelShoot;

        InputAction aim = playerInputActionMap.FindAction("Aim");
        aim.started += StartAim;
        aim.canceled += CancelAim;

        InputAction ReloadAction = playerInputActionMap.FindAction("Reload");
        ReloadAction.started += Reload;
    }

    private void UnEquip()
    {
        bIsEquip = false;
        weaponTransform = null;
    }

    private void Equip()
    {
        weaponTransform = transform.FindChildByName($"{Character.CodeName}_Weapon");
        bIsEquip = true;
    }

    private void StartShoot(InputAction.CallbackContext context)
    {
        if(bIsEquip)
        {
            if (weapon.weapondata.currentAmmo > 0)
                animator.SetBool("IsAttack", true);
            else
                weapon.Reload();
        }
    }

    public void Shoot()
    {
        if(bIsEquip)
        {
            weapon.CheckAmmoWhileShoot();
        }
        return;
    }

    private void CancelShoot(InputAction.CallbackContext context)
    {
        if (bIsEquip)
        {
            weapon.End_DoAction();
            animator.SetBool("IsAttack", false);
        }
    }

    private void StartAim(InputAction.CallbackContext context)
    {
        if (bIsEquip)
        {
            
        }
    }

    private void CancelAim(InputAction.CallbackContext context)
    {
        if (bIsEquip)
        {

        }
    }

    private void Reload(InputAction.CallbackContext context)
    {
        if (weapon != null && !weapon.gameObject.activeInHierarchy)
        {
            return;
        }

        if (bIsEquip)
        {
            weapon.Reload();
        }
    }
}
