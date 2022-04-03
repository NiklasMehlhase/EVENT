#if (UNITY_EDITOR) 
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Transform), true)]
[CanEditMultipleObjects]
public class TransformPersister : Editor
{
    private Vector3 playPosition;
    private Vector3 playScale;
    private Quaternion playRotation;

    private bool savedAnything;

    // Use this for initialization
    void OnEnable()
    {
        #if UNITY_EDITOR
                EditorApplication.playmodeStateChanged += StateChange;
        #endif
    }

    public void DrawABetterInspector(Transform t)
    {
        // Replicate the standard transform inspector gui        
        EditorGUIUtility.labelWidth = 100;
        EditorGUIUtility.fieldWidth = 45;

        EditorGUI.indentLevel = 0;
        Vector3 position = EditorGUILayout.Vector3Field("Position", t.localPosition);
        Vector3 eulerAngles = EditorGUILayout.Vector3Field("Rotation", t.localEulerAngles);
        Vector3 scale = EditorGUILayout.Vector3Field("Scale", t.localScale);

        EditorGUIUtility.labelWidth = 0;
        EditorGUIUtility.fieldWidth = 0;

        if (GUI.changed)
        {
            Undo.RecordObject(t, "Transform Change");

            t.localPosition = FixIfNaN(position);
            t.localEulerAngles = FixIfNaN(eulerAngles);
            t.localScale = FixIfNaN(scale);
        }
    }

    void StateChange()
    {
        if(!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
        {
            if(this.savedAnything)
            {
                Transform t = (Transform)target;
                Undo.RecordObject(t, "Apply transform");
                t.localScale = this.playScale;
                t.localPosition = this.playPosition;
                t.localRotation = this.playRotation;
                Undo.FlushUndoRecordObjects();
            }
        }
        else if(EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.isPlaying)
        {
            this.savedAnything = false;
        }
    }

    private Vector3 FixIfNaN(Vector3 v)
    {
        if (float.IsNaN(v.x))
        {
            v.x = 0.0f;
        }
        if (float.IsNaN(v.y))
        {
            v.y = 0.0f;
        }
        if (float.IsNaN(v.z))
        {
            v.z = 0.0f;
        }
        return v;
    }

    public override void OnInspectorGUI()
    {
        Transform t = (Transform)target;
        DrawABetterInspector(t);
        if (Application.isEditor && Application.isPlaying)
        {
            if (GUILayout.Button("Save"))
            {
                this.savedAnything = true;
                this.playRotation = t.localRotation;
                this.playPosition = t.localPosition;
                this.playScale = t.localScale;
            }
        }
    }
}
#endif