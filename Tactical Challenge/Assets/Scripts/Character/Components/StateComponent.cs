using System;
using UnityEngine;

public class StateComponent : MonoBehaviour
{
    public enum StateType
    {
        Idle, Equip, Action, Skill, Reload, Damaged, Dead,
    }
    private StateType type = StateType.Idle;

    public event Action<StateType, StateType> OnStateTypeChanged;

    public bool IdleMode { get => type == StateType.Idle; }
    public bool EquipMode { get => type == StateType.Equip; }
    public bool ActionMode { get => type == StateType.Action; }
    public bool SkillMode { get => type == StateType.Skill; }
    public bool ReloadMode { get => type == StateType.Reload; }
    public bool DamagedMode { get => type == StateType.Damaged; }
    public bool DeadMode { get => type == StateType.Dead; }

    public void SetIdleMode() => ChangeType(StateType.Idle);
    public void SetEquipMode() => ChangeType(StateType.Equip);
    public void SetActionMode() => ChangeType(StateType.Action);
    public void SetSkillMode() => ChangeType(StateType.Skill);
    public void SetReloadMode() => ChangeType(StateType.Reload);
    public void SetDamagedMode() => ChangeType(StateType.Damaged);
    public void SetDeadMode() => ChangeType(StateType.Dead);

    private void ChangeType(StateType type)
    {
        if(this.type == type) return;

        StateType prevtype = this.type;
        this.type = type;

        OnStateTypeChanged?.Invoke(prevtype, type);
    }
}