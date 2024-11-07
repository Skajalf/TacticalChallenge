using UnityEngine;

public class Test_Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 750.0f; // 총알 속도
    [SerializeField] private float destroyTime = 10.0f; // 일정 시간 후 총알 파괴 시간

    private Rigidbody rb; // Rigidbody 컴포넌트
    public Test_WeaponBase weapon; // Weapon 정보를 저장할 변수

    // Awake 메서드에서 Rigidbody 가져오기
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Start 메서드에서 속도 설정하여 총알 발사
    private void Start()
    {
        if (rb != null)
        {
            rb.velocity = transform.forward * speed; // 속도 설정
        }

        // 일정 시간 후 총알 파괴
        Destroy(gameObject, destroyTime);
    }

    // 충돌 시 총알 파괴
    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}
