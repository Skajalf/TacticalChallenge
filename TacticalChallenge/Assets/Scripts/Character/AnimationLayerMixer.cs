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

    // ���̾� ����ġ 0~1�� ����
    public void SetAnimationWeight(float w)
    {
        animator.SetLayerWeight(MovingLayer, Mathf.Clamp01(w));
        animator.SetLayerWeight(ActionLayer, Mathf.Clamp01(w));
    }

    // �ش� ���̾��� ���·� CrossFade
    public void PlayAnimationState(string state, float dur = 0.1f)
    {
        animator.CrossFade(state, dur, MovingLayer);
        animator.CrossFade(state, dur, ActionLayer);
    }
}