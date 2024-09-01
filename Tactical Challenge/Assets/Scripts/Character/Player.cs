using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character 
{
    [SerializeField] private string codeName;

    public string CodeName { get { return codeName; } }

    PlayerInput playerInput;
    InputActionMap inputActions;

    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        if (!(playerInput = GetComponent<PlayerInput>()))
            playerInput = this.AddComponent<PlayerInput>();
        inputActions = playerInput.actions.FindActionMap("Player");
    }
}
