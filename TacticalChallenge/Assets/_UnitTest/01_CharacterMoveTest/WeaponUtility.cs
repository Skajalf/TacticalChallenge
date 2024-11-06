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
            Debug.Log($"{hit.collider.name}에 명중했습니다. 데미지 적용까지 {damageDelay}초 지연.");

            // MonoBehaviour를 가진 caller가 코루틴 실행
            caller.StartCoroutine(ApplyDamageWithDelay(hit, damageDelay, power));
        }
        else
        {
            Debug.Log("목표에 명중하지 않았습니다.");
        }
    }

    // ApplyDamageWithDelay: 지연된 데미지 적용을 위한 코루틴
    private static IEnumerator ApplyDamageWithDelay(RaycastHit hit, float delay, float power)
    {
        // 지연 시간 대기
        yield return new WaitForSeconds(delay);

        //// 타격 대상이 IDamageable을 구현했는지 확인
        //var target = hit.collider.GetComponent<IDamageable>();
        //if (target != null)
        //{
        //    target.TakeDamage(power);
        //    Debug.Log($"데미지 {power} 적용 완료.");
        //}
        //else
        //{
        //    Debug.LogWarning("데미지를 적용할 수 없는 대상입니다.");
        //}
    }
}
