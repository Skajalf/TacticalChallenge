using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �߻� Ŭ���� - ��ų�� ������ �����ϰ� Ÿ���� ã�� ������ ����
public abstract class SkillRange : ScriptableObject
{
    public List<StatComponent> targets = new List<StatComponent>(); // ���� ���� Ÿ�� ����Ʈ

    // GameObject�� �߽����� Ÿ���� ã�� �߻� �޼���
    public abstract List<StatComponent> FindTargets(GameObject go, float range);

    // ��ġ�� �����Ÿ��� �޾� Ÿ���� ã�� �߻� �޼���
    public abstract List<StatComponent> FindTargets(Vector3 position, float range);

    // ���� �ð�ȭ�� ���� �߻� �޼���
    public abstract void DrawRange(Vector3 position, float range);
}
