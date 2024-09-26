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
    //private int currentWeaponIndex = 0; // ���� ������ ���� �ε��� ���� ��ü �׽�Ʈ��

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
                    Debug.LogError($"{weaponPrefab.name}�� WeaponBase�� �Ҵ���� �ʾҽ��ϴ�.");
                }

                // 0�� ���⸸ Ȱ��ȭ�ϰ� �������� ��Ȱ��ȭ
                for (int i = 0; i < weapons.Count; i++)
                {
                    weapons[i].gameObject.SetActive(i == 0);
                }
            }
        }
        else
        {
            Debug.LogError("���� �������� �������� �ʾҽ��ϴ�.");
        }
    }

    //���� ��ü �޼��� InputSystem���� ������ �׽�Ʈ �غ�
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
    //        weapons[currentWeaponIndex].UnEquip(); // ���� ���� ����
    //        currentWeaponIndex = newWeaponIndex; // ���ο� ���� �ε����� ����
    //        weapons[currentWeaponIndex].Equip(); // ���ο� ���� ����
    //        UpdateWeaponVisibility(); // ���� ���ü� ������Ʈ
    //    }
    //}

    //private void Update()
    //{
    //    // 1�� Ű�� ������ ��
    //    if (Keyboard.current[Key.Numpad1].wasPressedThisFrame)
    //    {
    //        SwapWeapon(0);
    //    }

    //    // 2�� Ű�� ������ ��
    //    if (Keyboard.current[Key.Numpad2].wasPressedThisFrame)
    //    {
    //        SwapWeapon(1);
    //    }
    //}
}