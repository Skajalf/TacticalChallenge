//using UnityEngine;
//using System.Collections;

//[RequireComponent(typeof(StatComponent))]
//public class SkillComponent : MonoBehaviour
//{
//    private StatComponent _caster;

//    void Awake()
//    {
//        _caster = GetComponent<StatComponent>();
//    }

//    public IEnumerator Cast(SkillSO skill, Vector3 aimPoint)
//    {
//        if (skill == null) yield break;

//        var ctx = new SkillContext(_caster, aimPoint, _caster.CurrentAP, _caster.CurrentStamps);
//        _caster.CurrentAP -= skill.apCost;

//        Unit[] targets = TargetingSystem.GetTargets(skill.targeting, _caster, aimPoint);

//        foreach (var module in skill.effectModules)
//            module.Apply(ctx, targets);

//        foreach (var binding in skill.triggerBindings)
//            if (binding.trigger.eventType == EventType.OnCast && binding.trigger.IsMet(ctx))
//                foreach (var m in binding.boundEffects)
//                    m.Apply(ctx, targets);

//        yield break;
//    }
//}