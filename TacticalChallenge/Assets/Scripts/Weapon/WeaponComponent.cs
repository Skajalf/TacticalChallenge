using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RootMotion.FinalIK;

public class WeaponComponent : MonoBehaviour
{
    [Header("Weapon Setup")]
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private GameObject weaponPrefab; //고유무기 프리팹

    private GameObject weaponInstance;

    private PivotManager pivotManager;
    private WeaponBase currentWeapon;
    private List<WeaponBase> detectedWeapons = new List<WeaponBase>();

    private PlayerInput playerInput;
    private InputActionMap playerInputActionMap;

    private Animator animator;
    private FullBodyBipedIK fbbIK;
    private AimIK aimIK;
    private StatComponent statComponent;

    private RuntimeAnimatorController initialAnimatorController;
    private string initialWeaponName;

    private bool isCancelAble;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
        fbbIK = GetComponent<FullBodyBipedIK>();
        statComponent = GetComponent<StatComponent>();
        aimIK = GetComponent<AimIK>();

        pivotManager = FindObjectOfType<PivotManager>();
        if (pivotManager == null)
            Debug.LogError("씬에 PivotManager가 없습니다!");

        if (fbbIK == null)
            Debug.LogError("FullBodyBipedIK 컴포넌트를 찾을 수 없습니다.");

        if (weaponPivot == null)
            Debug.LogError("WeaponPivot(Transform)을 인스펙터에서 할당하세요!");

        playerInputActionMap = playerInput.actions.FindActionMap("Player");
        InputAction attack = playerInputActionMap.FindAction("Attack");
        attack.started += Attack_Start;
        attack.canceled += Attack_Cancel;

        InputAction weaponPickUp = playerInputActionMap.FindAction("Action");
        weaponPickUp.started += weaponPickUp_Start;

        InputAction aim = playerInputActionMap.FindAction("Aim");
        aim.started += Aim_Start;
        aim.canceled += Aim_Cancel;

        InputAction reloadAction = playerInputActionMap.FindAction("Reload");
        reloadAction.started += Reload_Start;

        initialAnimatorController = animator.runtimeAnimatorController;

