using System.Collections;
using UnityEngine;
using Michsky.MUIP;

public class UI_HPBar : MonoBehaviour
{
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private Player targetPlayer;

    [SerializeField] private bool smooth = true;
    [SerializeField] private float smoothDuration = 0.12f;

    private Coroutine anim;

    private void Start()
    {
        if (targetPlayer == null)
        {
            targetPlayer = FindObjectOfType<Player>();
        }

        if (targetPlayer != null) SetTarget(targetPlayer);
    }

    public void SetTarget(Player p)
    {
        if (p == null) return;

        if (targetPlayer != null) targetPlayer.OnHPChanged -= HandleHPChanged;

        targetPlayer = p;

        if (progressBar != null)
        {
            progressBar.maxValue = targetPlayer.MaxHP;
            progressBar.SetValue(targetPlayer.HP);
        }

        targetPlayer.OnHPChanged += HandleHPChanged;
    }

    private void HandleHPChanged(float current, float max)
    {
        if (progressBar == null) return;

        if (!smooth)
        {
            progressBar.SetValue(current);
            return;
        }

        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(AnimateTo(current, smoothDuration));
    }

    private IEnumerator AnimateTo(float targetValue, float duration)
    {
        float start = progressBar.currentPercent;
        float t = 0f;

        if (duration <= 0f)
        {
            progressBar.SetValue(targetValue);
            anim = null;
            yield break;
        }

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float v = Mathf.Lerp(start, targetValue, Mathf.Clamp01(t / duration));
            progressBar.SetValue(v);
            yield return null;
        }

        progressBar.SetValue(targetValue);
        anim = null;
    }

    private void OnDestroy()
    {
        if (targetPlayer != null) targetPlayer.OnHPChanged -= HandleHPChanged;
    }
}