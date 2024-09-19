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
    public bool CanMove; // 스킬 사용할때 움직일 수 있는가?
    public bool InstantCast; // 스킬을 발동 즉시 시전하는가?
    public float SkillDamage; // 데미지 이건 직접 입력
    public float SkillRange; // 사거리
    public float SkillCoolTime; // 쿨타임
    public float APCost; // AP 소모량
    public SkillType skillType; // 스킬 유형 (타겟팅, 범위 공격, 자버프)
    public SkillShapeType shapeType; // 스킬 모양 타입 (타겟팅, 원형, 타원형, 사각형, 부채꼴)

    public SkillRange skillRangeInstance; // 스킬 범위에 맞는 인스턴스

    [Header(" Detail Setting")]
    public bool enableTargetAllies = false; // 타겟팅 시 아군만 선택

    // GameObject를 기준으로 타겟을 찾는 경우
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

        // GameObject를 기반으로 타겟을 찾는 FindTargets 호출
        return skillRangeInstance.FindTargets(caster, SkillRange);
    }

    // Vector3 위치를 기준으로 타겟을 찾는 경우
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

        // 위치를 기반으로 타겟을 찾는 FindTargets 호출
        return skillRangeInstance.FindTargets(position, SkillRange);
    }

    // GameObject 기준으로 스킬을 실행하는 메서드
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

    // Vector3 위치 기준으로 스킬을 실행하는 메서드
    public void ExecuteSkill(Vector3 targetPosition)
    {
        List<StatComponent> targets = GetTargets(targetPosition);

        if (targets.Count > 0)
        {
            ApplySkill(null, targets, SkillDamage); // 여기선 캐스터 대신 null을 넘김
        }
        else
        {
            Debug.Log("No valid targets found.");
        }
    }

    // 스킬을 적용하는 메서드 (구현 필요)
    private void ApplySkill(GameObject caster, List<StatComponent> targets, float skillDamage)
    {
        Debug.Log("SkillObject :: ApplySkill");
        foreach (StatComponent target in targets)
        {
            // 스킬 적용 로직 (데미지, 상태 효과 등)
            Debug.Log($"target : {target}");
            target.Damage(skillDamage);
        }
    }
}
