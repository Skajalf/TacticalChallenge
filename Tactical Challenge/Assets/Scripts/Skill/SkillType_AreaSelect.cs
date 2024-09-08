using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Skills/SkillType_AreaSelect", fileName ="SkillType_AreaSelect")]

public class SkillType_AreaSelect : ScriptableObject
{
    [SerializeField] SkillRange SkillRangeType;
    [SerializeField] private float from_position_range; // 스킬을 쓸 위치로 부터의 범위
    [SerializeField] private float select_range; // 위치를 정할 수 있는 범위

    private List<Character> Targets;

    private Vector3 position; // 스킬을 쓸 위치
   
    public List<Character> SelectPosition() 
    {
        // 내 중심으로 좌표 하나 선택해서
        
        // Targets = SkillRangeType.FindTargets(position, from_position_range);

        return Targets;
    }
}
