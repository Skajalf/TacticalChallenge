using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 75.0f; // 총알 속도
    [SerializeField] private float destroyTime = 10.0f; // 일정 시간 후 총알 비활성화

    private Rigidbody rb; // Rigidbody 컴포넌트
    public WeaponBase weapon; // Weapon 정보를 저장할 변수

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
        //Destroy(gameObject, destroyTime);
    }

    private void Update()
    {
        //rb.AddForce(new Vector3(0f, 0.05f , 0f));
    }

    // 충돌 시 총알 파괴
    private void OnTriggerEnter(Collider other)
    {
        //상대방에게 정보 전달 후(RigidBody 가 아니라서 상관없을 듯?), PoolManager로 반납
    }
    
    public static bool Shoot(Vector3 initialLocation, Vector3 direction, float speed, float time)
    {
        // WeaponBase.Fire()에서 호출하는게 좋을 것 같음. 그러려면, public static으로 되어있어야 할 것이고.. 
        // initialLocation은 로컬포지션(weapon 하단에 붙을 듯)이 될 거고, direction과 speed, time 모두 그렇다.

        // if(time > time) PoolManager.Return(gameObject)
    }

    public void OnDisable()
    {
        // 파괴되거나, 비활성화 시킬 때 => PoolManager로 반납할 때...
    }
}
