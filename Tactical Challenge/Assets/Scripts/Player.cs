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

        //TODO: ��ų ������Ʈ�� ���� �����ؾ���
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