        if (weaponPrefab != null)
        {
            Equip(weaponPrefab);
            initialWeaponName = currentWeapon.name;
            Debug.Log($"초기 장착 무기 : {initialWeaponName}");
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

    public void Equip(GameObject weaponObject, bool instantiate = true)
    {
        // 1) 기존 무기 해제
        UnEquip();

        // 2) Prefab 또는 씬 오브젝트를 instance에 할당
        GameObject instance;
        if (instantiate)
        {
            // 프리팹일 땐 새로 인스턴스화
            instance = Instantiate(weaponObject, weaponPivot, false);
            instance.name = weaponObject.name;
        }
        else
        {
            // 이미 씬에 존재하는 오브젝트
            instance = weaponObject;
            instance.transform.SetParent(weaponPivot, false);

            Rigidbody rb = instance.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;

            Collider col = instance.GetComponent<Collider>();
            if (col != null)
                col.enabled = false;
        }

        // 3) WeaponBase 컴포넌트 가져오기
        WeaponBase wb = instance.GetComponent<WeaponBase>();
        if (wb == null)
        {
            Debug.LogError("장착 대상에 WeaponBase 컴포넌트가 없습니다!");
            Destroy(instance);
            return;
        }

        // 4) 캐릭터명과 무기명으로 Pivot 데이터 조회
        string characterName = gameObject.name;  // e.g. "Azusa"
        string weaponName = wb.name;          // e.g. "Azusa_Weapon"

        PivotJsonEntry entry;
        bool found = pivotManager.TryGetEntry(characterName, weaponName, out entry);
        if (!found)
        {
            Debug.LogError($"[{characterName},{weaponName}] Pivot 데이터 미등록");
            return;
        }

        // 5) JSON에서 읽은 pos/rot/scale 적용
        instance.transform.localPosition = entry.pos;
        instance.transform.localEulerAngles = entry.rot;
        instance.transform.localScale = entry.scale;

        // 6) WeaponBase 초기화 및 후속 처리
        wb.Equip();
        currentWeapon = wb;

        // AnimatorController 세팅
        if (wb.name == initialWeaponName)
        {
            RestoreInitialAnimatorController();
        }
        else
        {
            ApplyAOCForWeapon(wb);
        }

        weaponInstance = instance;

        // IK 매핑
        MapWeaponToIK(wb);

        Debug.Log("장착된 무기: " + wb.name);
    }


    private GameObject AttachExisting(GameObject obj, Transform parent)
    {
        obj.transform.SetParent(parent, false);
        if (obj.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
        if (obj.TryGetComponent<Collider>(out var col)) col.enabled = false;
        return obj;
    }

    // 나중에 땅바닥에 드랍하는 형태로 변경 필요
    private void UnEquip()
    {
        if (currentWeapon != null)
        {
            // 1) Hierarchy에서 분리
            Transform weaponTransform = currentWeapon.transform;
            weaponTransform.SetParent(null);

            // 2) Rigidbody 활성화 (물리 시뮬레이션 적용)
            Rigidbody rbComponent;
            if (weaponTransform.TryGetComponent<Rigidbody>(out rbComponent))
            {
                rbComponent.isKinematic = false;
            }
            else
            {
                // Rigidbody가 없다면 새로 추가할 수도 있습니다.
                rbComponent = weaponTransform.gameObject.AddComponent<Rigidbody>();
            }

            // 3) Collider 활성화
            Collider colComponent;
            if (weaponTransform.TryGetComponent<Collider>(out colComponent))
            {
                colComponent.enabled = true;
            }
            else
            {
                // 필요하다면 적절한 Collider 타입을 추가
                weaponTransform.gameObject.AddComponent<BoxCollider>();
            }

            // 4) 약간의 임펄스(선택)
            //rbComponent.AddForce(transform.forward * 2f + Vector3.up * 1f, ForceMode.Impulse);

            // 5) currentWeapon 레퍼런스 해제
            currentWeapon = null;
        }
    }

    private void weaponPickUp_Start(InputAction.CallbackContext context)
    {
        // 감지된(줍기 가능한) 가장 가까운 무기 하나 꺼내기
        WeaponBase toPickup = GetDetectedWeapon();
        if (toPickup != null)
        {
            // 이미 씬에 있는 오브젝트이므로 instantiate=false
            Equip(toPickup.gameObject, false);

            // 감지 리스트에서 제거
            ClearDetectedWeapon(toPickup);

            Debug.Log("무기 픽업: " + toPickup.name);
        }
        else
        {
            Debug.Log("주변에 장착할 수 있는 무기가 없습니다.");
        }
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

        aimIK.solver.transform = weaponInstance.transform.Find("Aim").transform;

        // IK 즉시 갱신
        solver.Initiate(fbbIK.transform);
        solver.Update();
    }

    public WeaponBase GetDetectedWeapon()
    {
        if (detectedWeapons.Count == 0)
            return null;

        WeaponBase nearest = null;
        float bestDistSqr = float.MaxValue;
        Vector3 pivotPos = weaponPivot.position;

        foreach (WeaponBase wb in detectedWeapons)
        {
            if (wb == null) continue;
            float distSqr = (wb.transform.position - pivotPos).sqrMagnitude;
            if (distSqr < bestDistSqr)
            {
                bestDistSqr = distSqr;
                nearest = wb;
            }
        }

        return nearest;
    }

    public void SetDetectedWeapon(WeaponBase wb)
    {
        if (!detectedWeapons.Contains(wb))
        {
            detectedWeapons.Add(wb);
            Debug.Log($"Detected weapon added: {wb.name}");
        }
    }

    public void ClearDetectedWeapon(WeaponBase wb)
    {
        if (detectedWeapons.Remove(wb))
            Debug.Log($"Detected weapon removed: {wb.name}");
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