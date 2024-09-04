using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(WeaponComponent))]

public class Player : Character
{
    [SerializeField] private string codeName;

    public string CodeName { get { return codeName; } }

    private PlayerInput playerInput;
    private InputActionMap inputActions;
    private StatComponent statComponent;

    private Projectile bullet;


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

    public override void OnDamage(float damage)
    {
        statComponent.Damage(damage);
    }

    
    private void OnTriggerEnter(Collider other)
    {
        
        
    }
}
