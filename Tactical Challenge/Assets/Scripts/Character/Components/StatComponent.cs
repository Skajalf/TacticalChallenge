using UnityEngine;
using System.Collections;


public class StatComponent : MonoBehaviour
{
    [SerializeField] public float maxHealthPoint = 100.0f;
    [SerializeField] public float maxActionPoint = 10.0f;

    [SerializeField] private float apRegenTime = 0.01f; // Regen 주기
    [SerializeField] private float APRegenAmount = 0.01f;  // AP 회복량

    private float currentHealthPoint; // 현재 HP
    private float currentActionPoint; // 현재 AP

    public float CurrentHP { get { return currentHealthPoint; } private set { currentHealthPoint = value; } }
    public float CurrentAP => currentActionPoint;

    public bool Dead { get => currentHealthPoint <= 0.0f; }

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        currentHealthPoint = maxHealthPoint;
        currentActionPoint = 0.0f;
    }

    public void Damage(float amount)
    {
        if (amount < 1.0f)
            return;

        currentHealthPoint += (amount * -1.0f);
        currentHealthPoint = Mathf.Clamp(currentHealthPoint, 0.0f, maxHealthPoint);
    }

    private IEnumerator AP()
    {
        currentActionPoint += APRegenAmount;
        yield return new WaitForSeconds(apRegenTime);
    }

    public void APUse(float amount)
    {
        if (currentActionPoint < amount)
            return;

        currentActionPoint += (amount * -1.0f);
        currentActionPoint = Mathf.Clamp(currentActionPoint, 0.0f, maxActionPoint);
    }

    private void Update()
    {
        
    }
}