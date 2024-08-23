using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(MovingComponent))]
public class Player : Character
{
    protected override void Awake()
    {
        base.Awake();

        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        //TODO: 스킬 컴포넌트를 만들어서 연결해야함
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
}