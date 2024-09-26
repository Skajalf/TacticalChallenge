using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.TextCore.Text;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class WeaponComponent : MonoBehaviour
{
    [SerializeField] private GameObject[] weaponPrefabs;

    private List<WeaponBase> weapons = new List<WeaponBase>();
    //private int currentWeaponIndex = 0; // 현재 장착된 무기 인덱스 무기 교체 테스트용

    //private PlayerInput playerInput;
    //private InputActionMap playerInputActionMap;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        //playerInput = GetComponent<PlayerInput>();
        //playerInputActionMap = playerInput.actions.FindActionMap("Player");

        //InputAction attack = playerInputActionMap.FindAction("Attack");
        //attack.started += Attack_Start;
        //attack.canceled += Attack_Cancel;

        //InputAction aim = playerInputActionMap.FindAction("Aim");
        //aim.started += Aim_Start;
        //aim.canceled += Aim_Cancel;

        //InputAction reloadAction = playerInputActionMap.FindAction("Reload");
        //reloadAction.started += Reload_Start;

        Equip();
    }

    private void Attack_Start(InputAction.CallbackContext context)
    {
        
    }

    private void Attack_Cancel(InputAction.CallbackContext context)
    {
        
    }

    private void Aim_Start(InputAction.CallbackContext context)
    {
        
    }

    private void Aim_Cancel(InputAction.CallbackContext context)
    {
        
    }

    private void Reload_Start(InputAction.CallbackContext context)
    {
        
    }

    private void Equip()
    {
        if (weaponPrefabs != null && weaponPrefabs.Length > 0)
        {
            foreach (var weaponPrefab in weaponPrefabs)
            {
                GameObject weaponInstance = Instantiate(weaponPrefab);
                weaponInstance.name = weaponPrefab.name;
                WeaponBase weapon = weaponInstance.GetComponent<WeaponBase>();

                if (weapon != null)
                {
                    weaponInstance.transform.SetParent(transform); // Set the weapon as a child of the player
                    weapon.Equip(); // Call the Equip() method on the weapon
                    weapons.Add(weapon); // Add the weapon to the list of equipped weapons
                }
                else
                {
                    Debug.LogError($"{weaponPrefab.name}에 WeaponBase가 할당되지 않았습니다.");
                }

                // 0번 무기만 활성화하고 나머지는 비활성화
                for (int i = 0; i < weapons.Count; i++)
                {
                    weapons[i].gameObject.SetActive(i == 0);
                }
            }
        }
        else
        {
            Debug.LogError("무기 프리팹이 설정되지 않았습니다.");
        }
    }

    //무기 교체 메서드 InputSystem쓸때 용으로 테스트 해봄
    //private void UpdateWeaponVisibility()
    //{
    //    for (int i = 0; i < weapons.Count; i++)
    //    {
    //        weapons[i].gameObject.SetActive(i == currentWeaponIndex);
    //    }
    //}

    //
    //public void SwapWeapon(int newWeaponIndex)
    //{
    //    if (newWeaponIndex >= 0 && newWeaponIndex < weapons.Count && newWeaponIndex != currentWeaponIndex)
    //    {
    //        weapons[currentWeaponIndex].UnEquip(); // 현재 무기 해제
    //        currentWeaponIndex = newWeaponIndex; // 새로운 무기 인덱스로 변경
    //        weapons[currentWeaponIndex].Equip(); // 새로운 무기 장착
    //        UpdateWeaponVisibility(); // 무기 가시성 업데이트
    //    }
    //}

    //private void Update()
    //{
    //    // 1번 키를 눌렀을 때
    //    if (Keyboard.current[Key.Numpad1].wasPressedThisFrame)
    //    {
    //        SwapWeapon(0);
    //    }

    //    // 2번 키를 눌렀을 때
    //    if (Keyboard.current[Key.Numpad2].wasPressedThisFrame)
    //    {
    //        SwapWeapon(1);
    //    }
    //}
}