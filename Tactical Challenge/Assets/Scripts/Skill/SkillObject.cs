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
    public float SkillDamage; //������ �̰� ���� �Է�
    public float SkillRange; //��Ÿ�
    public float SkillCoolTime; //��Ÿ��
    public float APCost; //AP�Ҹ�
    public SkillType skillType; //Ÿ�����ΰ�? �ƴϸ� ���������ΰ�? Ȥ�� �ڹ��� ����?
    public SkillShapeType shapeType; //Ÿ������ ��쿡�� ������ ������ �����ϱ� �ϴ� ����.
}
