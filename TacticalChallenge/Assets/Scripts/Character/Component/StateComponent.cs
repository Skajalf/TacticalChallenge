using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateComponent : MonoBehaviour
{
    private enum StateLayer
    {
        Animation, Buff, 
    }

    private enum State
    {
        Idle, Skill, Shoot, Reload, Buff, NoBuff
    }

    private Dictionary<StateLayer, State> state;
    
    public void Init()
    {
        state.Add(StateLayer.Animation, State.Idle);
        state.Add(StateLayer.Buff, State.NoBuff);
    }



}
