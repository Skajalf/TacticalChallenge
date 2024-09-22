using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/SkillRange/SkillRange_Rectangle", fileName = "SkillRange_Rectangle")]
public class SkillRange_Rectangle : SkillRange
{
    // GameObject�� �߽����� Ÿ���� ã�� �޼��� ����
    public override List<StatComponent> FindTargets(GameObject go, float range)
    {
        return FindTargets(go.transform.position, range);
    }

    // �簢�� ���� �� Ÿ���� ã�� ����
    public override List<StatComponent> FindTargets(Vector3 position, float range)
    {
        targets.Clear(); // Ÿ�� ����Ʈ �ʱ�ȭ
        StatComponent[] components = GameObject.FindObjectsOfType<StatComponent>(); // ��� StatComponent ��������

        foreach (StatComponent component in components)
        {
            Vector3 offset = component.transform.position - position;

            // �簢�� ���� �� ĳ���͸� ã��
            if (Mathf.Abs(offset.x) <= range / 2 && Mathf.Abs(offset.z) <= range / 2)
            {
                targets.Add(component);
            }
        }

        return targets; // ���� �� Ÿ�� ����Ʈ ��ȯ
    }

    // �簢�� ���� �ð�ȭ
    public override void DrawRange(Vector3 position, float range)
    {
        // �簢�� ���� �ð�ȭ - Gizmos�� �̿��� �ð��� ǥ��
        Vector3 size = new Vector3(range, 1, range); // �簢�� ũ�� (���� x, ���� y, ���� z)
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(position, size); // Gizmos�� �簢�� �׸���
    }
}
