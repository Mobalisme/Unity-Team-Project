#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneNameAttribute))]
public class SceneNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        // 프로젝트 전체에서 Scene 에셋 검색
        var guids = AssetDatabase.FindAssets("t:Scene");
        var scenes = guids
            .Select(g => AssetDatabase.GUIDToAssetPath(g))
            .Select(p => Path.GetFileNameWithoutExtension(p))
            .Distinct()
            .OrderBy(n => n)
            .ToArray();

        var options = new string[scenes.Length + 1];
        options[0] = "(None)";
        for (int i = 0; i < scenes.Length; i++) options[i + 1] = scenes[i];

        int currentIndex = 0;
        if (!string.IsNullOrEmpty(property.stringValue))
        {
            int found = System.Array.IndexOf(scenes, property.stringValue);
            currentIndex = (found >= 0) ? (found + 1) : 0;
        }

        EditorGUI.BeginProperty(position, label, property);
        int nextIndex = EditorGUI.Popup(position, label.text, currentIndex, options);
        property.stringValue = (nextIndex <= 0) ? "" : options[nextIndex];
        EditorGUI.EndProperty();
    }
}
#endif
