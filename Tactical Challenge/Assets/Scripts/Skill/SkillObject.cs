using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Targeting = 0, AreaAttack, SelfCast,
}

public enum SkillShapeType
{
    Targeting = 0, Circle, Rectangle, CircularSect,
}

[CreateAssetMenu(fileName = "SkillObject_", menuName = "Skills/SkillObject", order = 1)]
public class SkillObject : ScriptableObject
{
    [Header(" Skill Setting")]
    public bool CanMove; // ��ų ����Ҷ� ������ �� �ִ°�?
    public bool InstantCast; // ��ų�� �ߵ� ��� �����ϴ°�?
    public float SkillDamage; // ������ �̰� ���� �Է�
    public float SkillRange; // ��Ÿ�
    public float SkillCoolTime; // ��Ÿ��
    public float APCost; // AP �Ҹ�
    public SkillType skillType; // ��ų ���� (Ÿ����, ���� ����, �ڹ���)
    public SkillShapeType shapeType; // ��ų ��� Ÿ�� (Ÿ����, ����, Ÿ����, �簢��, ��ä��)

    public SkillRange skillRangeInstance; // ��ų ������ �´� �ν��Ͻ�

    [Header(" Detail Setting")]
    public bool enableTargetAllies = false; // Ÿ���� �� �Ʊ��� ����

    public List<StatComponent> GetTargets(Vector3 position)
    {
        // skillRangeInstance�� null�� ���, �⺻������ Targeting���� ����
        if (skillRangeInstance == null)
        {
            Debug.LogWarning("SkillRangeInstance is null, defaulting to Targeting.");
            skillRangeInstance = ScriptableObject.CreateInstance<SkillRange_Targeting>();
        }

        // ���� �ν��Ͻ��� ��ȿ���� Ȯ��
        if (skillRangeInstance == null)
        {
            Debug.LogError("SkillRangeInstance is still null after assignment.");
            return new List<StatComponent>();
        }

        return skillRangeInstance.FindTargets(position, SkillRange);
    }
}