using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Skills/SkillRange/SkillRange_Circle", fileName ="SkillRange_Circle")]

public class SkillRange_Circle : SkillRange
{

    public override List<Character> FindTargets(GameObject go, float range)
    {
        return FindTargets(go.transform.position, range);
    }

    public override List<Character> FindTargets(Vector3 position, float range)
    {
        Character[] chrs;
        Vector3 playerPosition = position;
        chrs = GameObject.FindObjectsByType<Character>(0);

        foreach (Character c in chrs)
        {
            Vector3 distance = playerPosition - c.transform.position;
            if (distance.magnitude < range)
                targets.Add(c);
        }

        return targets;
    }
}
