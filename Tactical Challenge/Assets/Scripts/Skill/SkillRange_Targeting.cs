using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/SkillRange/SkillRange_Targeting", fileName = "SkillRange_Targeting")]
public class SkillRange_Targeting : SkillRange
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

        // 마우스 커서 위치를 기준으로 가장 가까운 대상을 찾기
        StatComponent[] components = GameObject.FindObjectsOfType<StatComponent>(); // 모든 StatComponent 가져오기

        // 가장 가까운 타겟을 찾기 위한 변수
        StatComponent closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (StatComponent component in components)
        {
            Vector3 targetPosition = component.transform.position;
            float distance = Vector3.Distance(position, targetPosition);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = component;
            }
        }

        if (closestTarget != null)
        {
            targets.Add(closestTarget); // 가장 가까운 타겟을 리스트에 추가
        }

        return targets; // 타겟 리스트 반환
    }

    // 범위 시각화 메서드
    public override void DrawRange(Vector3 position, float range)
    {
        // 타겟팅 스킬의 경우, 범위는 보통 의미가 없으므로 범위를 작게 시각화
        Gizmos.color = Color.yellow; // 색상 조정 가능
        Gizmos.DrawWireSphere(position, 0.5f); // 매우 작은 범위로 시각화
    }
}
