using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Targeting = 0, AreaAttack, SelfCast,
}

public enum SkillShapeType
{
    Targeting = 0, Circle, Ellipse, Rectangle, CircularSect,
}

[CreateAssetMenu(fileName = "SkillObject_", menuName = "Skills/SkillObject", order = 1)]
public class SkillObject : ScriptableObject
{
    public float SkillDamage; //데미지 이건 직접 입력
    public float SkillRange; //사거리
    public float SkillCoolTime; //쿨타임
    public float APCost; //AP소모량
    public SkillType skillType; //타겟팅인가? 아니면 범위공격인가? 혹은 자버프 시전?
    public SkillShapeType shapeType; //타겟팅의 경우에도 범위가 있을수 있으니까 일단 포함.
}
