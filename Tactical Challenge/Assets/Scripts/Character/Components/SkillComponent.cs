using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillComponent : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private GameObject weapon; // Bip001_Weapon              
    [SerializeField] private float ExSkillCoolDown;
    [SerializeField] private float NormalSkillCoolDown;

    public bool IsEXSkill { private set; get; }
    public bool IsNormalSkill { private set; get; }
    public bool IsMeleeAttack { private set; get; }

    PlayerInput input;
    InputActionMap actionMap;
    InputAction Skill;
    InputAction EXSkill;
    InputAction MeleeAttack;
    StateComponent state;

    public void Awake()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();

        weapon = transform.FindChildByName("Bip001_Weapon")?.gameObject;

        if (weapon == null)
        {
            Debug.LogError("Weapon not found! Make sure the weapon is named 'Bip001_Weapon'.");
        }

        if (!(input = GetComponent<PlayerInput>()))
            this.AddComponent<PlayerInput>();

        Init();
    }

    private void Init() // SkillComponent Init();
    {
        actionMap = input.actions.FindActionMap("Player");

        Skill = actionMap.FindAction("Skill");
        Skill.started += startSkill;

        EXSkill = actionMap.FindAction("EXSkill");
        EXSkill.started += startEXSkill;

        MeleeAttack = actionMap.FindAction("MeleeAttack");
        MeleeAttack.started += startMeleeAttack;
    }

    private void startSkill(InputAction.CallbackContext context)
    {
        if (state.CurrentState == StateType.Skill || state.CurrentState == StateType.Reload)
        {
            return;
        }

        state.SetSkillMode();

        if (weapon != null)
        {
            weapon.SetActive(false);
        }

        animator.SetTrigger("Skill");
        StartCoroutine(EndSkill());
    }

    private void startEXSkill(InputAction.CallbackContext context)
    {
        if (state.CurrentState == StateType.Skill || state.CurrentState == StateType.Reload)
        {
            return;
        }

        state.SetSkillMode();

        if (weapon != null)
        {
            weapon.SetActive(false);
        }

        animator.SetTrigger("EXSkill");
        StartCoroutine(EndEXSkill());
    }

    private IEnumerator EndSkill()
    {
        yield return new WaitForSeconds(NormalSkillCoolDown);

        if (weapon != null)
        {
            weapon.SetActive(true);
        }

        state.SetIdleMode();
    }

    private IEnumerator EndEXSkill()
    {
        yield return new WaitForSeconds(ExSkillCoolDown);

        if (weapon != null)
        {
            weapon.SetActive(true);
        }

        state.SetIdleMode();
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
}
