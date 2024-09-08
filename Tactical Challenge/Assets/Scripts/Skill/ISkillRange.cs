using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillRange : ScriptableObject
{
    public List<Character> targets;
    public virtual List<Character> FindTargets(GameObject go, float range) { return null; }
    public virtual List<Character> FindTargets(Vector3 position, float range) { return null; }
}
