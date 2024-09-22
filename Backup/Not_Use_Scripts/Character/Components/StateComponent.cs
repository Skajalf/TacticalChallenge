using System;
using UnityEngine;


public enum StateType
{
    Idle, Equip, Skill, Damaged, Dead, Reload
}

public class StateComponent : MonoBehaviour
{
    private StateType currentState = StateType.Idle;

    public StateType CurrentState { private set { currentState = value; }  get { return currentState; } }

    public event Action<StateType, StateType> OnStateTypeChanged;

    public bool IdleMode { get => currentState == StateType.Idle; }
    public bool EquipMode { get => currentState == StateType.Equip; }
    public bool SkillMode { get => currentState == StateType.Skill; }
    public bool ReloadMode {  get => currentState == StateType.Reload; }
    public bool DamagedMode { get => currentState == StateType.Damaged; }
    public bool DeadMode { get => currentState == StateType.Dead; }

    public void SetIdleMode() => ChangeType(StateType.Idle);
    public void SetEquipMode() => ChangeType(StateType.Equip);
    public void SetSkillMode() => ChangeType(StateType.Skill);
    public void SetReloadMode() => ChangeType(StateType.Reload);
    public void SetDamagedMode() => ChangeType(StateType.Damaged);
    public void SetDeadMode() => ChangeType(StateType.Dead);

    private void ChangeType(StateType type)
    {
        if(this.currentState == type) return;

        StateType prevtype = this.currentState;
        this.currentState = type;

        OnStateTypeChanged?.Invoke(prevtype, type);
    }

    /// <summary>
    /// 기본적으로 True를 반환하는 메서드. 스킬상태와 재장전할 때만 False를 반환한다.
    /// </summary>
    /// <returns>boolean</returns>
    public bool CanDoSomething()
    {
        if(currentState == StateType.Idle)
            return true;

        if(currentState == StateType.Skill ||  currentState == StateType.Reload)
            return false;

        return true; 
    }

}