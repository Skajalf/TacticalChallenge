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
    [SerializeField] private bool[] weaponActiveOnStart; // ���� Ȱ��ȭ ���� �迭

    private List<WeaponBase> weapons = new List<WeaponBase>();
    private int currentWeaponIndex = 0; // ���� ������ ���� �ε���
    private WeaponBase currentWeapon; // ������ ����
    private WeaponBase detectedWeapon; // �ֺ����� Ž���� ����

    private PlayerInput playerInput;
    private InputActionMap playerInputActionMap;
    
    private bool isCancelAble = false;

    private Animator animator;

    private FullBodyBipedIK fbbIK;

    // ���� AnimatorController ���� ����
    private RuntimeAnimatorController initialAnimatorController;
    private string initialWeaponName; // ���� ���� �̸� ����

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
        if (fbbIK == null) Debug.LogError("FullBodyBipedIK ������Ʈ�� ã�� �� �����ϴ�.");

        if (weaponPivot == null) Debug.LogError("WeaponPivot(Transform)�� �ν����Ϳ��� �Ҵ��ϼ���!");

        // ** ���� AnimatorController ���� **
        initialAnimatorController = animator.runtimeAnimatorController;


        Equip();

        foreach (var weapon in weapons)
        {
            weapon.ammo = weapon.megazine;
        }

        // �ڷ�ƾ ����
        StartCoroutine(CheckForNearbyWeapons());
    }

    private IEnumerator CheckForNearbyWeapons()
    {
        while (true)
        {
            DetectWeaponInRange();
            yield return new WaitForSeconds(1.0f); // 0.5�ʸ��� �ֱ������� ����
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 1.25f);
    }

    private void DetectWeaponInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1.25f);
        
        detectedWeapon = null; // �ʱ�ȭ

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Weapon"))
            {
                detectedWeapon = hitCollider.GetComponent<WeaponBase>();
                if (detectedWeapon != null)
                {
                    break; // ������ ���⸦ ã������ ���� ����
                }
            }
        }

        if (detectedWeapon == null)
        {
            Debug.Log("�ֺ��� ������ �� �ִ� ���Ⱑ �����ϴ�.");
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

    private void weaponSwap_Start(InputAction.CallbackContext context)
    {
        // ���� Ű�� ���� Ű(1~9 ��)���� Ȯ��
        int weaponIndex;
        if (int.TryParse(context.control.name, out weaponIndex))
        {
            // ���� �ε����� ����ϱ� ���� 1�� ���� (1�� Ű -> 0�� ����, 2�� Ű -> 1�� ����, ...)
            weaponIndex -= 1;

            // ��ȿ�� ���� �ε������� Ȯ��
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


    private void Equip()
    {
        if (weaponPrefabs == null || weaponPrefabs.Length == 0) return;
        if (weaponActiveOnStart.Length != weaponPrefabs.Length)
        {
            Debug.LogError("weaponActiveOnStart ũ�� ����ġ");
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
            Debug.Log($"�ʱ� ���� ����: {currentWeapon.name}");
        }
    }

    private void SwapWeapon(int newIndex)
    {
        if (newIndex == currentWeaponIndex) return;

        weapons[currentWeaponIndex].gameObject.SetActive(false);
        currentWeaponIndex = newIndex;
        currentWeapon = weapons[newIndex];
        currentWeapon.gameObject.SetActive(true);

        // �ִϸ����� ��Ʈ�ѷ� ���� �Ǵ� AOC ����
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
        currentWeaponIndex = weapons.IndexOf(newWeapon); // �� ������ ���� �ε����� ����

        // ���� Ȱ��ȭ
        newWeapon.gameObject.SetActive(true);

        // ** �ʱ� ������ �ʱ� AnimatorController ���� **
        if (newWeapon.name == initialWeaponName)
        {
            RestoreInitialAnimatorController();
        }
        else
        {
            ApplyAOCForWeapon(currentWeapon);
        }

        Debug.Log($"{newWeapon.name} ���⸦ �����߽��ϴ�.");
    }

    public WeaponBase GetActiveWeapon() //�Ƹ� ���� ������ ������ ȣ���ҵ�?
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