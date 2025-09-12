using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Enemy : Character
{
    public event Action<float, float> OnHPChanged;

    private void Stat_OnHPChanged(float cur, float max)
    {
        OnHPChanged?.Invoke(cur, max);
    }

    protected override void Awake()
    {
        base.Awake();

        if (stat != null)
        {
            stat.OnHPChanged += Stat_OnHPChanged;
        }
    }

    private void OnDestroy()
    {
        if (stat != null)
        {
            stat.OnHPChanged -= Stat_OnHPChanged;
        }
    }
}