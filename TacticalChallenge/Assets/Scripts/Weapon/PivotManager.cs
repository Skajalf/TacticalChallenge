using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PivotJsonEntry
{
    public string character;    // ex: "Azusa"
    public string weapon;       // ex: "Azusa_Weapon"
    public Vector3 pos;         // localPosition
    public Vector3 rot;         // localEulerAngles
    public Vector3 scale;       // localScale
}

// JsonUtility�� �ֻ��� �迭�� �ٷ� �Ľ����� ���ϹǷ� wrapper �ʿ�
[Serializable]
internal class PivotJsonWrapper
{
    public PivotJsonEntry[] entries;
}

public class PivotManager : MonoBehaviour
{
    private Dictionary<string, PivotJsonEntry> _map;

    private void Awake()
    {
        LoadJsonDatabase();
    }

    private void LoadJsonDatabase()
    {
        TextAsset ta = Resources.Load<TextAsset>("PivotDatabase");
        if (ta == null)
        {
            Debug.LogError("Resources/PivotDatabase.json �� Ȯ���ϼ���.");
            return;
        }

        // JsonUtility�� �ֻ��� �迭 �Ľ��� �������� �����Ƿ� wrapper�� ����
        string wrapped = "{\"entries\":" + ta.text + "}";
        PivotJsonWrapper wrapper = JsonUtility.FromJson<PivotJsonWrapper>(wrapped);

        // Dictionary �ʱ�ȭ
        _map = new Dictionary<string, PivotJsonEntry>(wrapper.entries.Length);
        foreach (var entry in wrapper.entries)
        {
            string key = MakeKey(entry.character, entry.weapon);
            _map[key] = entry;
        }
    }

    public bool TryGetEntry(string character, string weapon, out PivotJsonEntry entry)
    {
        return _map.TryGetValue(MakeKey(character, weapon), out entry);
    }

    private string MakeKey(string character, string weapon)
    {
        return character + "_" + weapon;
    }
}
