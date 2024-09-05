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
    public float SkillDamage;//������
    public float AttackRange;//��Ÿ�
    public float SkillCoolTime;//��Ÿ��
    public float APCost;//AP�Ҹ�
    public SkillType skillType;//��ų�� ����
    public SkillRangeType SkillRangeType;// ��ų ���� ����� ���� Ÿ�����̰ų� �ڹ��� ������ ��� None, �� ���� ����(����, Ÿ����, ���簢��, ��ä��)�� �� ���¿� �°� ����
    public float SkillRange;//��ų ���� Ÿ�����̰ų� �ڹ��� ������ ��� 1�� ����.
}
