using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/SkillRange/SkillRange_Targeting", fileName = "SkillRange_Targeting")]
public class SkillRange_Targeting : SkillRange
{
    // GameObject�� �߽����� Ÿ���� ã�� �޼��� ����
    public override List<StatComponent> FindTargets(GameObject go, float range)
    {
        return FindTargets(go.transform.position, range);
    }

    // ��ġ�� �����Ÿ��� �޾� Ÿ���� ã�� �޼��� ����
    public override List<StatComponent> FindTargets(Vector3 position, float range)
    {
        targets.Clear(); // Ÿ�� ����Ʈ �ʱ�ȭ

        // ���콺 Ŀ�� ��ġ�� �������� ���� ����� ����� ã��
        StatComponent[] components = GameObject.FindObjectsOfType<StatComponent>(); // ��� StatComponent ��������

        // ���� ����� Ÿ���� ã�� ���� ����
        StatComponent closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (StatComponent component in components)
        {
            Vector3 targetPosition = component.transform.position;
            float distance = Vector3.Distance(position, targetPosition);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = component;
            }
        }

        if (closestTarget != null)
        {
            targets.Add(closestTarget); // ���� ����� Ÿ���� ����Ʈ�� �߰�
        }

        return targets; // Ÿ�� ����Ʈ ��ȯ
    }

    // ���� �ð�ȭ �޼���
    public override void DrawRange(Vector3 position, float range)
    {
        // Ÿ���� ��ų�� ���, ������ ���� �ǹ̰� �����Ƿ� ������ �۰� �ð�ȭ
        Gizmos.color = Color.yellow; // ���� ���� ����
        Gizmos.DrawWireSphere(position, 0.5f); // �ſ� ���� ������ �ð�ȭ
    }
}
