using UnityEngine;

public class StatComponent : MonoBehaviour
{
    [SerializeField] private float maxHealthPoint = 100.0f;
    [SerializeField] private float maxActionPoint = 10.0f;
    [SerializeField] private float APRegen = 0.01f;
    //TODO: 5초 이상 피해를 입지 않는다면 체력이 30% 이하인 경우 체력이 천천히 채워지는 기능 추가해야함.

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

    private void Update()
    {
        currActionPoint += APRegen;
    }
}