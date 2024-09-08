using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Skills/SkillType_AreaSelect", fileName ="SkillType_AreaSelect")]

public class SkillType_AreaSelect : ScriptableObject
{
    [SerializeField] SkillRange SkillRangeType;
    [SerializeField] private float from_position_range; // ��ų�� �� ��ġ�� ������ ����
    [SerializeField] private float select_range; // ��ġ�� ���� �� �ִ� ����

    private List<Character> Targets;

    private Vector3 position; // ��ų�� �� ��ġ
   
    public List<Character> SelectPosition() 
    {
        // �� �߽����� ��ǥ �ϳ� �����ؼ�
        
        // Targets = SkillRangeType.FindTargets(position, from_position_range);

        return Targets;
    }
}
