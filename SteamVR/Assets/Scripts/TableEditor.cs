#if (UNITY_EDITOR) 
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Table))]
[CanEditMultipleObjects]
public class TableEditor : Editor
{
	SerializedProperty content;
    SerializedProperty lineMaterial;
    SerializedProperty textMaterialRegular;
    SerializedProperty textMaterialBold;
    SerializedProperty padding;
    SerializedProperty lineWidth;
    SerializedProperty verticalAlignment;
    SerializedProperty horizontalAlignment;
    SerializedProperty tableStyle;

    [MenuItem("GameObject/3D Object/Table")]
    public static void CreateTable()
    {
        GameObject gameObject = new GameObject("Table" + EditorUtilities.GetSuffix("Table"));
        Table table = gameObject.AddComponent<Table>();
        table.content = "New Cell|New Cell\nNew Cell|New Cell";
        Selection.activeGameObject = gameObject;
    }

    void OnEnable()
    {
		//lastContent = "";
        content = serializedObject.FindProperty("content");
        lineMaterial = serializedObject.FindProperty("lineMaterial");
        textMaterialRegular = serializedObject.FindProperty("textMaterialRegular");
        textMaterialBold = serializedObject.FindProperty("textMaterialBold");
        padding = serializedObject.FindProperty("padding");
        lineWidth = serializedObject.FindProperty("lineWidth");
        verticalAlignment = serializedObject.FindProperty("verticalAlignment");
        horizontalAlignment = serializedObject.FindProperty("horizontalAlignment");
        tableStyle = serializedObject.FindProperty("tableStyle");
    }

    private static string[][] ParseContent(string content, out int maxCols)
    {
        maxCols = 0;
        string[] rows = content.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        string[][] parsed = new string[rows.Length][];
        for (int i = 0; i < parsed.Length; i++)
        {
            parsed[i] = rows[i].Split('|');
            maxCols = Mathf.Max(maxCols, parsed[i].Length);
        }
        return parsed;
    }


