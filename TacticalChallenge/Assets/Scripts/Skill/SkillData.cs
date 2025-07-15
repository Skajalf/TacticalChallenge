//using UnityEngine;
//using System.Collections.Generic;

//#region Context and Targeting
//public class SkillContext
//{
//    public StatComponent Caster { get; private set; }
//    public Vector3 TargetPoint { get; private set; }
//    public int CurrentAP { get; set; }
//    public int CurrentStamps { get; set; }

//    public SkillContext(StatComponent caster, Vector3 targetPoint, int currentAP = 0, int currentStamps = 0)
//    {
//        Caster = caster;
//        TargetPoint = targetPoint;
//        CurrentAP = currentAP;
//        CurrentStamps = currentStamps;
//    }
//}

//public enum TargetShape { Point, Circle, Cone, Self }
//public enum TargetFaction { Ally, Enemy, Both }

//[System.Serializable]
//public struct TargetingInfo
//{
//    public TargetShape shape;
//    public float range;
//    public float radius;
//    public float angle;
//    public TargetFaction faction;
//}

//public static class TargetingSystem
//{
//    public static StatComponent[] GetTargets(TargetingInfo info, StatComponent caster, Vector3 point)
//    {
//        List<StatComponent> results = new List<StatComponent>();
//        switch (info.shape)
//        {
//            case TargetShape.Self:
//                results.Add(caster);
//                break;
//            case TargetShape.Point:
//                StatComponent single = FindNearest(point);
//                if (single != null && MatchesFaction(single, caster, info.faction))
//                    results.Add(single);
//                break;
//            case TargetShape.Circle:
//                var hits = Physics.OverlapSphere(point, info.radius);
//                foreach (var hit in hits)
//                {
//                    var u = hit.GetComponent<StatComponent>();
//                    if (u != null && MatchesFaction(u, caster, info.faction))
//                        results.Add(u);
//                }
//                break;
//            case TargetShape.Cone:
//                var coneHits = Physics.OverlapSphere(caster.transform.position, info.range);
//                foreach (var hit in coneHits)
//                {
//                    var dir = (hit.transform.position - caster.transform.position).normalized;
//                    if (Vector3.Angle(caster.transform.forward, dir) <= info.angle * 0.5f)
//                    {
//                        var u = hit.GetComponent<StatComponent>();
//                        if (u != null && MatchesFaction(u, caster, info.faction))
//                            results.Add(u);
//                    }
//                }
//                break;
//        }
//        return results.ToArray();
//    }

//    static StatComponent FindNearest(Vector3 point)
//    {
//        Collider[] hits = Physics.OverlapSphere(point, 0.5f);
//        StatComponent best = null; float minDist = float.MaxValue;
//        foreach (var hit in hits)
//        {
//            var u = hit.GetComponent<StatComponent>();
//            if (u != null)
//            {
//                float d = Vector3.Distance(point, u.transform.position);
//                if (d < minDist) { best = u; minDist = d; }
//            }
//        }
//        return best;
//    }

//    static bool MatchesFaction(StatComponent unit, StatComponent caster, TargetFaction faction)
//    {
//        bool isAlly = unit.Faction == caster.Faction;
//        return faction == TargetFaction.Both || (faction == TargetFaction.Ally && isAlly) || (faction == TargetFaction.Enemy && !isAlly);
//    }
//}
//#endregion

//#region Conditions
//public abstract class ConditionSO : ScriptableObject
//{
//    public abstract bool IsMet(SkillContext ctx);
//}
//#endregion

//#region Effect Modules
//public interface IEffectModule
//{
//    void Apply(SkillContext ctx, StatComponent[] targets);
//}

//public abstract class EffectModuleSO : ScriptableObject, IEffectModule
//{
//    public abstract void Apply(SkillContext ctx, StatComponent[] targets);
//}

//public enum GroupMode { Sequential, Parallel }

//[CreateAssetMenu(menuName = "Skill/EffectGroup")]
//public class EffectGroupSO : ScriptableObject, IEffectModule
//{
//    public GroupMode mode = GroupMode.Sequential;
//    public EffectModuleSO[] modules;

//    public void Apply(SkillContext ctx, StatComponent[] targets)
//    {
//        if (mode == GroupMode.Sequential)
//        {
//            foreach (var m in modules) m.Apply(ctx, targets);
//        }
//        else
//        {
//            foreach (var m in modules) m.Apply(ctx, targets);
//        }
//    }
//}
//#endregion

//#region Triggers
//public enum EventType { OnCast, OnHit, OnEnd }

//[CreateAssetMenu(menuName = "Skill/Trigger")]
//public class TriggerSO : ScriptableObject
//{
//    public EventType eventType;
//    public ConditionSO[] conditions;

//    public bool IsMet(SkillContext ctx)
//    {
//        foreach (var c in conditions)
//            if (!c.IsMet(ctx)) return false;
//        return true;
//    }
//}

//[CreateAssetMenu(menuName = "Skill/TriggerBinding")]
//public class TriggerBindingSO : ScriptableObject
//{
//    public TriggerSO trigger;
//    public EffectModuleSO[] boundEffects;
//}
//#endregion

//#region Skill Definition
//[CreateAssetMenu(menuName = "Skill/Skill")]
//public class SkillSO : ScriptableObject
//{
//    public string skillName;
//    public int apCost;
//    public TargetingInfo targeting;
//    public EffectModuleSO[] effectModules;
//    public TriggerBindingSO[] triggerBindings;
//}
//#endregion