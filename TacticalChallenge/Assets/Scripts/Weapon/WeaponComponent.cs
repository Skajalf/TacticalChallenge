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
    private WeaponBase detectedWeapon; // 주변에서 탐색된 무기

    private PlayerInput playerInput;
    private InputActionMap playerInputActionMap;
    
    private bool isCancelAble = false;

    private Animator animator;
    private HandIKComponent handIK;

    // 최초 AnimatorController 저장 변수
    private RuntimeAnimatorController initialAnimatorController;
    private string initialWeaponName; // 최초 무기 이름 저장

    private StatComponent statComponent;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInputActionMap = playerInput.actions.FindActionMap("Player");
        statComponent = GetComponent<StatComponent>();

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
        handIK = GetComponent<HandIKComponent>();

        // ** 최초 AnimatorController 저장 **
        initialAnimatorController = animator.runtimeAnimatorController;


        Equip();

        foreach (var weapon in weapons)
        {
            weapon.ammo = weapon.megazine;
        }

        // 코루틴 시작
        StartCoroutine(CheckForNearbyWeapons());
    }

    private IEnumerator CheckForNearbyWeapons()
    {
        while (true)
        {
            DetectWeaponInRange();
            yield return new WaitForSeconds(1.0f); // 0.5초마다 주기적으로 실행
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 1.25f);
    }

    private void DetectWeaponInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1.25f);
        
        detectedWeapon = null; // 초기화

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Weapon"))
            {
                detectedWeapon = hitCollider.GetComponent<WeaponBase>();
                if (detectedWeapon != null)
                {
                    break; // 감지된 무기를 찾았으면 루프 종료
                }
            }
        }

        if (detectedWeapon == null)
        {
            Debug.Log("주변에 장착할 수 있는 무기가 없습니다.");
        }
    }

    private void Attack_Start(InputAction.CallbackContext context)
    {
        if (isCancelAble) // CancelAble 상태라면 애니메이션을 초기화하고 다시 재생
        {
            Debug.Log("CancelAble 상태: 애니메이션 초기화 후 재생");
            animator.Play("Attack", 0, 0); // "Attack" 클립을 0프레임부터 다시 재생
            isCancelAble = false; // 초기화 후 CancelAble 상태를 false로 변경
            return;
        }

        string currentAnimationName = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        if (!currentAnimationName.Contains("_Attack_Ing"))
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.normalizedTime < 1.0f)
            {
                Debug.Log("애니메이션 진행 중: 중복 실행 방지");
                return;
            }
        }

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
        if (detectedWeapon != null)
        {
            EquipWeapon(detectedWeapon);
            detectedWeapon = null; // 무기를 장착하면 탐색된 무기 초기화
        }
        else
        {
            Debug.Log("주변에 장착할 수 있는 무기가 없습니다.");
        }
    }

    private void Aim_Start(InputAction.CallbackContext context)
    {
        handIK.UpdateAimRigWeight(true); // 에이밍 시작 시 Weight를 1로 변경
    }

    private void Aim_Cancel(InputAction.CallbackContext context)
    {
        handIK.UpdateAimRigWeight(false); // 에이밍 취소 시 Weight를 0으로 변경
    }

    private void Reload_Start(InputAction.CallbackContext context) // 나중에 State조건 넣어주기
    {
        if (currentWeapon == null)
        {
            Debug.LogWarning("현재 장착된 무기가 없습니다. 재장전할 수 없습니다.");
            return;
        }

        if (currentWeapon.ammo == currentWeapon.megazine)
        {
            Debug.Log("탄약이 가득 차 있어 재장전이 필요하지 않습니다.");
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Animator 컴포넌트가 초기화되지 않았습니다.");
            return;
        }

        currentWeapon.Reload();
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

                    // 최초 무기 이름 저장
                    if (i == currentWeaponIndex)
                    {
                        initialWeaponName = weapon.name; // 초기 무기 이름 저장
                    }
                }
                else
                {
                    Debug.LogError($"{weaponPrefabs[i].name}에 WeaponBase가 할당되지 않았습니다.");
                }
            }

            // weapons 리스트에 무기들이 추가된 후 currentWeapon 설정
            if (weapons.Count > 0)
            {
                currentWeapon = weapons[currentWeaponIndex];
                Debug.Log($"초기 장착 무기: {currentWeapon.name}");

                // 비동기적으로 IK 설정
                StartCoroutine(InitializeIKSettings()); // 첫 번째 무기 장착 후 IK 업데이트
            }
            else
            {
                Debug.LogWarning("weapons 리스트에 무기가 없습니다.");
            }

            // 무기들이 모두 장착된 후 IKSettingsUpdate() 호출
            //IKSettingsUpdate();
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
            currentWeapon = weapons[currentWeaponIndex];
            currentWeapon.gameObject.SetActive(true);

            // ** 초기 무기라면 초기 AnimatorController 복원 **
            if (currentWeapon.name == initialWeaponName)
            {
                RestoreInitialAnimatorController();
            }
            else
            {
                ApplyAOCForWeapon(currentWeapon);
            }

            IKSettingsUpdate();
        }
    }

    private void EquipWeapon(WeaponBase newWeapon)
    {
        Transform prevWeapon = weapons[currentWeaponIndex].transform;
        int emptySlotIndex = weapons.FindIndex(w => w == null);

        if (emptySlotIndex != -1)
        {
            weapons[emptySlotIndex] = newWeapon;
        }
        else
        {
            
            weapons[currentWeaponIndex].UnEquip();
            weapons[currentWeaponIndex] = newWeapon;
        }

        newWeapon.transform.SetParent(transform.FindChildByName("WeaponPivot"));
        newWeapon.transform.position = prevWeapon.position;
        newWeapon.transform.rotation = new Quaternion(0, 0, 0, 0);
        newWeapon.Equip();

        currentWeapon = newWeapon;
        currentWeaponIndex = weapons.IndexOf(newWeapon); // 새 무기의 슬롯 인덱스를 저장

        // 무기 활성화
        newWeapon.gameObject.SetActive(true);

        // ** 초기 무기라면 초기 AnimatorController 복원 **
        if (newWeapon.name == initialWeaponName)
        {
            RestoreInitialAnimatorController();
        }
        else
        {
            ApplyAOCForWeapon(currentWeapon);
        }

        Debug.Log($"{newWeapon.name} 무기를 장착했습니다.");

        IKSettingsUpdate();
    }

    private IEnumerator InitializeIKSettings()
    {
        yield return new WaitForEndOfFrame(); // 한 프레임 대기하여 무기 초기화 완료 대기
        IKSettingsUpdate(); // 무기가 제대로 장착된 후 IK 설정 진행
    }

    private void IKSettingsUpdate()
    {
        if (handIK != null)
        {
            handIK.UpdateWeaponIKTargets();
            handIK.ApplyWeaponOffsets();
            handIK.BuildRig();
        }
        else
        {
            Debug.LogWarning("HandIKComponent가 없거나 초기화되지 않았습니다.");
        }
    }

    public WeaponBase GetActiveWeapon() //아마 무기 데미지 넣을때 호출할듯?
    {
        if (currentWeaponIndex >= 0 && currentWeaponIndex < weapons.Count)
        {
            return weapons[currentWeaponIndex];
        }
        return null;
    }

    private void ApplyAOCForWeapon(WeaponBase weapon)
    {
        if (weapon == null || animator == null) return;

        // 현재 캐릭터 이름과 무기 이름 가져오기
        string characterName = gameObject.name; // 캐릭터 이름

        // 무기 이름에서 "_Weapon" 부분 제거
        string weaponName = weapon.name.Replace("_Weapon", "");

        // AOC 이름 형식: {캐릭터이름}_Swap_{무기이름}
        string aocName = $"{characterName}_Swap_{weaponName}";

        // 리소스에서 AOC를 로드
        AnimatorOverrideController aoc = Resources.Load<AnimatorOverrideController>(aocName);

        if (aoc != null)
        {
            animator.runtimeAnimatorController = aoc;

            // **즉시 상태 초기화**
            animator.Rebind();
            animator.Update(0);

            Debug.Log($"AOC 적용: {aocName}");
        }
    }

    private void RestoreInitialAnimatorController()
    {
        if (initialAnimatorController != null)
        {
            animator.runtimeAnimatorController = initialAnimatorController;

            // **즉시 상태 초기화**
            animator.Rebind();
            animator.Update(0);

            Debug.Log($"초기 AnimatorController로 복원: {initialAnimatorController.name}");
        }
        else
        {
            Debug.LogWarning("초기 AnimatorController가 저장되지 않았습니다.");
        }
    }

    ///
    /// 애니메이션 메서드
    ///

    public void Attack()
    {
        // currentWeapon이 유효하면 CheckAmmo() 호출
        if (currentWeapon != null)
        {
            currentWeapon.AmmoLeft();
        }
        else
        {
            Debug.LogWarning("현재 장착된 무기가 없습니다.");
        }
    }

    public void CancelAble()
    {
        isCancelAble = true;
        Debug.Log("CancelAble 상태");
    }

    public void Impulse()
    {
        
    }

    /// <summary>
    /// 무기 스탯을 플레이어 stat에 적용하는 부분
    /// </summary>
    /// <param name="weapon"></param>
    public void ApplyStats(WeaponBase weapon)
    {

        statComponent.HasStat(weapon.ammo);
    }
}