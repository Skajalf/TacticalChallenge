using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/SkillRange/SkillRange_Circle", fileName = "SkillRange_Circle")]
public class SkillRange_Circle : SkillRange
{
    // GameObject를 중심으로 타겟을 찾는 메서드 구현
    public override List<StatComponent> FindTargets(GameObject go, float range)
    {
        return FindTargets(go.transform.position, range);
    }

    // 위치와 사정거리를 받아 타겟을 찾는 메서드 구현
    public override List<StatComponent> FindTargets(Vector3 position, float range)
    {
        targets.Clear(); // 타겟 리스트 초기화
        StatComponent[] components = GameObject.FindObjectsOfType<StatComponent>(); // 모든 캐릭터 가져오기

        // 범위 내 타겟 탐색
        foreach (StatComponent component in components)
        {
            Vector3 distance = position - component.transform.position;
            if (distance.magnitude <= range) // 스킬 범위 안에 있는지 확인
            {
                targets.Add(component); // 타겟 리스트에 추가
            }
        }

        return targets; // 범위 내 타겟 리스트 반환
    }

    // 원형 범위 시각화
    public override void DrawRange(Vector3 position, float range)
    {
        // Debug.DrawLine 또는 Gizmos로 원형 범위를 시각적으로 표시
        Debug.DrawLine(position, position + Vector3.forward * range, Color.green);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(position, range); // Gizmos를 사용하여 원형 범위를 표시
    }
}
