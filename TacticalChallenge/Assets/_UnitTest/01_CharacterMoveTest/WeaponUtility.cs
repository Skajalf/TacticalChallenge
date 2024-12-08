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
            // Ÿ�ݵ� ��ü�� �̸� ���
            Debug.Log($"������ ��ü �̸�: {hit.collider.name}");

            // MonoBehaviour�� ���� caller�� �ڷ�ƾ ����
            caller.StartCoroutine(ApplyDamageWithDelay(hit, damageDelay, power));
        }
        else
        {
            Debug.Log("��ǥ�� �������� �ʾҽ��ϴ�.");
        }
    }

    private static IEnumerator ApplyDamageWithDelay(RaycastHit hit, float delay, float power)
    {
        yield return new WaitForSeconds(delay);

        var target = hit.collider.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(power);
            Debug.Log($"������ {power} ���� �Ϸ�.");
        }
        else
        {
            Debug.LogWarning("�������� ������ �� ���� ����Դϴ�.");
        }
    }
}
