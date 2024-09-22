using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class StatComponent : MonoBehaviour
{
    [SerializeField] public float maxHealthPoint = 100.0f;
    [SerializeField] public float maxActionPoint = 10.0f;

    [SerializeField] private float apRegenTime = 1.0f; // Regen 주기
    [SerializeField] private float APRegenAmount = 0.1f;  // AP 회복량

    [SerializeField] private float currentHealthPoint; // 현재 HP
    [SerializeField] private float currentActionPoint; // 현재 AP

    public float CurrentHP { get { return currentHealthPoint; } private set { currentHealthPoint = value; } }
    public float CurrentAP => currentActionPoint;

    public bool Dead { get => currentHealthPoint <= 0.0f; }
    private bool isRegenActive = false;

    private void Awake()
    {
        Init();
        StartCoroutine("APCharge");
    }

    private void Init()
    {
        currentHealthPoint = maxHealthPoint;
        currentActionPoint = 0.0f;
    }

    public void Damage(float amount)
    {
        // -1과 1 사이의 값(작은 데미지나 작은 회복)은 무시
        if (amount > -1.0f && amount < 1.0f)
            return;

        currentHealthPoint += (amount * -1.0f);
        currentHealthPoint = Mathf.Clamp(currentHealthPoint, 0.0f, maxHealthPoint);
    }
    
    private void APCharge()
    {
        AP();
    }

    private IEnumerator AP()
    {
        isRegenActive = true; // 코루틴이 실행 중임을 표시
        while (currentActionPoint < maxActionPoint)
        {
            currentActionPoint += APRegenAmount;
            currentActionPoint = Mathf.Clamp(currentActionPoint, 0.0f, maxActionPoint);
            yield return new WaitForSeconds(apRegenTime);
        }
        isRegenActive = false; // 코루틴이 끝났음을 표시
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
        // AP가 최대치에 도달하지 않았고, 코루틴이 실행 중이 아닐 때만 코루틴 시작
        if (currentActionPoint < maxActionPoint && !isRegenActive)
        {
            StartCoroutine(AP());
        }
    }
}