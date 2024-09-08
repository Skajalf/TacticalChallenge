using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillObject_", menuName = "Skills/SkillObject", order = 1)]
public class SkillObject : ScriptableObject
{
    public float SkillDamage; //������
    public float AttackRange; //��Ÿ�
    public float SkillCoolTime; //��Ÿ��
    public float APCost; //AP�Ҹ�
    public Skill skill; //��ų�� ����
    //public SkillRangeType SkillRangeType;// ��ų ���� ����� ���� Ÿ�����̰ų� �ڹ��� ������ ��� None, �� ���� ����(����, Ÿ����, ���簢��, ��ä��)�� �� ���¿� �°� ����
    public float SkillRange;//��ų ���� Ÿ�����̰ų� �ڹ��� ������ ��� 1�� ����.
    public SkillType skillType;

    public void OnSkill()
    {
        skill.OnSkill();
    }
}
