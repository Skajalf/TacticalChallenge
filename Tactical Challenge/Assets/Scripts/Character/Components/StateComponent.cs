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

    public bool isIdleMode { get => currentState == StateType.Idle; }
    public bool isEquipMode { get => currentState == StateType.Equip; }
    public bool isSkillMode { get => currentState == StateType.Skill; }
    public bool isReloadMode {  get => currentState == StateType.Reload; }
    public bool isDamagedMode { get => currentState == StateType.Damaged; }
    public bool isDeadMode { get => currentState == StateType.Dead; }

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
    /// �⺻������ True�� ��ȯ�ϴ� �޼���. ��ų���¿� �������� ���� False�� ��ȯ�Ѵ�.
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


    // ��ų�� ����ϱ� ���� State�� ���� StateType.Skill�� �ٲٰ� ��. => CanDoSomething�� �̶� �˻縦 �Ѵٸ�? => False;
}