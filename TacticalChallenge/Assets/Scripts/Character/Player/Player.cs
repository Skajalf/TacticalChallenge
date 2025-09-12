using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character
{
    public event Action<float, float> OnHPChanged;
    public event Action<float, float> OnAPChanged;

    public float HP => stat != null ? stat.CurrentHP : 0f;
    public float MaxHP => stat != null ? stat.MaxHP : 1f;
    public float AP => stat != null ? stat.CurrentAP : 0f;
    public float MaxAP => stat != null ? stat.MaxAP : 1f;

    // 테스트용
    [SerializeField] private float testDamageRadius = 5f; // 공격 반경
    [SerializeField] private float testDamageAmount = 20f; // 데미지 양

    private void Stat_OnHPChanged(float cur, float max) => OnHPChanged?.Invoke(cur, max);
    private void Stat_OnAPChanged(float cur, float max) => OnAPChanged?.Invoke(cur, max);

    protected override void Awake()
    {
        base.Awake();

        Init();
    }

    private void Init()
    {
        if (stat != null)
        {
            stat.OnHPChanged += Stat_OnHPChanged;
            stat.OnAPChanged += Stat_OnAPChanged;
        }

        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        InputAction test = actionMap.FindAction("Test");
        test.started += Test;
    }

    private void Test(InputAction.CallbackContext context)
    {
        //bool died = GetDamage(100f, this.gameObject);
        //UseAP(5);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, testDamageRadius);

        foreach (var hitCollider in hitColliders)
        {
            Enemy enemy = hitCollider.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.GetDamage(testDamageAmount, this.gameObject);
            }
        }
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        if (stat != null)
        {
            stat.OnHPChanged -= Stat_OnHPChanged;
            stat.OnAPChanged -= Stat_OnAPChanged;
        }
    }

    private void OnDestroy()
    {
        if (stat != null)
        {
            stat.OnHPChanged -= Stat_OnHPChanged;
            stat.OnAPChanged -= Stat_OnAPChanged;
        }
    }

    protected override void Update()
    {
        base.Update();

    }

    public bool TryUseSkill(int SkillNum, float cost)
    {
        if (!UseAP(cost))
        {
            return false;
        }

        return true;
    }
}