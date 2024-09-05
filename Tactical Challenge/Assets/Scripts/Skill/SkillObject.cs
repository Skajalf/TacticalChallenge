using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Targeting = 0, AreaAttack, SelfCast,
}

public enum SkillRangeType
{
    None = 0, Circle, Ellipse, Rectangle, CircularSect, 
}

[CreateAssetMenu(fileName = "NewSkillObject", menuName = "Skills/SkillObject", order = 1)]
public class SkillObject : ScriptableObject
{
    public float SkillDamage;//데미지
    public float AttackRange;//사거리
    public float SkillCoolTime;//쿨타임
    public float APCost;//AP소모량
    public SkillType skillType;//스킬의 종류
    public SkillRangeType SkillRangeType;// 스킬 범위 모양의 형태 타겟팅이거나 자버프 형태인 경우 None, 이 외의 형태(원형, 타원형, 직사각형, 부채꼴)는 각 형태에 맞게 선택
    public float SkillRange;//스킬 범위 타게팅이거나 자버프 형태인 경우 1로 지정.
}
