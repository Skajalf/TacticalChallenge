using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

[CreateAssetMenu]
public class Stat : ScriptableObject
{
    public delegate void ValueChangedHandler(Stat stat, float currentValue, float prevValue);

    [SerializeField] public string ID = string.Empty;

    [SerializeField] private bool isPercentType;
    [SerializeField] private float maxValue;
    [SerializeField] private float minValue;
    [SerializeField] private float defaultValue;

    private Dictionary<object, Dictionary<object, float>> adjustValuesByKey = new();

    public bool IsPercentType => isPercentType;
    public float MaxValue
    {
        get => maxValue;
        set => maxValue = value;
    }
    public float MinValue
    {
        get => minValue;
        set => minValue = value;
    }
    public float DefaultValue
    {
        get => defaultValue;
        set
        {
            float prevValue = value;
            defaultValue = Mathf.Clamp(value, MinValue, MaxValue);
            TryInvokeValueChangedEvent(Value, prevValue);
        }
    }

    // AdjustValue는 적용된 버프 및 디버프 능력치의 총합임.
    public float AdjustValue { get; private set; }
    public float Value => Mathf.Clamp(defaultValue + AdjustValue, MinValue, MaxValue);
    public bool IsMax => Mathf.Approximately(Value, maxValue);
    public bool IsMin => Mathf.Approximately(Value, minValue);

    public event ValueChangedHandler onValueChanged;
    public event ValueChangedHandler onValueMax;
    public event ValueChangedHandler onValueMin;

    private void TryInvokeValueChangedEvent(float currentValue, float prevValue)
    {
        if (!Mathf.Approximately(currentValue, prevValue))
        {
            onValueChanged?.Invoke(this, currentValue, prevValue);
            if (Mathf.Approximately(currentValue, MaxValue))
                onValueMax?.Invoke(this, MaxValue, prevValue);
            else if (Mathf.Approximately(currentValue, MinValue))
                onValueMin?.Invoke(this, MinValue, prevValue);
        }
    }


    public void SetAdjustValue(object key, object subkey, float value)
    {
        if (!adjustValuesByKey.ContainsKey(key))
            adjustValuesByKey[key] = new Dictionary<object, float>();
        else
            AdjustValue -= adjustValuesByKey[key][subkey];

        float prevValue = Value;
        adjustValuesByKey[key][subkey] = value;
        AdjustValue += value;

        TryInvokeValueChangedEvent(Value, prevValue);
    }

    public void SetAdjustValue(object key, float value)
        => SetAdjustValue(key, string.Empty, value);

    public float GetAdjustValue(object key)
        => adjustValuesByKey.TryGetValue(key, out var adjustValuesBySubkey) ? adjustValuesBySubkey.Sum(x => x.Value) : 0f;

    public float GetAdjustValue(object key, object subkey)
    {
        if (adjustValuesByKey.TryGetValue(key, out var adjustValuesBySubkey))
        {
            if (adjustValuesBySubkey.TryGetValue(subkey, out var value))
                return value;
        }
        return 0f;
    }

    public bool RemoveAdjustValue(object key)
    {
        if (adjustValuesByKey.TryGetValue(key, out var adjustValuesBySubkey))
        {
            float prevValue = Value;
            AdjustValue -= adjustValuesBySubkey.Values.Sum();
            adjustValuesByKey.Remove(key);

            TryInvokeValueChangedEvent(Value, prevValue);
            return true;
        }
        return false;
    }

    public bool RemoveAdjustValue(object key, object subkey)
    {
        if (adjustValuesByKey.TryGetValue(key, out var adjustValuesBySubkey))
        {
            if (adjustValuesBySubkey.Remove(subkey, out var value))
            {
                var prevValue = Value;
                AdjustValue -= value;
                TryInvokeValueChangedEvent(Value, prevValue);
                return true;
            }
        }
        return false;
    }

    public bool ContainsAdjustValue(object key)
        => adjustValuesByKey.ContainsKey(key);

    public bool ContainsAdjustValue(object key, object subkey)
        => adjustValuesByKey.TryGetValue(key, out var adjustValuesBySubkey) ? adjustValuesBySubkey.ContainsKey(subkey) : false;

}