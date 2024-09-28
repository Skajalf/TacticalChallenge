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
    [SerializeField] private bool[] weaponActiveOnStart; // 무기 활성화 여부 배열

    private List<WeaponBase> weapons = new List<WeaponBase>();
    private int currentWeaponIndex = 0; // 현재 장착된 무기 인덱스
    private WeaponBase detectedWeapon; // 감지된 무기

    private PlayerInput playerInput;
    private InputActionMap playerInputActionMap;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInputActionMap = playerInput.actions.FindActionMap("Player");

        //InputAction attack = playerInputActionMap.FindAction("Attack");
        //attack.started += Attack_Start;
        //attack.canceled += Attack_Cancel;

        InputAction weaponSwap = playerInputActionMap.FindAction("WeaponSwap");
        weaponSwap.started += weaponSwap_Start;

        InputAction weaponPickUp = playerInputActionMap.FindAction("Action");
        weaponPickUp.started += weaponPickUp_Start;

        //InputAction aim = playerInputActionMap.FindAction("Aim");
        //aim.started += Aim_Start;
        //aim.canceled += Aim_Cancel;

        //InputAction reloadAction = playerInputActionMap.FindAction("Reload");
        //reloadAction.started += Reload_Start;

        Equip();
    }

    private void Update()
    {
        // 매 프레임마다 주변의 무기를 감지
        DetectWeaponInRange();
    }

    private void DetectWeaponInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1.0f);
        detectedWeapon = null; // 초기화

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Weapon")) // "Weapon" 레이어 확인
            {
                detectedWeapon = hitCollider.GetComponent<WeaponBase>();
                if (detectedWeapon != null)
                {
                    break; // 감지된 무기를 찾았으면 루프 종료
                }
            }
        }
    }

    private void Attack_Start(InputAction.CallbackContext context)
    {
        
    }

    private void Attack_Cancel(InputAction.CallbackContext context)
    {
        
    }

    private void weaponSwap_Start(InputAction.CallbackContext context)
    {
        // 눌린 키가 숫자 키(1~9 등)인지 확인
        int weaponIndex;
        if (int.TryParse(context.control.name, out weaponIndex))
        {
            // 무기 인덱스로 사용하기 위해 1을 빼줌 (1번 키 -> 0번 무기, 2번 키 -> 1번 무기, ...)
            weaponIndex -= 1;

            // 유효한 무기 인덱스인지 확인
            if (weaponIndex >= 0 && weaponIndex < weapons.Count)
            {
                SwapWeapon(weaponIndex);
            }
        }
    }

    private void weaponPickUp_Start(InputAction.CallbackContext context)
    {
        // 감지된 무기가 있을 경우 교체
        if (detectedWeapon != null)
        {
            EquipWeapon(detectedWeapon);
        }
        else
        {
            Debug.Log("주변에 장착할 수 있는 무기가 없습니다.");
        }
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
            // weaponActiveOnStart 배열이 weaponPrefabs와 길이가 같지 않으면 오류 출력
            if (weaponActiveOnStart.Length != weaponPrefabs.Length)
            {
                Debug.LogError("weaponActiveOnStart 배열의 크기는 weaponPrefabs 배열의 크기와 일치해야 합니다.");
                return;
            }

            for (int i = 0; i < weaponPrefabs.Length; i++)
            {
                GameObject weaponInstance = Instantiate(weaponPrefabs[i]);
                weaponInstance.name = weaponPrefabs[i].name;
                WeaponBase weapon = weaponInstance.GetComponent<WeaponBase>();

                if (weapon != null)
                {
                    weaponInstance.transform.SetParent(transform); // Set the weapon as a child of the player
                    weapon.Equip(); // Call the Equip() method on the weapon
                    weapons.Add(weapon); // Add the weapon to the list of equipped weapons

                    // 무기를 활성화할지 비활성화할지 설정 (weaponActiveOnStart 배열을 사용)
                    weapon.gameObject.SetActive(weaponActiveOnStart[i]);
                }
                else
                {
                    Debug.LogError($"{weaponPrefabs[i].name}에 WeaponBase가 할당되지 않았습니다.");
                }
            }
        }
        else
        {
            Debug.LogError("무기 프리팹이 설정되지 않았습니다.");
        }
    }

    private void SwapWeapon(int newWeaponIndex)
    {
        if (newWeaponIndex >= 0 && newWeaponIndex < weapons.Count && newWeaponIndex != currentWeaponIndex)
        {
            // 현재 무기 비활성화
            weapons[currentWeaponIndex].gameObject.SetActive(false);

            // 새로운 무기 활성화
            currentWeaponIndex = newWeaponIndex;
            weapons[currentWeaponIndex].gameObject.SetActive(true);
        }
    }

    //// 무기 교체 메서드
    //private void SwapWeapon(int newWeaponIndex)
    //{
    //    if (newWeaponIndex >= 0 && newWeaponIndex < weapons.Count && newWeaponIndex != currentWeaponIndex)
    //    {
    //        // 현재 무기 해제
    //        weapons[currentWeaponIndex].UnEquip();

    //        // 새로운 무기 인덱스로 변경
    //        currentWeaponIndex = newWeaponIndex;

    //        // 새로운 무기 장착
    //        weapons[currentWeaponIndex].Equip();

    //        // 무기 가시성 업데이트
    //        for (int i = 0; i < weapons.Count; i++)
    //        {
    //            weapons[i].gameObject.SetActive(i == currentWeaponIndex);
    //        }
    //    }
    //}

    private void EquipWeapon(WeaponBase newWeapon)
    {
        // 현재 장착된 무기를 비활성화
        weapons[currentWeaponIndex].UnEquip(); // 현재 무기 UnEquip

        // 새로운 무기를 리스트에 추가하고 활성화
        weapons[currentWeaponIndex] = newWeapon; // 새로운 무기를 현재 슬롯에 추가
        newWeapon.transform.SetParent(transform); // 새로운 무기의 부모를 플레이어로 설정
        newWeapon.Equip(); // 새로운 무기를 장착

        // 새로운 무기 활성화
        newWeapon.gameObject.SetActive(true);
        Debug.Log($"{newWeapon.name} 무기를 장착했습니다.");
    }
}