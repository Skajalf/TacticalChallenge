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

    public List<StatComponent> GetTargets(Vector3 position)
    {
        // skillRangeInstance가 null일 경우, 기본적으로 Targeting으로 설정
        if (skillRangeInstance == null)
        {
            Debug.LogWarning("SkillRangeInstance is null, defaulting to Targeting.");
            skillRangeInstance = ScriptableObject.CreateInstance<SkillRange_Targeting>();
        }

        // 현재 인스턴스가 유효한지 확인
        if (skillRangeInstance == null)
        {
            Debug.LogError("SkillRangeInstance is still null after assignment.");
            return new List<StatComponent>();
        }

        return skillRangeInstance.FindTargets(position, SkillRange);
    }
}