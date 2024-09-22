using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.TextCore.Text;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class WeaponComponent : MonoBehaviour
{
    [SerializeField] private GameObject weaponPrefab;
    
    private WeaponBase weapon;

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
        if (weaponPrefab != null)
        {
            GameObject weaponInstance = Instantiate(weaponPrefab);
            weaponInstance.name = weaponPrefab.name;
            weapon = weaponInstance.GetComponent<WeaponBase>();

            if (weapon != null)
            {
                weaponInstance.transform.SetParent(transform); // ���⸦ �÷��̾ �ڽ����� ���̱�
                weapon.Equip(); // Equip() ȣ��
            }
            else
            {
                Debug.LogError("WeaponBase�� �Ҵ���� �ʾҽ��ϴ�.");
            }
        }
        else
        {
            Debug.LogError("���� �������� �������� �ʾҽ��ϴ�.");
        }
    }
}
