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
    private WeaponBase currentWeapon; // 감지된 무기

    private PlayerInput playerInput;
    private InputActionMap playerInputActionMap;

    private Animator animator;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInputActionMap = playerInput.actions.FindActionMap("Player");

        InputAction attack = playerInputActionMap.FindAction("Attack");
        attack.started += Attack_Start;
        attack.canceled += Attack_Cancel;

        InputAction weaponSwap = playerInputActionMap.FindAction("WeaponSwap");
        weaponSwap.started += weaponSwap_Start;

        InputAction weaponPickUp = playerInputActionMap.FindAction("Action");
        weaponPickUp.started += weaponPickUp_Start;

        InputAction aim = playerInputActionMap.FindAction("Aim");
        aim.started += Aim_Start;
        aim.canceled += Aim_Cancel;

        InputAction reloadAction = playerInputActionMap.FindAction("Reload");
        reloadAction.started += Reload_Start;

        animator = GetComponent<Animator>();

        Equip();

        foreach (var weapon in weapons)
        {
            weapon.ammo = weapon.megazine;
        }

        // 초기 장착 무기를 currentWeapon에 할당
        if (weapons.Count > 0)
        {
            currentWeapon = weapons[currentWeaponIndex];
        }
    }

    private void Update()
    {
        // 매 프레임마다 주변의 무기를 감지
        DetectWeaponInRange();

        if (currentWeapon == null)
        {
            Debug.LogWarning("Update 중 currentWeapon이 null로 확인되었습니다.");
        }
    }

    private void DetectWeaponInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1.0f);
        currentWeapon = null; // 초기화

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Weapon")) // "Weapon" 레이어 확인
            {
                currentWeapon = hitCollider.GetComponent<WeaponBase>();
                if (currentWeapon != null)
                {
                    break; // 감지된 무기를 찾았으면 루프 종료
                }
            }
        }
    }

    private void Attack_Start(InputAction.CallbackContext context)
    {
        animator.SetBool("IsAttack", true);
    }

    private void Attack_Cancel(InputAction.CallbackContext context)
    {
        animator.SetBool("IsAttack", false);
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
        if (currentWeapon != null)
        {
            EquipWeapon(currentWeapon);
        }
        else
        {
            Debug.Log("주변에 장착할 수 있는 무기가 없습니다.");
        }
    }

    private void Aim_Start(InputAction.CallbackContext context)
    {
        HandIKComponent handIK = GetComponent<HandIKComponent>();
        handIK.UpdateAimRigWeight(true); // 에이밍 시작 시 Weight를 1로 변경
    }

    private void Aim_Cancel(InputAction.CallbackContext context)
    {
        HandIKComponent handIK = GetComponent<HandIKComponent>();
        handIK.UpdateAimRigWeight(false); // 에이밍 취소 시 Weight를 0으로 변경
    }

    private void Reload_Start(InputAction.CallbackContext context)
    {
        if (/*bIsEquip && playerState.CanDoSomething() &&*/ (currentWeapon.ammo != currentWeapon.megazine))
        {
            currentWeapon.Reload();
            animator.SetTrigger("Reload");
        }
        else
            return;
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
                    // FindChildByName을 사용하여 WeaponPivot을 찾기
                    Transform weaponPivot = transform.FindChildByName("WeaponPivot");
                    if (weaponPivot != null)
                    {
                        // 무기를 WeaponPivot의 자식으로 설정
                        weaponInstance.transform.SetParent(weaponPivot, false);
                    }
                    else
                    {
                        Debug.LogError("WeaponPivot을 찾을 수 없습니다.");
                    }

                    weapon.Equip(); // Call the Equip() method on the weapon
                    weapons.Add(weapon); // Add the weapon to the list of equipped weapons

                    // 무기를 활성화할지 비활성화할지 설정 (weaponActiveOnStart 배열을 사용)
                    weapon.gameObject.SetActive(weaponActiveOnStart[i]);

                    // 첫 번째 무기 장착 후 IK 타겟 업데이트
                    if (i == 0)
                    {
                        // HandIKComponent의 IK 타겟 갱신 및 리그 빌드
                        HandIKComponent handIK = GetComponent<HandIKComponent>();
                        handIK.UpdateWeaponIKTargets(); // 초기 무기 장착 시 IK 타겟 업데이트
                        handIK.ApplyWeaponOffsets(); // 무기 오프셋 업데이트 추가
                        handIK.BuildRig(); // 리그 빌드 호출
                    }
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

        // weapons 리스트에 무기들이 추가된 후 currentWeapon 설정
        if (weapons.Count > 0)
        {
            currentWeapon = weapons[currentWeaponIndex];
            Debug.Log($"초기 장착 무기: {currentWeapon.name}");
        }
        else
        {
            Debug.LogWarning("weapons 리스트에 무기가 없습니다.");
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
            currentWeapon = weapons[currentWeaponIndex];
            currentWeapon.gameObject.SetActive(true);

            currentWeapon = weapons[currentWeaponIndex]; // 현재 무기 업데이트

            // HandIKComponent의 IK 타겟 갱신 및 리그 빌드
            HandIKComponent handIK = GetComponent<HandIKComponent>();
            handIK.UpdateWeaponIKTargets();
            handIK.ApplyWeaponOffsets(); // 오프셋 설정 호출
            handIK.BuildRig(); // 리그 빌드 호출
        }
    }

    private void EquipWeapon(WeaponBase newWeapon)
    {
        weapons[currentWeaponIndex].UnEquip();

        weapons[currentWeaponIndex] = newWeapon;
        newWeapon.transform.SetParent(transform);
        newWeapon.Equip();

        currentWeapon = newWeapon; // 현재 무기 업데이트

        newWeapon.gameObject.SetActive(true);
        Debug.Log($"{newWeapon.name} 무기를 장착했습니다.");

        // HandIKComponent의 IK 타겟 갱신 및 리그 빌드
        HandIKComponent handIK = GetComponent<HandIKComponent>();
        handIK.UpdateWeaponIKTargets();
        handIK.ApplyWeaponOffsets();
        handIK.BuildRig(); // 리그 빌드 호출
    }

    public WeaponBase GetActiveWeapon()
    {
        if (currentWeaponIndex >= 0 && currentWeaponIndex < weapons.Count)
        {
            return weapons[currentWeaponIndex];
        }
        return null;
    }

    ///
    /// 애니메이션 메서드
    ///

    public void Attack()
    {
        // currentWeapon이 유효하면 CheckAmmo() 호출
        if (currentWeapon != null)
        {
            currentWeapon.CheckAmmo();
        }
        else
        {
            Debug.LogWarning("현재 장착된 무기가 없습니다.");
        }
    }

    public void Impulse()
    {
        //if (bIsEquip)
        //{
            currentWeapon.makeImpulse();
        //}
        return;
    }
}