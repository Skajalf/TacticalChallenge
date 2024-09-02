using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]

public class Enemy : Character, IDamagable
{
    private StatComponent stat;

    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, WeaponData data)
    {
        stat.Damage(data.Power);
        MovableStopper.Instance.Start_Delay(data.StopFrame);

        transform.LookAt(attacker.transform, Vector3.up);
        return;
    }
}