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

    // 충돌 시 총알 파괴
    private void OnTriggerEnter(Collider other)
    {
        ObjectPoolingManager.Instance.ReturnToPool(gameObject);
        gameObject.SetActive(false);
    }

    public bool Shoot(Vector3 initialLocation, Vector3 direction, float speed, float time)
    {
        // WeaponBase.Fire()에서 호출하는게 좋을 것 같음. 그러려면, public static으로 되어있어야 할 것이고.. 
        // initialLocation은 로컬포지션(weapon 하단에 붙을 듯)이 될 거고, direction과 speed, time 모두 그렇다.
        if (rb != null)
        {
            rb.velocity = direction * speed; // 속도 설정
        }

        // if(time > time) PoolManager.Return(gameObject)
        return true;
    }

    public void OnDisable()
    {
        // 파괴되거나, 비활성화 시킬 때 => PoolManager로 반납할 때...
    }
}
