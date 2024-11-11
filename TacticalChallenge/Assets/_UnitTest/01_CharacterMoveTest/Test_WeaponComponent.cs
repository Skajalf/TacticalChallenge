using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.TextCore.Text;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class Test_WeaponComponent : MonoBehaviour
{
    [SerializeField] private GameObject[] weaponPrefabs;
    [SerializeField] private bool[] weaponActiveOnStart; // ���� Ȱ��ȭ ���� �迭

    private List<Test_WeaponBase> weapons = new List<Test_WeaponBase>();
    private int currentWeaponIndex = 0; // ���� ������ ���� �ε���
    private Test_WeaponBase currentWeapon; // ������ ����
    private Test_WeaponBase detectedWeapon; // �ֺ����� Ž���� ����

    private PlayerInput playerInput;
    private InputActionMap playerInputActionMap;

    private Animator animator;
    private Test_HandIKComponent handIK;

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
        handIK = GetComponent<Test_HandIKComponent>();

        Equip();

        foreach (var weapon in weapons)
        {
            weapon.ammo = weapon.magazine;
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

    private void DetectWeaponInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1.0f);
        detectedWeapon = null; // �ʱ�ȭ

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Weapon"))
            {
                detectedWeapon = hitCollider.GetComponent<Test_WeaponBase>();
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
        // ���� �ִϸ��̼� �̸��� ���ڿ��� ��ȯ
        string currentAnimationName = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        // �ִϸ��̼� �̸��� '_Attack_Ing'�� ���ԵǾ� ������, �ִϸ��̼��� ������� ����
        if (!currentAnimationName.Contains("_Attack_Ing"))
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // �ִϸ��̼��� ������ �ʾҴٸ� (normalizedTime < 1.0f)
            if (stateInfo.normalizedTime < 1.0f)
            {
                Debug.Log("�ִϸ��̼� ���� ��: �ߺ� ���� ����");
                return; // �ִϸ��̼��� ���� ������ ��ٸ�
            }
        }

        // �ִϸ��̼��� ������ �� ������, ���� �ִϸ��̼� ����
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

        if (currentWeapon.ammo == currentWeapon.magazine)
        {
            Debug.Log("ź���� ���� �� �־� �������� �ʿ����� �ʽ��ϴ�.");
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Animator ������Ʈ�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        currentWeapon.Test_Reload();
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
                Test_WeaponBase weapon = weaponInstance.GetComponent<Test_WeaponBase>();

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

                    weapon.Test_Equip(); // Call the Equip() method on the weapon
                    weapons.Add(weapon); // Add the weapon to the list of equipped weapons

                    // ���⸦ Ȱ��ȭ���� ��Ȱ��ȭ���� ���� (weaponActiveOnStart �迭�� ���)
                    weapon.gameObject.SetActive(weaponActiveOnStart[i]);
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

            IKSettingsUpdate();
        }
    }

    private void EquipWeapon(Test_WeaponBase newWeapon)
    {
        int emptySlotIndex = weapons.FindIndex(w => w == null);

        if (emptySlotIndex != -1)
        {
            weapons[emptySlotIndex] = newWeapon;
        }
        else
        {
            weapons[currentWeaponIndex].Test_UnEquip();
            weapons[currentWeaponIndex] = newWeapon;
        }

        newWeapon.transform.SetParent(transform);
        newWeapon.Test_Equip();

        currentWeapon = newWeapon;
        newWeapon.gameObject.SetActive(true);
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
            Debug.LogWarning("Test_HandIKComponent�� ���ų� �ʱ�ȭ���� �ʾҽ��ϴ�.");
        }
    }

    public Test_WeaponBase GetActiveWeapon() //�Ƹ� ���� ������ ������ ȣ���ҵ�?
    {
        if (currentWeaponIndex >= 0 && currentWeaponIndex < weapons.Count)
        {
            return weapons[currentWeaponIndex];
        }
        return null;
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

    public void Impulse()
    {
        
    }
}