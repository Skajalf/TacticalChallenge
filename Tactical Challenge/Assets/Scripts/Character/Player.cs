using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.StandaloneInputModule;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(MovingComponent))]
public class Player : Character
{
    protected override void Awake()
    {
        base.Awake();

        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        InputAction attack = actionMap.FindAction("Attack");
        attack.started += startAttack;
        attack.canceled += cancelAttack;

        //TODO: ActionComponent를 따로 만들고 거기다가 공격기능 집어넣기
        //actionMap.FindAction("Attack").started += context =>
        //{
        //    Action.SetAttackMode();
        //};

        //actionMap.FindAction("Skill").started += context =>
        //{
        //    Action.SetSkillMode();
        //};

        //actionMap.FindAction("EXS").started += context =>
        //{
        //    Action.SetEXSMode();
        //};

        //actionMap.FindAction("Reload").started += context =>
        //{
        //    Action.SetReloadMode();
        //};
    }

    private void startAttack(InputAction.CallbackContext context)
    {

    }

    private void cancelAttack(InputAction.CallbackContext context)
    {

    }
}