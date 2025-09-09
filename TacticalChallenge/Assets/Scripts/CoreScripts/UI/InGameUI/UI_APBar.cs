using System.Collections;
using UnityEngine;
using Michsky.MUIP;

public class UI_APBar : MonoBehaviour
{
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private Player targetPlayer;

    private void Start()
    {
        if (progressBar == null) Debug.LogWarning("ProgressBar 참조 없음 (UI_APBar).");
        if (targetPlayer == null)
        {
            targetPlayer = FindObjectOfType<Player>();
            if (targetPlayer == null) Debug.LogWarning("Player를 찾을 수 없습니다.");
        }

        if (targetPlayer != null) SetTarget(targetPlayer);
    }

    public void SetTarget(Player p)
    {
        if (p == null) return;

        if (targetPlayer != null) targetPlayer.OnAPChanged -= HandleAPChanged;

        targetPlayer = p;

        if (progressBar != null)
        {
            progressBar.maxValue = targetPlayer.MaxAP;
            progressBar.SetValue(targetPlayer.AP);
        }

        targetPlayer.OnAPChanged += HandleAPChanged;
    }

    private void HandleAPChanged(float current, float max)
    {
        if (progressBar == null) return;

        progressBar.SetValue(current);
    }

    private void OnDestroy()
    {
        if (targetPlayer != null) targetPlayer.OnAPChanged -= HandleAPChanged;
    }
}