using UnityEngine;

public class StatComponent : MonoBehaviour
{
    [SerializeField] private float maxHealthPoint = 100.0f;
    [SerializeField] private float maxActionPoint = 10.0f;
    [SerializeField] private float APRegen = 0.01f;

    private float currHealthPoint;
    private float currActionPoint;


    public bool Dead { get => currHealthPoint <= 0.0f; }

    private void Start()
    {
        currHealthPoint = maxHealthPoint;
        currActionPoint = 0.0f;
    }

    public void Damage(float amount)
    {
        if (amount < 1.0f)
            return;

        currHealthPoint += (amount * -1.0f);
        currHealthPoint = Mathf.Clamp(currHealthPoint, 0.0f, maxHealthPoint);
    }

    public void APUse(float amount)
    {
        if (currActionPoint < amount)
            return;

        currActionPoint += (amount * -1.0f);
        currActionPoint = Mathf.Clamp(currActionPoint, 0.0f, maxActionPoint);
    }

    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, WeaponData data)
    {
        Damage(data.Power); // 데미지 처리
    }

    private void Update()
    {
        currActionPoint += APRegen;
    }
}