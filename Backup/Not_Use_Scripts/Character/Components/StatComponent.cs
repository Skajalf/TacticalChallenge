using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class StatComponent : MonoBehaviour
{
    [SerializeField] public float maxHealthPoint = 100.0f;
    [SerializeField] public float maxActionPoint = 10.0f;

    [SerializeField] private float apRegenTime = 1.0f; // Regen �ֱ�
    [SerializeField] private float APRegenAmount = 0.1f;  // AP ȸ����

    [SerializeField] private float currentHealthPoint; // ���� HP
    [SerializeField] private float currentActionPoint; // ���� AP

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
        // -1�� 1 ������ ��(���� �������� ���� ȸ��)�� ����
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
        isRegenActive = true; // �ڷ�ƾ�� ���� ������ ǥ��
        while (currentActionPoint < maxActionPoint)
        {
            currentActionPoint += APRegenAmount;
            currentActionPoint = Mathf.Clamp(currentActionPoint, 0.0f, maxActionPoint);
            yield return new WaitForSeconds(apRegenTime);
        }
        isRegenActive = false; // �ڷ�ƾ�� �������� ǥ��
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
        // AP�� �ִ�ġ�� �������� �ʾҰ�, �ڷ�ƾ�� ���� ���� �ƴ� ���� �ڷ�ƾ ����
        if (currentActionPoint < maxActionPoint && !isRegenActive)
        {
            StartCoroutine(AP());
        }
    }
}