using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(TransformData), true)]
public class TransformDataDrawer : PropertyDrawer
{
    protected static bool foldedOut = true;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.

        EditorGUI.BeginProperty(position, label, property);
        property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, 20f), property.isExpanded, label.text);
        if (property.isExpanded)
        {
            // Calculate rects
            var posRect = new Rect(position.x, position.y + 20f, position.width, position.height);
            var rotRect = new Rect(position.x, position.y + 40f, position.width, position.height);
            var scaleRect = new Rect(position.x, position.y + 60f, position.width, position.height);

            bool largeEnough = (EditorGUIUtility.currentViewWidth > 345);
            if (largeEnough)
            {
                EditorGUI.PropertyField(posRect, property.FindPropertyRelative("position"));
                EditorGUI.PropertyField(rotRect, property.FindPropertyRelative("eulerAngles"));
                EditorGUI.PropertyField(scaleRect, property.FindPropertyRelative("scale"));
            }
            else
            {
                EditorGUI.PropertyField(posRect, property.FindPropertyRelative("position"), new GUIContent(""));
                EditorGUI.PropertyField(rotRect, property.FindPropertyRelative("eulerAngles"), new GUIContent(""));
                EditorGUI.PropertyField(scaleRect, property.FindPropertyRelative("scale"), new GUIContent(""));
            }

            float barWidth = position.width * 0.91f;
            if (largeEnough)
                barWidth = (position.width * 0.56f);

            float xPosition = position.x + (position.width - barWidth);
            TransformData availableData = TransformDataHolder.data;

            if (availableData != null) barWidth = (barWidth / 2f) - 15f;
            var buttonOneRect = new Rect(xPosition, position.y + 80f, barWidth, 20f);
            if (GUI.Button(buttonOneRect, "Reset Data"))
            {
                property.FindPropertyRelative("position").vector3Value = Vector3.zero;
                property.FindPropertyRelative("eulerAngles").vector3Value = Vector3.zero;
                property.FindPropertyRelative("scale").vector3Value = Vector3.one;
            }

            if (availableData != null)
            {
                var buttonTwoRect = new Rect(xPosition + barWidth + 5f, position.y + 80f, barWidth, 20f);
                if (GUI.Button(buttonTwoRect, "Paste Data"))
                {
                    availableData = TransformDataHolder.PasteData();
                    property.FindPropertyRelative("position").vector3Value = availableData.position;
                    property.FindPropertyRelative("eulerAngles").vector3Value = availableData.eulerAngles;
                    property.FindPropertyRelative("scale").vector3Value = availableData.scale;
                }

                var oldColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1f, 0.25f, 0.25f);
                var buttonThreeRect = new Rect(position.width, position.y + 80f, 20f, 20f);
                if (GUI.Button(buttonThreeRect, "X"))
                    availableData = TransformDataHolder.PasteData();
                GUI.backgroundColor = oldColor;
            }
        }
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUI.GetPropertyHeight(property);
        if (property.isExpanded)
            return 120f;
        return height;
    }
}
