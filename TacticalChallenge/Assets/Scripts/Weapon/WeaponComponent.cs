using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RootMotion.FinalIK;

public class WeaponComponent : MonoBehaviour
{
    [Header("Weapon Setup")]
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private GameObject weaponPrefab;

    private PivotManager pivotManager;
    private WeaponBase currentWeapon;
    private List<WeaponBase> detectedWeapons = new List<WeaponBase>();

    private PlayerInput playerInput;
    private InputActionMap playerInputActionMap;

    private Animator animator;
    private FullBodyBipedIK fbbIK;
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

        pivotManager = FindObjectOfType<PivotManager>();
        if (pivotManager == null)
            Debug.LogError("���� PivotManager�� �����ϴ�!");

        if (fbbIK == null)
            Debug.LogError("FullBodyBipedIK ������Ʈ�� ã�� �� �����ϴ�.");

        if (weaponPivot == null)
            Debug.LogError("WeaponPivot(Transform)�� �ν����Ϳ��� �Ҵ��ϼ���!");

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
            Debug.Log($"�ʱ� ���� ���� : {initialWeaponName}");
        }

    }

    private void Attack_Start(InputAction.CallbackContext context)
    {
        if (isCancelAble) // CancelAble ���¶�� �ִϸ��̼��� �ʱ�ȭ�ϰ� �ٽ� ���
        {
            Debug.Log("CancelAble ����: �ִϸ��̼� �ʱ�ȭ �� ���");
            animator.Play("Attack", 0, 0); // "Attack" Ŭ���� 0�����Ӻ��� �ٽ� ���
            isCancelAble = false; // �ʱ�ȭ �� CancelAble ���¸� false�� ����
            return;
        }

        string currentAnimationName = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        if (!currentAnimationName.Contains("_Attack_Ing"))
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.normalizedTime < 1.0f)
            {
                Debug.Log("�ִϸ��̼� ���� ��: �ߺ� ���� ����");
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

    private void Reload_Start(InputAction.CallbackContext context) // ���߿� State���� �־��ֱ�
    {
        if (currentWeapon == null)
        {
            Debug.LogWarning("���� ������ ���Ⱑ �����ϴ�. �������� �� �����ϴ�.");
            return;
        }

        if (currentWeapon.ammo == currentWeapon.megazine)
        {
            Debug.Log("ź���� ���� �� �־� �������� �ʿ����� �ʽ��ϴ�.");
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Animator ������Ʈ�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        currentWeapon.Reload();
    }

    public void Equip(GameObject weaponObject, bool instantiate = true)
    {
        // 1) ���� ���� ����
        UnEquip();

        // 2) Prefab �Ǵ� �� ������Ʈ�� instance�� �Ҵ�
        GameObject instance;
        if (instantiate)
        {
            // �������� �� ���� �ν��Ͻ�ȭ
            instance = Instantiate(weaponObject, weaponPivot, false);
            instance.name = weaponObject.name;
        }
        else
        {
            // �̹� ���� �����ϴ� ������Ʈ
            instance = weaponObject;
            instance.transform.SetParent(weaponPivot, false);

            Rigidbody rb = instance.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;

            Collider col = instance.GetComponent<Collider>();
            if (col != null)
                col.enabled = false;
        }

        // 3) WeaponBase ������Ʈ ��������
        WeaponBase wb = instance.GetComponent<WeaponBase>();
        if (wb == null)
        {
            Debug.LogError("���� ��� WeaponBase ������Ʈ�� �����ϴ�!");
            Destroy(instance);
            return;
        }

        // 4) ĳ���͸�� ��������� Pivot ������ ��ȸ
        string characterName = gameObject.name;  // e.g. "Azusa"
        string weaponName = wb.name;          // e.g. "Azusa_Weapon"

        PivotJsonEntry entry;
        bool found = pivotManager.TryGetEntry(characterName, weaponName, out entry);
        if (!found)
        {
            Debug.LogError($"[{characterName},{weaponName}] Pivot ������ �̵��");
            return;
        }

        // 5) JSON���� ���� pos/rot/scale ����
        instance.transform.localPosition = entry.pos;
        instance.transform.localEulerAngles = entry.rot;
        instance.transform.localScale = entry.scale;

        // 6) WeaponBase �ʱ�ȭ �� �ļ� ó��
        wb.Equip();
        currentWeapon = wb;

        // AnimatorController ����
        if (wb.name == initialWeaponName)
        {
            RestoreInitialAnimatorController();
        }
        else
        {
            ApplyAOCForWeapon(wb);
        }

        // IK ����
        MapWeaponToIK(wb);

        Debug.Log("������ ����: " + wb.name);
    }


    private GameObject AttachExisting(GameObject obj, Transform parent)
    {
        obj.transform.SetParent(parent, false);
        if (obj.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
        if (obj.TryGetComponent<Collider>(out var col)) col.enabled = false;
        return obj;
    }

    // ���߿� ���ٴڿ� ����ϴ� ���·� ���� �ʿ�
    private void UnEquip()
    {
        if (currentWeapon != null)
        {
            // 1) Hierarchy���� �и�
            Transform weaponTransform = currentWeapon.transform;
            weaponTransform.SetParent(null);

            // 2) Rigidbody Ȱ��ȭ (���� �ùķ��̼� ����)
            Rigidbody rbComponent;
            if (weaponTransform.TryGetComponent<Rigidbody>(out rbComponent))
            {
                rbComponent.isKinematic = false;
            }
            else
            {
                // Rigidbody�� ���ٸ� ���� �߰��� ���� �ֽ��ϴ�.
                rbComponent = weaponTransform.gameObject.AddComponent<Rigidbody>();
            }

            // 3) Collider Ȱ��ȭ
            Collider colComponent;
            if (weaponTransform.TryGetComponent<Collider>(out colComponent))
            {
                colComponent.enabled = true;
            }
            else
            {
                // �ʿ��ϴٸ� ������ Collider Ÿ���� �߰�
                weaponTransform.gameObject.AddComponent<BoxCollider>();
            }

            // 4) �ణ�� ���޽�(����)
            //rbComponent.AddForce(transform.forward * 2f + Vector3.up * 1f, ForceMode.Impulse);

            // 5) currentWeapon ���۷��� ����
            currentWeapon = null;
        }
    }

    private void weaponPickUp_Start(InputAction.CallbackContext context)
    {
        // ������(�ݱ� ������) ���� ����� ���� �ϳ� ������
        WeaponBase toPickup = GetDetectedWeapon();
        if (toPickup != null)
        {
            // �̹� ���� �ִ� ������Ʈ�̹Ƿ� instantiate=false
            Equip(toPickup.gameObject, false);

            // ���� ����Ʈ���� ����
            ClearDetectedWeapon(toPickup);

            Debug.Log("���� �Ⱦ�: " + toPickup.name);
        }
        else
        {
            Debug.Log("�ֺ��� ������ �� �ִ� ���Ⱑ �����ϴ�.");
        }
    }

    private void ApplyAOCForWeapon(WeaponBase weapon)
    {
        if (weapon == null || animator == null) return;

        // ���� ĳ���� �̸��� ���� �̸� ��������
        string characterName = gameObject.name; // ĳ���� �̸�

        // ���� �̸����� "_Weapon" �κ� ����
        string weaponName = weapon.name.Replace("_Weapon", "");

        // AOC �̸� ����: {ĳ�����̸�}_Swap_{�����̸�}
        string aocName = $"{characterName}_Swap_{weaponName}";

        // ���ҽ����� AOC�� �ε�
        AnimatorOverrideController aoc = Resources.Load<AnimatorOverrideController>(aocName);

        if (aoc != null)
        {
            animator.runtimeAnimatorController = aoc;

            // **��� ���� �ʱ�ȭ**
            animator.Rebind();
            animator.Update(0);

            Debug.Log($"AOC ����: {aocName}");
        }
    }

    private void RestoreInitialAnimatorController()
    {
        if (initialAnimatorController != null)
        {
            animator.runtimeAnimatorController = initialAnimatorController;

            // **��� ���� �ʱ�ȭ**
            animator.Rebind();
            animator.Update(0);

            Debug.Log($"�ʱ� AnimatorController�� ����: {initialAnimatorController.name}");
        }
        else
        {
            Debug.LogWarning("�ʱ� AnimatorController�� ������� �ʾҽ��ϴ�.");
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
            Debug.LogError($"{weapon.name}�� Left/RightHandGrip�� �����ϴ�.");
            return;
        }

        solver.leftHandEffector.target = leftGrip;
        solver.rightHandEffector.target = rightGrip;
        solver.leftHandEffector.positionWeight = 1f;
        solver.leftHandEffector.rotationWeight = 1f;
        solver.rightHandEffector.positionWeight = 1f;
        solver.rightHandEffector.rotationWeight = 1f;

        // IK ��� ����
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
    /// �ִϸ��̼� �޼���
    ///

    public void Attack()
    {
        // currentWeapon�� ��ȿ�ϸ� CheckAmmo() ȣ��
        if (currentWeapon != null)
        {
            currentWeapon.AmmoLeft();
        }
        else
        {
            Debug.LogWarning("���� ������ ���Ⱑ �����ϴ�.");
        }
    }

    public void CancelAble()
    {
        isCancelAble = true;
        Debug.Log("CancelAble ����");
    }

    public void Impulse()
    {

    }

    /// <summary>
    /// ���� ������ �÷��̾� stat�� �����ϴ� �κ�
    /// </summary>
    /// <param name="weapon"></param>
    public void ApplyStats(WeaponBase weapon)
    {

        statComponent.HasStat(weapon.ammo);
    }
}