using UnityEngine;

public class StatComponent : MonoBehaviour
{
    [SerializeField] public float maxHealthPoint = 100.0f;
    [SerializeField] public float maxActionPoint = 10.0f;

    [SerializeField] private float APRegen = 0.01f;  // TODO : Coroutine으로 수정.

    [SerializeField] private float currentHealthPoint; // 현재 HP
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

    public void APUse(float amount)
    {
        if (currentActionPoint < amount)
            return;

        currentActionPoint += (amount * -1.0f);
        currentActionPoint = Mathf.Clamp(currentActionPoint, 0.0f, maxActionPoint);
    }

    private void Update()
    {
        currentActionPoint += APRegen;
    }
}