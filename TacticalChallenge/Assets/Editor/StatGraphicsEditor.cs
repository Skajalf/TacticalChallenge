using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StatGraphics))]
public class StatGraphicsEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect maxValRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        SerializedProperty maxValueProp = property.FindPropertyRelative("maxValue");
        EditorGUI.PropertyField(maxValRect, maxValueProp);

        SerializedProperty statValuesProp = property.FindPropertyRelative("statValues");

        Rect arrayStartRect = new Rect(position.x, maxValRect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);

        for (int i = 0; i < statValuesProp.arraySize; i++)
        {
            Rect elementRect = new Rect(arrayStartRect.x, arrayStartRect.y + (i * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing)), arrayStartRect.width, EditorGUIUtility.singleLineHeight);

            string statName = ((StatGraphics.StatType)i).ToString();

            EditorGUI.PropertyField(elementRect, statValuesProp.GetArrayElementAtIndex(i), new GUIContent(statName));
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 7 + EditorGUIUtility.standardVerticalSpacing * 7;
    }
}