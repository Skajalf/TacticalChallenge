using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 75.0f; // �Ѿ� �ӵ�
    [SerializeField] private float destroyTime = 10.0f; // ���� �ð� �� �Ѿ� ��Ȱ��ȭ

    private Rigidbody rb; // Rigidbody ������Ʈ
    public WeaponBase weapon; // Weapon ������ ������ ����

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
        //Destroy(gameObject, destroyTime);
    }

    private void Update()
    {
        rb.AddForce(new Vector3(0f, 0.05f , 0f));
    }

    // �浹 �� �Ѿ� �ı�
    private void OnTriggerEnter(Collider other)
    {
        
    }
}
