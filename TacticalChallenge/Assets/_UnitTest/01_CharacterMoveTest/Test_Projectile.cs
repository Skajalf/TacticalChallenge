using UnityEngine;

public class Test_Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 750.0f; // �Ѿ� �ӵ�
    [SerializeField] private float destroyTime = 10.0f; // ���� �ð� �� �Ѿ� �ı� �ð�

    private Rigidbody rb; // Rigidbody ������Ʈ
    public Test_WeaponBase weapon; // Weapon ������ ������ ����

    // Awake �޼��忡�� Rigidbody ��������
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Start �޼��忡�� �ӵ� �����Ͽ� �Ѿ� �߻�
    private void Start()
    {
        if (rb != null)
        {
            rb.velocity = transform.forward * speed; // �ӵ� ����
        }

        // ���� �ð� �� �Ѿ� �ı�
        Destroy(gameObject, destroyTime);
    }

    // �浹 �� �Ѿ� �ı�
    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}
