using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/SkillRange/SkillRange_Rectangle", fileName = "SkillRange_Rectangle")]
public class SkillRange_Rectangle : SkillRange
{
    // GameObject를 중심으로 타겟을 찾는 메서드 구현
    public override List<StatComponent> FindTargets(GameObject go, float range)
    {
        return FindTargets(go.transform.position, range);
    }

    // 사각형 범위 내 타겟을 찾는 로직
    public override List<StatComponent> FindTargets(Vector3 position, float range)
    {
        targets.Clear(); // 타겟 리스트 초기화
        StatComponent[] components = GameObject.FindObjectsOfType<StatComponent>(); // 모든 StatComponent 가져오기

        foreach (StatComponent component in components)
        {
            Vector3 offset = component.transform.position - position;

            // 사각형 범위 내 캐릭터를 찾음
            if (Mathf.Abs(offset.x) <= range / 2 && Mathf.Abs(offset.z) <= range / 2)
            {
                targets.Add(component);
            }
        }

        return targets; // 범위 내 타겟 리스트 반환
    }

    // 사각형 범위 시각화
    public override void DrawRange(Vector3 position, float range)
    {
        // 사각형 범위 시각화 - Gizmos를 이용한 시각적 표시
        Vector3 size = new Vector3(range, 1, range); // 사각형 크기 (가로 x, 세로 y, 깊이 z)
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(position, size); // Gizmos로 사각형 그리기
    }
}
