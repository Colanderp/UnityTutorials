using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;

#if UNITY_EDITOR

[CanEditMultipleObjects]
[CustomEditor(typeof(Transform), true)]
public class TransformInspector : Editor
{
    //Unity's built-in editor
    Editor defaultEditor;
    Transform transform;

    void OnEnable()
    {
        //When this inspector is created, also create the built-in inspector
        defaultEditor = CreateEditor(targets, Type.GetType("UnityEditor.TransformInspector, UnityEditor"));
        transform = target as Transform;
    }

    void OnDisable()
    {
        //When OnDisable is called, the default editor we created should be destroyed to avoid memory leakage.
        //Also, make sure to call any required methods like OnDisable
        MethodInfo disableMethod = defaultEditor.GetType().GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (disableMethod != null)
            disableMethod.Invoke(defaultEditor, null);
        DestroyImmediate(defaultEditor);
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Local Space", EditorStyles.boldLabel);
        if (GUILayout.Button("Copy Data", GUILayout.MinWidth(42)))
            TransformDataHolder.CopyData(new TransformData(transform));
        EditorGUILayout.EndHorizontal();
        defaultEditor.OnInspectorGUI();

        //Show World Space Transform
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("World Space", EditorStyles.boldLabel);

        GUI.enabled = false;
        Vector3 localPosition = transform.localPosition;
        transform.localPosition = transform.position;

        Quaternion localRotation = transform.localRotation;
        transform.localRotation = transform.rotation;

        Vector3 localScale = transform.localScale;
        transform.localScale = transform.lossyScale;

        defaultEditor.OnInspectorGUI();
        transform.localPosition = localPosition;
        transform.localRotation = localRotation;
        transform.localScale = localScale;
        GUI.enabled = true;
    }

}

#endif