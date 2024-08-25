using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillComponent : MonoBehaviour
{
    private Animator animator;

    public bool IsEXSkill { private set; get; }
    public bool IsNormalSkill { private set; get; }

    public void Awake()
    {
        animator = GetComponent<Animator>();

        PlayerInput input = GetComponent<PlayerInput>();

        InputActionMap actionMap = input.actions.FindActionMap("Player");

        InputAction EXSkill = actionMap.FindAction("EXSkill");
        EXSkill.started += startEXSkill;
    }

    private void startEXSkill(InputAction.CallbackContext context)
    {
        IsEXSkill = true;
        animator.SetTrigger("EXSkill");
        IsEXSkill = false;
    }
}
