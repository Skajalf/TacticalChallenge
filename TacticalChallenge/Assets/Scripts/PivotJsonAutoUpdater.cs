using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

public class PivotJsonAutoUpdater : EditorWindow
{
    private WeaponBase selectedWeapon;
    private string jsonPath = "Assets/Resources/PivotDatabase.json";

    [MenuItem("Tools/Pivot JSON Updater")]
    public static void OpenWindow()
    {
        GetWindow<PivotJsonAutoUpdater>("Pivot JSON Updater");
    }

    private void OnGUI()
    {
        GUILayout.Label("선택한 무기로 Pivot JSON 갱신", EditorStyles.boldLabel);
        selectedWeapon = EditorGUILayout.ObjectField("WeaponBase", selectedWeapon, typeof(WeaponBase), true) as WeaponBase;
        jsonPath = EditorGUILayout.TextField("JSON Path", jsonPath);

        if (selectedWeapon != null && GUILayout.Button("Update Pivot Entry"))
        {
            UpdatePivotEntry();
        }
    }

    [Serializable]
    public class PivotJsonEntry
    {
        public string character;
        public string weapon;
        public Vector3 pos;
        public Vector3 rot;
        public Vector3 scale;
    }

    private void UpdatePivotEntry()
    {
        List<PivotJsonEntry> entries = new List<PivotJsonEntry>();

        if (File.Exists(jsonPath))
        {
            string raw = File.ReadAllText(jsonPath);
            entries = JsonUtilityWrapper.FromJson<PivotJsonEntry>(raw);
        }

        string charName = selectedWeapon.transform.root.name;
        string weaponName = selectedWeapon.name;

        int idx = entries.FindIndex(e => e.character == charName && e.weapon == weaponName);
        PivotJsonEntry updated = new PivotJsonEntry
        {
            character = charName,
            weapon = weaponName,
            pos = selectedWeapon.transform.localPosition,
            rot = selectedWeapon.transform.localEulerAngles,
            scale = selectedWeapon.transform.localScale
        };

        if (idx >= 0)
            entries[idx] = updated;
        else
            entries.Add(updated);

        string json = JsonUtilityWrapper.ToJson(entries);
        File.WriteAllText(jsonPath, json);
        AssetDatabase.Refresh();
        Debug.Log($"[Pivot JSON] {charName}/{weaponName} 덮어쓰기 완료");
    }

    public static class JsonUtilityWrapper
    {
        public static List<T> FromJson<T>(string json)
        {
            string wrapped = "{\"items\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrapped);
            return wrapper.items;
        }

        public static string ToJson<T>(List<T> list)
        {
            Wrapper<T> wrapper = new Wrapper<T> { items = list };
            string raw = JsonUtility.ToJson(wrapper, true);
            int start = raw.IndexOf('[');
            int end = raw.LastIndexOf(']');
            return raw.Substring(start, end - start + 1);
        }

        [Serializable]
        private class Wrapper<T>
        {
            public List<T> items;
        }
    }
}
