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

    [Header("Hand Animation Setup")]
    [SerializeField] private Transform HandBone; // 탄창 잡는 손
    [SerializeField] private bool IsLeftHandGrip = false;  // 왼손 그립을 쓸지 여부

    private Transform magazineBone; // 탄창 오브젝트
    private Transform gripBone; // 무기 프리팹 안의 Left/RightHandGrip
    private Transform magazineParent; //원래 magazine 부모

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
    private MovingComponent movingComponent;

    private RuntimeAnimatorController initialAnimatorController;
    private FullBodyBipedEffector magazineEffectorType = FullBodyBipedEffector.LeftHand;

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
        movingComponent = GetComponent<MovingComponent>();
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

        //InputAction NormalSkill = playerInputActionMap.FindAction("NormalSkill");
        //NormalSkill.started += NormalSkill_Start;

        //InputAction EXSkill = playerInputActionMap.FindAction("EXSkill");
        //EXSkill.started += EXSkill_Start;

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
        animator.SetBool("IsAim", true);

        if (movingComponent != null && movingComponent.bCover)
        {
            aimIK.enabled = true;
        }
    }

    private void Aim_Cancel(InputAction.CallbackContext context)
    {
        animator.SetBool("IsAim", false);

        if (movingComponent != null && movingComponent.bCover)
        {
            aimIK.enabled = false;
        }
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

        int reloadIdx = UnityEngine.Random.Range(0, currentWeapon.RandomReload);
        animator.SetInteger("Reload", reloadIdx);
        animator.SetTrigger("ReloadTrigger");

        Debug.Log($"애니메이터에 재장전 트리거 실행 (Reload={reloadIdx})");

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
            instance = weaponObject;
            instance.transform.SetParent(weaponPivot, false);
        }

        Rigidbody rb = instance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false; // 선택: 중력도 꺼야 장착 중 떠 있거나 움직이지 않음
        }

        Collider[] cols = instance.GetComponents<Collider>();
        foreach (var col in cols)
        {
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
        instance.transform.localPosition = entry.weaponPos;
        instance.transform.localEulerAngles = entry.weaponRot;
        instance.transform.localScale = entry.weaponScale;

        ApplyGripPivots(instance.transform, entry);

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

        string gripName = IsLeftHandGrip ? "LeftHandGrip" : "RightHandGrip";

        gripBone = weaponInstance.transform.Find(gripName);
        if (gripBone == null)
            Debug.LogError($"{gripName}을(를) 찾을 수 없습니다!");

        DetectEffector();

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

            Rigidbody rb = weaponTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            Collider[] cols = weaponTransform.GetComponents<Collider>();
            foreach (var col in cols)
            {
                col.enabled = true;
            }

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
        if (fbbIK == null || aimIK == null || weapon == null)
        {
            Debug.LogError("FBBIK, AimIK 또는 weapon이 null입니다.");
            return;
        }

        IKSolverFullBodyBiped solver = fbbIK.solver;

        Transform leftGrip = weapon.transform.Find("LeftHandGrip");
        Transform rightGrip = weapon.transform.Find("RightHandGrip");

        if (leftGrip == null || rightGrip == null)
        {
            Debug.LogError($"{weapon.name}에 LeftHandGrip/RightHandGrip이 없습니다.");
        }
        else
        {
            solver.leftHandEffector.target = leftGrip;
            solver.leftHandEffector.positionWeight = 1f;
            solver.leftHandEffector.rotationWeight = 1f;

            solver.rightHandEffector.target = rightGrip;
            solver.rightHandEffector.positionWeight = 1f;
            solver.rightHandEffector.rotationWeight = 1f;

            // 즉시 반영
            solver.Initiate(fbbIK.transform);
            solver.Update();
            Debug.Log($"[MapWeaponToIK] FBBIK 그립 세팅 → {leftGrip.name}, {rightGrip.name}");
        }

        Transform firePoint = weapon.transform.Find("fire_01");

        if (firePoint == null)
        {
            Debug.LogError($"{weapon.name}에 fire_01 포인트가 없습니다.");
            return;
        }
        else
        {
            aimIK.solver.Initiate(animator.transform);
            aimIK.solver.transform = firePoint;
            aimIK.solver.IKPositionWeight = 1f;
            aimIK.solver.Update();

            Debug.Log($"[MapWeaponToAimIK] Aim Transform → {firePoint.name}");
        }
    }

    private void ApplyGripPivots(Transform weaponRoot, PivotJsonEntry entry)
    {
        // 왼손 그립
        if (entry.leftHandGrip != null)
        {
            Transform left = weaponRoot.Find("LeftHandGrip");
            if (left != null)
                ApplyTransformInfo(left, entry.leftHandGrip);
            else
                Debug.LogWarning("LeftHandGrip 오브젝트가 없습니다.");
        }

        // 오른손 그립
        if (entry.rightHandGrip != null)
        {
            Transform right = weaponRoot.Find("RightHandGrip");
            if (right != null)
                ApplyTransformInfo(right, entry.rightHandGrip);
            else
                Debug.LogWarning("RightHandGrip 오브젝트가 없습니다.");
        }
    }

    private void ApplyTransformInfo(Transform t, WeaponTransformInfo info)
    {
        t.localPosition = info.position;
        t.localEulerAngles = info.rotation;
        t.localScale = info.scale;
    }

    private void DetectEffector()
    {
        if (HandBone == null) return;

        string handName = HandBone.name.ToLower();

        if (handName.Contains("left") || handName.Contains("_l") || handName.Contains("l "))
        {
            magazineEffectorType = FullBodyBipedEffector.LeftHand;
        }
        else if (handName.Contains("right") || handName.Contains("_r") || handName.Contains("r "))
        {
            magazineEffectorType = FullBodyBipedEffector.RightHand;
        }
        else
        {
            Debug.LogWarning($"HandBone 이름으로부터 손을 인식하지 못했습니다: {HandBone.name}, 기본값 RightHand로 처리합니다.");
            magazineEffectorType = FullBodyBipedEffector.RightHand;
        }

        Debug.Log($"[WeaponComponent] HandBone '{HandBone.name}' → IK Effector = {magazineEffectorType}");
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

    private IKEffector GetMagazineEffector()
    {
        return fbbIK?.solver?.GetEffector(magazineEffectorType);
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

    public void OnReloadStart()
    {
        IKEffector effector = GetMagazineEffector();
        if (effector != null)
        {
            effector.positionWeight = 0f;
            effector.rotationWeight = 0f;
        }
    }

    // 탄창 잡는 시점 → IK 타겟 magazineBone, Weight 켜기 + 부모를 손에 붙이기
    public void OnMagazineStart()
    {
        if (magazineBone == null || HandBone == null) return;

        IKEffector effector = GetMagazineEffector();
        effector.target = magazineBone;
        effector.positionWeight = 1f;
        effector.rotationWeight = 1f;

        magazineParent = magazineBone.parent;
        magazineBone.SetParent(HandBone, worldPositionStays: true);
    }

    // 탄창 분리 후 손 떼기(필요 시 IK 끄기)
    public void OnMagazineEnd()
    {
        IKEffector effector = GetMagazineEffector();
        effector.positionWeight = 0f;
        effector.rotationWeight = 0f;
    }

    // 새 탄창 끼우는 시점 → magazineBone을 무기로 재부착
    public void OnInsertMagazine()
    {
        if (magazineBone == null)
            return;

        magazineBone.SetParent(magazineParent, worldPositionStays: true);
    }

    // 재장전 끝 → 손잡이로 IK 타겟 복귀, Weight 켜기
    public void OnReloadEnd()
    {
        IKEffector effector = GetMagazineEffector();
        effector.target = gripBone;
        effector.positionWeight = 1f;
        effector.rotationWeight = 1f;
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