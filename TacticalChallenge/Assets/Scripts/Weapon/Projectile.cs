using System.Collections;
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
        if (other.CompareTag("Enemy") || other.CompareTag("Environment"))
        {
            ObjectPoolingManager.Instance.ReturnToPool(gameObject);
            gameObject.SetActive(false);
        }
    }

    public bool Shoot(Vector3 initialLocation, Vector3 direction, float speed, float time)
    {
        if (rb != null)
        {
            rb.velocity = direction * speed; // direction 벡터에 속도를 곱하여 적용
        }

        // 일정 시간 후 오브젝트 풀로 반환하는 코루틴 시작
        StartCoroutine(ReturnToPool(time));

        return true;
    }

    private IEnumerator ReturnToPool(float time)
    {
        yield return new WaitForSeconds(time);
        ObjectPoolingManager.Instance.ReturnToPool(gameObject);
    }

    public void OnDisable()
    {
        // 파괴되거나, 비활성화 시킬 때 => PoolManager로 반납할 때...
    }
}
