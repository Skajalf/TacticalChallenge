using System.Collections;
using System.Security.Claims;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActionComponent : MonoBehaviour
{
    //TODO: Attack, Aim, Reload ����� �����ؾ���

    private Animator animator;
    [SerializeField] private GameObject weapon; // Bip001_Weapon�� ������ ����

    public bool IsAim { private set; get; }
    public bool IsReload { private set; get; }
    public bool IsAttack { private set; get; }
    public bool IsMeleeAttack { private set; get; }


    public void Awake()
    {
        animator = GetComponent<Animator>();

        PlayerInput input = GetComponent<PlayerInput>();

        InputActionMap actionMap = input.actions.FindActionMap("Player");

        InputAction attack = actionMap.FindAction("Attack");
        attack.started += startAttack;
        attack.canceled += cancelAttack;

        InputAction aim = actionMap.FindAction("Aim");
        aim.started += startAim;
        aim.canceled += cancelAim;

        InputAction Reload = actionMap.FindAction("Reload");
        Reload.started += startReload;

        InputAction MeleeAttack = actionMap.FindAction("MeleeAttack");
        MeleeAttack.started += startMeleeAttack;
    }

    private void startAttack(InputAction.CallbackContext context)
    {
        IsAttack = true;
        animator.SetBool("IsAttack", true);
    }

    private void cancelAttack(InputAction.CallbackContext context)
    {
        IsAttack = false;
        animator.SetBool("IsAttack", false);
    }

    private void startAim(InputAction.CallbackContext context)
    {
        IsAim = true;
        animator.SetBool("IsAim", true);
    }

    private void cancelAim(InputAction.CallbackContext context)
    {
        IsAim = false;
        animator.SetBool("IsAim", false);
    }

    private void startMeleeAttack(InputAction.CallbackContext context)
    {
        IsMeleeAttack = true;

        if (weapon != null)
        {
            weapon.SetActive(false);
        }

        animator.SetTrigger("MeleeAttack");

        StartCoroutine(EndMeleeAttack());
    }

    private IEnumerator EndMeleeAttack()
    {
        yield return new WaitForSeconds(1.0f);

        if (weapon != null)
        {
            weapon.SetActive(true);
        }

        IsMeleeAttack = false;
    }

    private void startReload(InputAction.CallbackContext context)
    {
        //TODO: źâ�� ��á���� Ȯ���ϰ� ������ return, �����̻� �������� Ȯ���ϰ� ������ return
        IsReload = true;
        animator.SetTrigger("Reload");
        IsReload = false;
    }
}