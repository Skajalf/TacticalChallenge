using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RootMotion.FinalIK;

public class WeaponComponent : MonoBehaviour
{
    [Header("Weapon Setup")]
    [SerializeField] private Transform weaponPivot;
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

    private FullBodyBipedIK fbbIK;

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

        fbbIK = GetComponent<FullBodyBipedIK>();
        if (fbbIK == null) Debug.LogError("FullBodyBipedIK 컴포넌트를 찾을 수 없습니다.");

        if (weaponPivot == null) Debug.LogError("WeaponPivot(Transform)을 인스펙터에서 할당하세요!");

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
        if (detectedWeapon == null) return;

        EquipWeapon(detectedWeapon);
        MapWeaponToIK(detectedWeapon);
        detectedWeapon = null;
    }

    private void Aim_Start(InputAction.CallbackContext context)
    {
        
    }

    private void Aim_Cancel(InputAction.CallbackContext context)
    {
        
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
        if (weaponPrefabs == null || weaponPrefabs.Length == 0) return;
        if (weaponActiveOnStart.Length != weaponPrefabs.Length)
        {
            Debug.LogError("weaponActiveOnStart 크기 불일치");
            return;
        }

        for (int i = 0; i < weaponPrefabs.Length; i++)
        {
            var inst = Instantiate(weaponPrefabs[i], weaponPivot, false);
            inst.name = weaponPrefabs[i].name;
            var wb = inst.GetComponent<WeaponBase>();
            if (wb == null) { Destroy(inst); continue; }

            wb.Equip();
            weapons.Add(wb);
            inst.SetActive(weaponActiveOnStart[i]);

            if (i == currentWeaponIndex)
            {
                initialWeaponName = wb.name;
                MapWeaponToIK(wb);
            }
        }

        if (weapons.Count > 0)
        {
            currentWeapon = weapons[currentWeaponIndex];
            Debug.Log($"초기 장착 무기: {currentWeapon.name}");
        }
    }

    private void SwapWeapon(int newIndex)
    {
        if (newIndex == currentWeaponIndex) return;

        weapons[currentWeaponIndex].gameObject.SetActive(false);
        currentWeaponIndex = newIndex;
        currentWeapon = weapons[newIndex];
        currentWeapon.gameObject.SetActive(true);

        // 애니메이터 컨트롤러 복원 또는 AOC 적용
        if (currentWeapon.name == initialWeaponName)
            RestoreInitialAnimatorController();
        else
            ApplyAOCForWeapon(currentWeapon);

        MapWeaponToIK(currentWeapon);
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

    private void MapWeaponToIK(WeaponBase weapon)
    {
        if (fbbIK == null || weapon == null) return;

        var solver = fbbIK.solver;
        var leftGrip = weapon.transform.Find("LeftHandGrip");
        var rightGrip = weapon.transform.Find("RightHandGrip");
        if (leftGrip == null || rightGrip == null)
        {
            Debug.LogError($"{weapon.name}에 Left/RightHandGrip이 없습니다.");
            return;
        }

        solver.leftHandEffector.target = leftGrip;
        solver.rightHandEffector.target = rightGrip;
        solver.leftHandEffector.positionWeight = 1f;
        solver.leftHandEffector.rotationWeight = 1f;
        solver.rightHandEffector.positionWeight = 1f;
        solver.rightHandEffector.rotationWeight = 1f;

        // IK 즉시 갱신
        solver.Initiate(fbbIK.transform);
        solver.Update();
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