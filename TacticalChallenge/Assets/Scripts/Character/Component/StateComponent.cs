using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateComponent : MonoBehaviour
{
    private enum State
    {
        Idle, Skill, Shoot, Reload, Buff, NoBuff
    }

    private State[] state;
    
    public void Init()
    {
        state = new State[2];
        state[0] = State.Idle;
        state[1] = State.NoBuff;
    }



}
