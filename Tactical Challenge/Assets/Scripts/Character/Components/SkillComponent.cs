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

        InputAction Skill = actionMap.FindAction("Skill");
        Skill.started += startSkill;
    }

    private void startSkill(InputAction.CallbackContext context)
    {
        IsEXSkill = true;

        if (weapon != null)
        {
            weapon.SetActive(false);
        }

        animator.SetTrigger("Skill");
        StartCoroutine(EndSkill());
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
