using System;
using UnityEngine;


public enum StateType
{
    Idle, Equip, Action, Skill, Damaged, Dead, Reload
}

public class StateComponent : MonoBehaviour
{
    private StateType currentState = StateType.Idle;

    public StateType CurrentState { private set { currentState = value; }  get { return currentState; } }

    public event Action<StateType, StateType> OnStateTypeChanged;

    public bool IdleMode { get => currentState == StateType.Idle; }
    public bool EquipMode { get => currentState == StateType.Equip; }
    public bool ActionMode { get => currentState == StateType.Action; }
    public bool SkillMode { get => currentState == StateType.Skill; }
    public bool DamagedMode { get => currentState == StateType.Damaged; }
    public bool DeadMode { get => currentState == StateType.Dead; }
    public bool ReloadMode {  get => currentState == StateType.Reload; }

    public void SetIdleMode() => ChangeType(StateType.Idle);
    public void SetEquipMode() => ChangeType(StateType.Equip);
    public void SetActionMode() => ChangeType(StateType.Action);
    public void SetSkillMode() => ChangeType(StateType.Skill);
    public void SetDamagedMode() => ChangeType(StateType.Damaged);
    public void SetDeadMode() => ChangeType(StateType.Dead);
    public void SetReloadMode() => ChangeType(StateType.Reload);

    private void ChangeType(StateType type)
    {
        if(this.currentState == type) return;

        StateType prevtype = this.currentState;
        this.currentState = type;

        OnStateTypeChanged?.Invoke(prevtype, type);
    }
}