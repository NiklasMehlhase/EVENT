#if (UNITY_EDITOR) 
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(TextMesh),true)]
[CanEditMultipleObjects]
public class TextEditor : Editor {
    
    void OnEnable()
    {
        TextMesh textMesh = (TextMesh)target;
        if (textMesh.fontSize<100) { 
            textMesh.fontSize = 100;
            textMesh.gameObject.transform.localScale = Vector3.one * 0.04f;
            FixDepth(ref textMesh);
        }
    }

    private void FixDepth(ref TextMesh textMesh)
    {
        MeshRenderer renderer = textMesh.gameObject.GetComponent<MeshRenderer>();
        Material nMaterial = new Material(Shader.Find("GUI/Depth Text Shader"));
        Texture2D fontTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Text/OpenSans-Regular.ttf");
        textMesh.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Text/OpenSans-Regular.ttf");
        nMaterial.SetTexture("_MainTex", fontTex);
        nMaterial.renderQueue = 3001;
        renderer.material = nMaterial;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TextMesh textMesh = (TextMesh)target;
        bool isBold = !textMesh.font.name.Equals("OpenSans-Regular");
        bool nIsBold = EditorGUILayout.Toggle("Bold", isBold);
        if(nIsBold!=isBold)
        {
            Undo.RecordObject(textMesh, "Change text-boldness");
            if(nIsBold)
            {
				Texture2D fontTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Text/OpenSans-Bold.ttf");
				textMesh.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Text/OpenSans-Bold.ttf");
                textMesh.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", fontTex);
            }
            else
            {
				Texture2D fontTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Text/OpenSans-Regular.ttf");
				textMesh.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Text/OpenSans-Regular.ttf");
                textMesh.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", fontTex);
            }
            Undo.FlushUndoRecordObjects();
        }

        if(GUILayout.Button("Fix depth"))
        {
            Undo.RecordObject(textMesh, "Fix depth issues");
            FixDepth(ref textMesh);
            Undo.FlushUndoRecordObjects();
        }

    }
}
#endif