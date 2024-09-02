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

    PlayerInput input;
    InputActionMap actionMap;
    InputAction Skill;
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

    private IEnumerator EndSkill()
    {
        yield return new WaitForSeconds(NormalSkillCoolDown);

        if (weapon != null)
        {
            weapon.SetActive(true);
        }

        state.SetIdleMode();
    }
}
