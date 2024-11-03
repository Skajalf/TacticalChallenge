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
    private WeaponBase detectedWeapon; // ������ ����

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
    }

    private void Update()
    {
        // �� �����Ӹ��� �ֺ��� ���⸦ ����
        DetectWeaponInRange();
    }

    private void DetectWeaponInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1.0f);
        detectedWeapon = null; // �ʱ�ȭ

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Weapon")) // "Weapon" ���̾� Ȯ��
            {
                detectedWeapon = hitCollider.GetComponent<WeaponBase>();
                if (detectedWeapon != null)
                {
                    break; // ������ ���⸦ ã������ ���� ����
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
        // ������ ���Ⱑ ���� ��� ��ü
        if (detectedWeapon != null)
        {
            EquipWeapon(detectedWeapon);
        }
        else
        {
            Debug.Log("�ֺ��� ������ �� �ִ� ���Ⱑ �����ϴ�.");
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
        if (/*bIsEquip && playerState.CanDoSomething() &&*/ (detectedWeapon.ammo != detectedWeapon.megazine))
        {
            detectedWeapon.Reload();
            animator.SetTrigger("Reload");
        }
        else
            return;
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

                    // ù ��° ���� ���� �� IK Ÿ�� ������Ʈ
                    if (i == 0)
                    {
                        // HandIKComponent�� IK Ÿ�� ���� �� ���� ����
                        HandIKComponent handIK = GetComponent<HandIKComponent>();
                        handIK.UpdateWeaponIKTargets(); // �ʱ� ���� ���� �� IK Ÿ�� ������Ʈ
                        handIK.BuildRig(); // ���� ���� ȣ��
                    }
                }
                else
                {
                    Debug.LogError($"{weaponPrefabs[i].name}�� WeaponBase�� �Ҵ���� �ʾҽ��ϴ�.");
                }
            }
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
            weapons[currentWeaponIndex].gameObject.SetActive(true);

            // HandIKComponent�� IK Ÿ�� ���� �� ���� ����
            HandIKComponent handIK = GetComponent<HandIKComponent>();
            handIK.UpdateWeaponIKTargets();
            handIK.BuildRig(); // ���� ���� ȣ��
        }
    }

    private void EquipWeapon(WeaponBase newWeapon)
    {
        // ���� ������ ���⸦ ��Ȱ��ȭ
        weapons[currentWeaponIndex].UnEquip(); // ���� ���� UnEquip

        // ���ο� ���⸦ ����Ʈ�� �߰��ϰ� Ȱ��ȭ
        weapons[currentWeaponIndex] = newWeapon; // ���ο� ���⸦ ���� ���Կ� �߰�
        newWeapon.transform.SetParent(transform); // ���ο� ������ �θ� �÷��̾�� ����
        newWeapon.Equip(); // ���ο� ���⸦ ����

        // ���ο� ���� Ȱ��ȭ
        newWeapon.gameObject.SetActive(true);
        Debug.Log($"{newWeapon.name} ���⸦ �����߽��ϴ�.");

        // HandIKComponent�� IK Ÿ�� ���� �� ���� ����
        HandIKComponent handIK = GetComponent<HandIKComponent>();
        handIK.UpdateWeaponIKTargets();
        handIK.BuildRig(); // ���� ���� ȣ��
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
    /// �ִϸ��̼� �޼���
    ///

    public void Attack()
    {
        //if (���߿� ���� �����ߴ��� Ȯ���ϴ� �޼��� ����ֱ�)
        //{
        detectedWeapon.CheckAmmo();
        //}
        return;
    }

    public void Impulse()
    {
        //if (bIsEquip)
        //{
            detectedWeapon.makeImpulse();
        //}
        return;
    }
}