using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class AnimationLayerMixer : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private int MovingLayer;
    private int ActionLayer;
    private int SkillLayer;
    private int DamagedLayer;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        MovingLayer = animator.GetLayerIndex("MovingLayer_Generic");
        ActionLayer = animator.GetLayerIndex("ActionLayer_Generic");
    }

    // 레이어 가중치 0~1로 조절
    public void SetAnimationWeight(float w)
    {
        animator.SetLayerWeight(MovingLayer, Mathf.Clamp01(w));
        animator.SetLayerWeight(ActionLayer, Mathf.Clamp01(w));
    }

    // 해당 레이어의 상태로 CrossFade
    public void PlayAnimationState(string state, float dur = 0.1f)
    {
        animator.CrossFade(state, dur, MovingLayer);
        animator.CrossFade(state, dur, ActionLayer);
    }
}