    private void BuildTable(string[][] tableContent, int maxCols,Material textMaterialRegular,Material textMaterialBold,Material lineMaterial)
    {
        HorizontalAlignment horizontal = (HorizontalAlignment)horizontalAlignment.enumValueIndex;
        VerticalAlignment vertical = (VerticalAlignment)verticalAlignment.enumValueIndex;
        TableStyle style = (TableStyle)tableStyle.enumValueIndex;
        Font fontRegular = AssetDatabase.LoadAssetAtPath<Font>("Assets/Text/OpenSans-Regular.ttf");
        Font fontBold = AssetDatabase.LoadAssetAtPath<Font>("Assets/Text/OpenSans-Bold.ttf");

        GameObject parent = ((Table)target).gameObject;
        TextMesh[][] texts = new TextMesh[tableContent.Length][];
        Vector2[][] textExtents = new Vector2[tableContent.Length][];
        float[] widths = new float[maxCols];
        float[] heights = new float[texts.Length];
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i] = new TextMesh[tableContent[i].Length];
            textExtents[i] = new Vector2[tableContent[i].Length];
            for (int j = 0; j < texts[i].Length; j++)
            {
                if (tableContent[i][j] != null)
                {
                    GameObject textObject = new GameObject("Text" + i + "_" + j);
                    textObject.transform.parent = parent.transform;
                    texts[i][j] = textObject.AddComponent<TextMesh>();
                    MeshRenderer renderer = textObject.GetComponent<MeshRenderer>();
                    if(i==0)
                    {
                        renderer.material = textMaterialBold;
                        texts[i][j].font = fontBold;
                    }
                    else { 
                        renderer.material = textMaterialRegular;
                        texts[i][j].font = fontRegular;
                    }
                    texts[i][j].fontSize = 100;                    
                    textObject.transform.localScale = Vector3.one * 0.04f;
                    texts[i][j].text = tableContent[i][j].Replace('~','\n');
                    widths[j] = Mathf.Max(renderer.bounds.extents.x*2.0f/parent.transform.localScale.x, widths[j]);
                    heights[i] = Mathf.Max(renderer.bounds.extents.y * 2.0f / parent.transform.localScale.y, heights[i]);
                    textExtents[i][j] = new Vector2(renderer.bounds.extents.x * 2.0f, renderer.bounds.extents.y * 2.0f);
                }
            }
        }

        

        float y = -lineWidth.floatValue- padding.floatValue;
        for (int i = 0; i < texts.Length; i++)
        {
            float x_ = lineWidth.floatValue + padding.floatValue;
            for (int j = 0; j < texts[i].Length; j++)
            {
                float x_Offset = 0.0f;
                switch(horizontal)
                {
                    case HorizontalAlignment.Center:
                        x_Offset = (widths[j] - textExtents[i][j].x)/2.0f;
                        break;
                    case HorizontalAlignment.Left:
                        x_Offset = 0.0f;
                        break;
                    case HorizontalAlignment.Right:
                        x_Offset = widths[j] - textExtents[i][j].x;
                        break;
                }
                float y_Offset = 0.0f;
                switch(vertical)
                {
                    case VerticalAlignment.Middle:
                        y_Offset = (heights[i] - textExtents[i][j].y) / 2.0f;
                        break;
                    case VerticalAlignment.Bottom:
                        y_Offset = heights[i] - textExtents[i][j].y;
                        break;
                    case VerticalAlignment.Top:
                        y_Offset = 0.0f;
                        break;
                }

                texts[i][j].gameObject.transform.localPosition = new Vector3(x_+x_Offset,y-y_Offset,0.0f);                
                x_ += widths[j] + lineWidth.floatValue +padding.floatValue * 2.0f;
            }
            y -= heights[i] + lineWidth.floatValue+padding.floatValue * 2.0f;
        }

        foreach(TextMesh[] meshes in texts)
        {
            foreach(TextMesh mesh in meshes)
            {
                mesh.transform.localRotation = Quaternion.identity;
            }
        }

        float totalWidth = lineWidth.floatValue;
        for(int i=0;i<maxCols;i++)
        {
            totalWidth += widths[i] + lineWidth.floatValue+padding.floatValue*2.0f;
        }
        y = 0;
        for (int i = 0; i <= texts.Length; i++)
        {
            if(style==TableStyle.AllLines||(style==TableStyle.NoSideLines && i>0)||(style==TableStyle.MinimalLines && (i==1 || i==texts.Length))) { 
                GameObject lineObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lineObject.transform.parent = parent.transform;
                lineObject.name = "hLine";
                lineObject.transform.localScale = new Vector3(totalWidth, lineWidth.floatValue, lineWidth.floatValue);
                lineObject.transform.localPosition = new Vector3(totalWidth/2.0f, y-lineWidth.floatValue / 2.0f, 0);
                lineObject.transform.localRotation = Quaternion.identity;
                lineObject.GetComponent<MeshRenderer>().material = lineMaterial;
            }
            if (i<texts.Length) { 
                y -= heights[i] + lineWidth.floatValue + padding.floatValue * 2.0f;
            }
        }
        float totalHeight = lineWidth.floatValue;
        for(int i=0;i<texts.Length;i++)
        {
            totalHeight += heights[i] + lineWidth.floatValue + padding.floatValue * 2.0f;
        }
        float x = 0.0f;
        for(int i=0;i<=maxCols;i++)
        {
            if (style == TableStyle.AllLines || (style == TableStyle.NoSideLines && i > 0 && i<maxCols))
            {
                GameObject lineObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lineObject.transform.parent = parent.transform;
                lineObject.name = "vLine";
                lineObject.transform.localScale = new Vector3(lineWidth.floatValue, totalHeight, lineWidth.floatValue);
                lineObject.transform.localPosition = new Vector3(x + lineWidth.floatValue / 2.0f, -totalHeight / 2.0f, 0);
                lineObject.transform.localRotation = Quaternion.identity;
                lineObject.GetComponent<MeshRenderer>().material = lineMaterial;
            }
            if (i<maxCols) {
                x += widths[i]+lineWidth.floatValue + padding.floatValue * 2.0f;
            }
        }
    }

    private void DeleteAllChildren()
    {
        GameObject parent = ((Table)target).gameObject;
        foreach (Transform obj in parent.GetComponentsInChildren<Transform>())
        {
            if (!parent.Equals(obj.gameObject))
            {
                Object.DestroyImmediate(obj.gameObject);
            }
        }
    }


    public override void OnInspectorGUI()
    {
        bool change = false;
        serializedObject.Update();
        EditorGUILayout.LabelField("~ = Line break in cell");
        EditorGUILayout.LabelField("| = New column");
        EditorGUILayout.LabelField("Line break = New row");
        string nContent = EditorGUILayout.TextArea(content.stringValue, GUILayout.Height(300));
        if(!nContent.Equals(content.stringValue))
        {
            change = true;
        }
        content.stringValue = nContent;
        Material textMatRegular = (Material)textMaterialRegular.objectReferenceValue;
        if (textMatRegular==null)
        {
            textMatRegular = new Material(Shader.Find("GUI/Depth Text Shader"));
            Texture2D fontTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Text/OpenSans-Regular.ttf");
            textMatRegular.SetTexture("_MainTex", fontTex);
            textMatRegular.renderQueue = 3001;
            textMaterialRegular.objectReferenceValue = textMatRegular;
        }
        textMatRegular.SetColor("_Color", EditorGUILayout.ColorField("Text Color", textMatRegular.GetColor("_Color")));

        Material textMatBold = (Material)textMaterialBold.objectReferenceValue;
        if(textMatBold==null)
        {
            textMatBold = new Material(Shader.Find("GUI/Depth Text Shader"));
            Texture2D fontTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Text/OpenSans-Bold.ttf");
            textMatBold.SetTexture("_MainTex", fontTex);
            textMatBold.renderQueue = 3001;
            textMaterialBold.objectReferenceValue = textMatBold;
        }
        textMatBold.SetColor("_Color", textMatRegular.GetColor("_Color"));

        Material lineMat = (Material)lineMaterial.objectReferenceValue;
        if(lineMat==null)
        {
            lineMat = new Material(Shader.Find("Standard"));
            lineMat.SetFloat("_Glossiness", 0.0f);
            lineMat.EnableKeyword("_EMISSION");
            lineMaterial.objectReferenceValue = lineMat;
        }
        Color lineColor = EditorGUILayout.ColorField("Line Color", lineMat.GetColor("_Color"));
        lineMat.SetColor("_Color", lineColor);
        lineMat.SetColor("_EmissionColor", lineColor * 0.3f);

        float nPadding = EditorGUILayout.FloatField("Padding", padding.floatValue);
        if(nPadding!=padding.floatValue)
        {
            change = true;
        }
        padding.floatValue = (nPadding >= 0.0f ? nPadding : 0.0f);

        float nLineWidth = EditorGUILayout.FloatField("Line Width", lineWidth.floatValue);
        if(nLineWidth!=lineWidth.floatValue)
        {
            change = true;
        }
        lineWidth.floatValue = (nLineWidth >= 0.0f ? nLineWidth : 0.0f);
        

        VerticalAlignment vertical = (VerticalAlignment)verticalAlignment.enumValueIndex;
        vertical = (VerticalAlignment)EditorGUILayout.EnumPopup("Vertical alignment", vertical);
        if (verticalAlignment.enumValueIndex != (int)vertical)
        {
            change = true;
        }
        verticalAlignment.enumValueIndex = (int)vertical;
        HorizontalAlignment horizontal = (HorizontalAlignment)horizontalAlignment.enumValueIndex;
        horizontal = (HorizontalAlignment)EditorGUILayout.EnumPopup("Horizontal alignment", horizontal);
        if (horizontalAlignment.enumValueIndex != (int)horizontal)
        {
            change = true;
        }
        horizontalAlignment.enumValueIndex = (int)horizontal;

        TableStyle style = (TableStyle)tableStyle.enumValueIndex;
        style = (TableStyle)EditorGUILayout.EnumPopup("Table style", style);
        if(tableStyle.enumValueIndex!=(int)style)
        {
            change = true;
        }
        tableStyle.enumValueIndex = (int)style;

		if (change) {
			int maxCols;
			string[][] tableContent = ParseContent (content.stringValue, out maxCols);
			DeleteAllChildren ();
			BuildTable (tableContent, maxCols, textMatRegular,textMatBold, lineMat);
			serializedObject.ApplyModifiedProperties ();
		}
		
    }
}
#endif