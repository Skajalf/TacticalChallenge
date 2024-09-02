using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]

public class Enemy : Character
{
    private StatComponent statComponent;

    private void Awake()
    {
        statComponent = GetComponent<StatComponent>();
    }

    public override void OnDamage(float damage)
    {
        statComponent.Damage(damage);
    }
}