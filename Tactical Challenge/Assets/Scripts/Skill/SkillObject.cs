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

    // GameObject�� �������� Ÿ���� ã�� ���
    public List<StatComponent> GetTargets(GameObject caster)
    {
        if (skillRangeInstance == null)
        {
            Debug.LogWarning("SkillRangeInstance is null, defaulting to Targeting.");
            skillRangeInstance = ScriptableObject.CreateInstance<SkillRange_Targeting>();
        }

        if (skillRangeInstance == null)
        {
            Debug.LogError("SkillRangeInstance is still null after assignment.");
            return new List<StatComponent>();
        }

        // GameObject�� ������� Ÿ���� ã�� FindTargets ȣ��
        return skillRangeInstance.FindTargets(caster, SkillRange);
    }

    // Vector3 ��ġ�� �������� Ÿ���� ã�� ���
    public List<StatComponent> GetTargets(Vector3 position)
    {
        if (skillRangeInstance == null)
        {
            Debug.LogWarning("SkillRangeInstance is null, defaulting to Targeting.");
            skillRangeInstance = ScriptableObject.CreateInstance<SkillRange_Targeting>();
        }

        if (skillRangeInstance == null)
        {
            Debug.LogError("SkillRangeInstance is still null after assignment.");
            return new List<StatComponent>();
        }

        // ��ġ�� ������� Ÿ���� ã�� FindTargets ȣ��
        return skillRangeInstance.FindTargets(position, SkillRange);
    }

    // GameObject �������� ��ų�� �����ϴ� �޼���
    public void ExecuteSkill(GameObject caster)
    {
        Debug.Log("SkillObject :: ExecuteSkill");
        List<StatComponent> targets = GetTargets(caster);

        if (targets.Count > 0)
        {
            ApplySkill(caster, targets, SkillDamage);
        }
        else
        {
            Debug.Log("No valid targets found.");
        }
    }

    // Vector3 ��ġ �������� ��ų�� �����ϴ� �޼���
    public void ExecuteSkill(Vector3 targetPosition)
    {
        List<StatComponent> targets = GetTargets(targetPosition);

        if (targets.Count > 0)
        {
            ApplySkill(null, targets, SkillDamage); // ���⼱ ĳ���� ��� null�� �ѱ�
        }
        else
        {
            Debug.Log("No valid targets found.");
        }
    }

    // ��ų�� �����ϴ� �޼��� (���� �ʿ�)
    private void ApplySkill(GameObject caster, List<StatComponent> targets, float skillDamage)
    {
        Debug.Log("SkillObject :: ApplySkill");
        foreach (StatComponent target in targets)
        {
            // ��ų ���� ���� (������, ���� ȿ�� ��)
            Debug.Log($"target : {target}");
            target.Damage(skillDamage);
        }
    }
}
