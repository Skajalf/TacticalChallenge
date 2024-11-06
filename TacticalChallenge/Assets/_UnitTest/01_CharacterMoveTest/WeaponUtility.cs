using System.Collections;
using UnityEngine;

public static class WeaponUtility
{
    // Fire �޼���: ���� ���� �� ����ĳ��Ʈ�� ������ ������ ������ ó��
    public static void Fire(Transform weaponTransform, Transform bulletTransform, float range, float damageDelay, float power, LayerMask hitLayerMask, MonoBehaviour caller)
    {
        RaycastHit hit;
        Vector3 fireDirection = weaponTransform.forward; // �߻� ����
        Vector3 startPoint = bulletTransform.position;   // �Ѿ� �߻� ��ġ

        // ����ĳ��Ʈ�� Ÿ�� Ȯ��
        if (Physics.Raycast(startPoint, fireDirection, out hit, range, hitLayerMask))
        {
            Debug.Log($"{hit.collider.name}�� �����߽��ϴ�. ������ ������� {damageDelay}�� ����.");

            // MonoBehaviour�� ���� caller�� �ڷ�ƾ ����
            caller.StartCoroutine(ApplyDamageWithDelay(hit, damageDelay, power));
        }
        else
        {
            Debug.Log("��ǥ�� �������� �ʾҽ��ϴ�.");
        }
    }

    // ApplyDamageWithDelay: ������ ������ ������ ���� �ڷ�ƾ
    private static IEnumerator ApplyDamageWithDelay(RaycastHit hit, float delay, float power)
    {
        // ���� �ð� ���
        yield return new WaitForSeconds(delay);

        //// Ÿ�� ����� IDamageable�� �����ߴ��� Ȯ��
        //var target = hit.collider.GetComponent<IDamageable>();
        //if (target != null)
        //{
        //    target.TakeDamage(power);
        //    Debug.Log($"������ {power} ���� �Ϸ�.");
        //}
        //else
        //{
        //    Debug.LogWarning("�������� ������ �� ���� ����Դϴ�.");
        //}
    }
}
