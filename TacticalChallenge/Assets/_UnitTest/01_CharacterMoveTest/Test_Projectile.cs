using UnityEngine;

public class Test_Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 750.0f;          // 탄속 (유닛/초)
    [SerializeField] private float range = 50.0f;           // 유효 사거리 (유닛)
    [SerializeField] private float destroyAfterRange = 1.0f; // 유효 사거리 벗어난 후 파괴까지 시간
    [SerializeField] private float destroyTime = 10.0f;     // 충돌하지 않았을 때 최대 생존 시간

    public WeaponBase weapon;

    private new Rigidbody rigidbody;
    private Vector3 initialPosition;  // 발사 위치
    private float travelTime;         // 유효 사거리 내에서 이동할 시간

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        initialPosition = transform.position;  // 발사 시 위치 저장

        // 유효 사거리 동안 이동할 시간 = 사거리 ÷ 탄속
        travelTime = range / speed;

        // 초기 속도 설정 (velocity)
        rigidbody.velocity = transform.forward * speed;

        // 중력 비활성화
        rigidbody.useGravity = false;

        // 유효 사거리 이후 중력 적용
        Invoke(nameof(ApplyGravity), travelTime);

        // 최대 생존 시간 이후 파괴
        Destroy(gameObject, destroyTime);
    }

    private void ApplyGravity()
    {
        // 중력 다시 적용
        rigidbody.useGravity = true;

        // 유효 사거리 벗어난 후 destroyAfterRange 시간 후 파괴
        Destroy(gameObject, destroyAfterRange);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 물체에 부딪혔을 때 즉시 파괴
        Destroy(gameObject);
    }
}
