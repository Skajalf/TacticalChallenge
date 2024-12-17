using System.Collections;
using UnityEngine;

public static class WeaponUtility
{
    // Fire 메서드: 무기 공격 시 레이캐스트와 지연된 데미지 적용을 처리
    public static void Fire(Transform weaponTransform, Transform bulletTransform, float range, float damageDelay, float power, LayerMask hitLayerMask, MonoBehaviour caller)
    {
        RaycastHit hit;
        Vector3 fireDirection = weaponTransform.forward; // 발사 방향
        Vector3 startPoint = bulletTransform.position;   // 총알 발사 위치

        // 레이캐스트로 타격 확인
        if (Physics.Raycast(startPoint, fireDirection, out hit, range, hitLayerMask))
        {
            // 타격된 객체의 이름 출력
            Debug.Log($"명중한 객체 이름: {hit.collider.name}");

            // MonoBehaviour를 가진 caller가 코루틴 실행
            caller.StartCoroutine(ApplyDamageWithDelay(hit, damageDelay, power));
        }
        else
        {
            Debug.Log("목표에 명중하지 않았습니다.");
        }
    }

    private static IEnumerator ApplyDamageWithDelay(RaycastHit hit, float delay, float power)
    {
        yield return new WaitForSeconds(delay);

        var target = hit.collider.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(power);
            Debug.Log($"데미지 {power} 적용 완료.");
        }
        else
        {
            Debug.LogWarning("데미지를 적용할 수 없는 대상입니다.");
        }
    }
}
