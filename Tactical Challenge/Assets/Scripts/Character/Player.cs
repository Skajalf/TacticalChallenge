using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character, IDamagable
{
    [SerializeField] private string codeName;

    public string CodeName { get { return codeName; } }

    PlayerInput playerInput;
    InputActionMap inputActions;
    private StatComponent statComponent;

    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        if (!(playerInput = GetComponent<PlayerInput>()))
            playerInput = this.AddComponent<PlayerInput>();
        statComponent = GetComponent<StatComponent>();
        inputActions = playerInput.actions.FindActionMap("Player");
    }

    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, WeaponData data)
    {
        statComponent.Damage(data.Power);
    }
}
