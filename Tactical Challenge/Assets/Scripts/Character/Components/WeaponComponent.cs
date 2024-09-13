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
    private StateComponent playerState;

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

        playerState = Character.gameObject.GetComponent<StateComponent>();

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
        attack.started += StartAttack;
        attack.canceled += CancelAttack;

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

    private void StartAttack(InputAction.CallbackContext context)
    {
        if (bIsEquip && playerState.CanDoSomething())
        {
            if (weapon.weapondata.currentAmmo > 0)
                animator.SetBool("IsAttack", true);
            else
                weapon.Reload();
        }
    }

    private void CancelAttack(InputAction.CallbackContext context)
    {
        if (bIsEquip)
        {
            weapon.EndDoAction();
            animator.SetBool("IsAttack", false);
        }
    }

    /// <summary>
    /// 애니메이터에서 호출하는 메서드이다. 건들지 말 것.
    /// </summary>
    public void Shoot()
    {
        if (bIsEquip)
        {
            weapon.CheckAmmo();
        }
        return;
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
        if (bIsEquip && playerState.CanDoSomething() && (weapon.weapondata.currentAmmo != weapon.weapondata.Ammo))
        {
            weapon.Reload();
            animator.SetTrigger("Reload");  // 재장전 애니메이션 트리거
        }
        else
            return;
    }

    /// <summary>
    /// 플레이어가 현재 들고있는 무기 정보 (Weapon)을 반환한다.
    /// </summary>
    /// <returns>Weapon, null</returns>
    public Weapon GetWeaponInfo()
    {
        if(weapon != null)
            return weapon;
        return null;
    }


}
