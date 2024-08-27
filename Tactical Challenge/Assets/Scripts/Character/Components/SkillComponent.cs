using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillComponent : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private GameObject weapon; // Bip001_Weapon을 참조할 변수

    public bool IsEXSkill { private set; get; }
    public bool IsNormalSkill { private set; get; }
    public bool IsMeleeAttack { private set; get; }

    public void Awake()
    {
        animator = GetComponent<Animator>();

        weapon = transform.FindChildByName("Bip001_Weapon")?.gameObject;

        if (weapon == null)
        {
            Debug.LogError("Weapon not found! Make sure the weapon is named 'Bip001_Weapon'.");
        }

        PlayerInput input = GetComponent<PlayerInput>();

        InputActionMap actionMap = input.actions.FindActionMap("Player");

        InputAction EXSkill = actionMap.FindAction("EXSkill");
        EXSkill.started += startEXSkill;

        InputAction MeleeAttack = actionMap.FindAction("MeleeAttack");
        MeleeAttack.started += startMeleeAttack;
    }

    private void startEXSkill(InputAction.CallbackContext context)
    {
        IsEXSkill = true;

        if (weapon != null)
        {
            weapon.SetActive(false);
        }

        animator.SetTrigger("EXSkill");
        StartCoroutine(EndSkill());
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

    private IEnumerator EndSkill()
    {
        yield return new WaitForSeconds(4.0f);

        if (weapon != null)
        {
            weapon.SetActive(true);
        }

        IsEXSkill = false;
    }
}
