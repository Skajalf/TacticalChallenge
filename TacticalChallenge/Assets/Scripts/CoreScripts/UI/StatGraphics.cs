using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class StatGraphics
{
    public enum StatType { 체력, 공격력, 방어력, 기동력, AP, 사거리}

    public int[] statValues = new int[6];
    public int maxValue = 100;

    public event System.Action OnStatsChanged;
    public void SetStatValue(StatType type, int value)
    {
        statValues[(int)type] = Mathf.Clamp(value, 0, maxValue);
        OnStatsChanged?.Invoke();
    }
}