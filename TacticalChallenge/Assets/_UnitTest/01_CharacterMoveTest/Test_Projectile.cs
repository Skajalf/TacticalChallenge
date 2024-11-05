using UnityEngine;

public class Test_Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 750.0f;          // ź�� (����/��)
    [SerializeField] private float range = 50.0f;           // ��ȿ ��Ÿ� (����)
    [SerializeField] private float destroyAfterRange = 1.0f; // ��ȿ ��Ÿ� ��� �� �ı����� �ð�
    [SerializeField] private float destroyTime = 10.0f;     // �浹���� �ʾ��� �� �ִ� ���� �ð�

    public WeaponBase weapon;

    private new Rigidbody rigidbody;
    private Vector3 initialPosition;  // �߻� ��ġ
    private float travelTime;         // ��ȿ ��Ÿ� ������ �̵��� �ð�

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        initialPosition = transform.position;  // �߻� �� ��ġ ����

        // ��ȿ ��Ÿ� ���� �̵��� �ð� = ��Ÿ� �� ź��
        travelTime = range / speed;

        // �ʱ� �ӵ� ���� (velocity)
        rigidbody.velocity = transform.forward * speed;

        // �߷� ��Ȱ��ȭ
        rigidbody.useGravity = false;

        // ��ȿ ��Ÿ� ���� �߷� ����
        Invoke(nameof(ApplyGravity), travelTime);

        // �ִ� ���� �ð� ���� �ı�
        Destroy(gameObject, destroyTime);
    }

    private void ApplyGravity()
    {
        // �߷� �ٽ� ����
        rigidbody.useGravity = true;

        // ��ȿ ��Ÿ� ��� �� destroyAfterRange �ð� �� �ı�
        Destroy(gameObject, destroyAfterRange);
    }

    private void OnTriggerEnter(Collider other)
    {
        // ��ü�� �ε����� �� ��� �ı�
        Destroy(gameObject);
    }
}
