using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/SkillRange/SkillRange_Circle", fileName = "SkillRange_Circle")]
public class SkillRange_Circle : SkillRange
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
        StatComponent[] components = GameObject.FindObjectsOfType<StatComponent>(); // ��� ĳ���� ��������

        // ���� �� Ÿ�� Ž��
        foreach (StatComponent component in components)
        {
            Vector3 distance = position - component.transform.position;
            if (distance.magnitude <= range) // ��ų ���� �ȿ� �ִ��� Ȯ��
            {
                targets.Add(component); // Ÿ�� ����Ʈ�� �߰�
            }
        }

        return targets; // ���� �� Ÿ�� ����Ʈ ��ȯ
    }

    // ���� ���� �ð�ȭ
    public override void DrawRange(Vector3 position, float range)
    {
        // Debug.DrawLine �Ǵ� Gizmos�� ���� ������ �ð������� ǥ��
        Debug.DrawLine(position, position + Vector3.forward * range, Color.green);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(position, range); // Gizmos�� ����Ͽ� ���� ������ ǥ��
    }
}
