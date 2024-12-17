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
    [SerializeField] private bool[] weaponActiveOnStart; // ���� Ȱ��ȭ ���� �迭

    private List<WeaponBase> weapons = new List<WeaponBase>();
    private int currentWeaponIndex = 0; // ���� ������ ���� �ε���
    private WeaponBase currentWeapon; // ������ ����
    private WeaponBase detectedWeapon; // �ֺ����� Ž���� ����

    private PlayerInput playerInput;
    private InputActionMap playerInputActionMap;
    
    private bool isCancelAble = false;

    private Animator animator;
    private HandIKComponent handIK;

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
        handIK = GetComponent<HandIKComponent>();

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
        if (detectedWeapon != null)
        {
            EquipWeapon(detectedWeapon);
            detectedWeapon = null; // ���⸦ �����ϸ� Ž���� ���� �ʱ�ȭ
        }
        else
        {
            Debug.Log("�ֺ��� ������ �� �ִ� ���Ⱑ �����ϴ�.");
        }
    }

    private void Aim_Start(InputAction.CallbackContext context)
    {
        handIK.UpdateAimRigWeight(true); // ���̹� ���� �� Weight�� 1�� ����
    }

    private void Aim_Cancel(InputAction.CallbackContext context)
    {
        handIK.UpdateAimRigWeight(false); // ���̹� ��� �� Weight�� 0���� ����
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
        if (weaponPrefabs != null && weaponPrefabs.Length > 0)
        {
            // weaponActiveOnStart �迭�� weaponPrefabs�� ���̰� ���� ������ ���� ���
            if (weaponActiveOnStart.Length != weaponPrefabs.Length)
            {
                Debug.LogError("weaponActiveOnStart �迭�� ũ��� weaponPrefabs �迭�� ũ��� ��ġ�ؾ� �մϴ�.");
                return;
            }

            for (int i = 0; i < weaponPrefabs.Length; i++)
            {
                GameObject weaponInstance = Instantiate(weaponPrefabs[i]);
                weaponInstance.name = weaponPrefabs[i].name;
                WeaponBase weapon = weaponInstance.GetComponent<WeaponBase>();

                if (weapon != null)
                {
                    // FindChildByName�� ����Ͽ� WeaponPivot�� ã��
                    Transform weaponPivot = transform.FindChildByName("WeaponPivot");
                    if (weaponPivot != null)
                    {
                        // ���⸦ WeaponPivot�� �ڽ����� ����
                        weaponInstance.transform.SetParent(weaponPivot, false);
                    }
                    else
                    {
                        Debug.LogError("WeaponPivot�� ã�� �� �����ϴ�.");
                    }

                    weapon.Equip(); // Call the Equip() method on the weapon
                    weapons.Add(weapon); // Add the weapon to the list of equipped weapons

                    // ���⸦ Ȱ��ȭ���� ��Ȱ��ȭ���� ���� (weaponActiveOnStart �迭�� ���)
                    weapon.gameObject.SetActive(weaponActiveOnStart[i]);

                    // ���� ���� �̸� ����
                    if (i == currentWeaponIndex)
                    {
                        initialWeaponName = weapon.name; // �ʱ� ���� �̸� ����
                    }
                }
                else
                {
                    Debug.LogError($"{weaponPrefabs[i].name}�� WeaponBase�� �Ҵ���� �ʾҽ��ϴ�.");
                }
            }

            // weapons ����Ʈ�� ������� �߰��� �� currentWeapon ����
            if (weapons.Count > 0)
            {
                currentWeapon = weapons[currentWeaponIndex];
                Debug.Log($"�ʱ� ���� ����: {currentWeapon.name}");

                // �񵿱������� IK ����
                StartCoroutine(InitializeIKSettings()); // ù ��° ���� ���� �� IK ������Ʈ
            }
            else
            {
                Debug.LogWarning("weapons ����Ʈ�� ���Ⱑ �����ϴ�.");
            }

            // ������� ��� ������ �� IKSettingsUpdate() ȣ��
            //IKSettingsUpdate();
        }
        else
        {
            Debug.LogError("���� �������� �������� �ʾҽ��ϴ�.");
        }
    }

    private void SwapWeapon(int newWeaponIndex)
    {
        if (newWeaponIndex >= 0 && newWeaponIndex < weapons.Count && newWeaponIndex != currentWeaponIndex)
        {
            // ���� ���� ��Ȱ��ȭ
            weapons[currentWeaponIndex].gameObject.SetActive(false);

            // ���ο� ���� Ȱ��ȭ
            currentWeaponIndex = newWeaponIndex;
            currentWeapon = weapons[currentWeaponIndex];
            currentWeapon.gameObject.SetActive(true);

            // ** �ʱ� ������ �ʱ� AnimatorController ���� **
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

        IKSettingsUpdate();
    }

    private IEnumerator InitializeIKSettings()
    {
        yield return new WaitForEndOfFrame(); // �� ������ ����Ͽ� ���� �ʱ�ȭ �Ϸ� ���
        IKSettingsUpdate(); // ���Ⱑ ����� ������ �� IK ���� ����
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
            Debug.LogWarning("HandIKComponent�� ���ų� �ʱ�ȭ���� �ʾҽ��ϴ�.");
        }
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