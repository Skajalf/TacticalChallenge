using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StatComponent : MonoBehaviour
{
    [SerializeField] protected float baseMaxHP = 200f;
    [SerializeField] protected float baseMaxAP = 10f;
    [SerializeField] protected float baseMaxSpeed = 5f;
    [SerializeField] protected float baseMaxDefence = 15f;

    [SerializeField] private float initialAP = 0f;

    private float currentHP;
    private float currentAP;
    private float currentSpeed;
    private float currentDefence;

    public float MaxHP { get; private set; }
    public float MaxAP { get; private set; }
    public float MaxSpeed { get; private set; }
    public float MaxDefence { get; private set; }

    public event Action<float, float> OnHPChanged;
    public event Action<float, float> OnAPChanged;
    public event Action<float, float> OnSpeedChanged;
    public event Action<float, float> OnDefenceChanged;

    protected virtual void Awake()
    {
        RecalculateMaxValues();
        currentHP = MaxHP;
        currentAP = Mathf.Clamp(initialAP, 0f, MaxAP);
        currentSpeed = MaxSpeed;
        currentDefence = MaxDefence;
    }

    public virtual void RecalculateMaxValues()
    {
        MaxHP = baseMaxHP;
        MaxSpeed = baseMaxSpeed;
        MaxAP = baseMaxAP;
        MaxDefence = baseMaxDefence;

        currentHP = Mathf.Min(currentHP, MaxHP);
        currentAP = Mathf.Min(currentAP, MaxAP);

        OnHPChanged?.Invoke(currentHP, MaxHP);
        OnAPChanged?.Invoke(currentAP, MaxAP);
    }

    public float CurrentHP => currentHP;
    public float CurrentAP => currentAP;

    public virtual bool TakeDamage(float damage, GameObject giver = null)
    {
        float final = Mathf.Max(0f, damage - currentDefence);
        currentHP = Mathf.Max(0f, currentHP - final);
        OnHPChanged?.Invoke(currentHP, MaxHP);
        return currentHP <= 0f;
    }

    public virtual void Heal(float amount)
    {
        if (amount <= 0f) return;
        currentHP = Mathf.Min(MaxHP, currentHP + amount);
        OnHPChanged?.Invoke(currentHP, MaxHP);
    }

    public virtual bool UseAP(float cost)
    {
        if (cost <= 0f) return true;
        if (currentAP < cost) return false;
        currentAP -= cost;
        OnAPChanged?.Invoke(currentAP, MaxAP);
        return true;
    }

    public virtual void RecoverAP(float amount)
    {
        if (amount <= 0f) return;
        currentAP = Mathf.Min(MaxAP, currentAP + amount);
        OnAPChanged?.Invoke(currentAP, MaxAP);
    }
}