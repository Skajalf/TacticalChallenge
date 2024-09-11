using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 추상 클래스 - 스킬의 범위를 정의하고 타겟을 찾는 로직을 포함
public abstract class SkillRange : ScriptableObject
{
    public List<StatComponent> targets = new List<StatComponent>(); // 범위 내의 타겟 리스트

    // GameObject를 중심으로 타겟을 찾는 추상 메서드
    public abstract List<StatComponent> FindTargets(GameObject go, float range);

    // 위치와 사정거리를 받아 타겟을 찾는 추상 메서드
    public abstract List<StatComponent> FindTargets(Vector3 position, float range);

    // 범위 시각화를 위한 추상 메서드
    public abstract void DrawRange(Vector3 position, float range);
}
