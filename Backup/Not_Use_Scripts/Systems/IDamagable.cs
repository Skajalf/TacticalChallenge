using UnityEngine;

public interface IDamagable
{
    void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, WeaponData data);
}