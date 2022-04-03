#if (UNITY_EDITOR) 
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(Symbol))]
[CanEditMultipleObjects]
public class SymbolEditor : Editor
{
    SerializedProperty type;
    SerializedProperty length;
    SerializedProperty color;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    Material mat;

    


    [MenuItem("GameObject/3D Object/Arrow")]
    public static void CreateArrow()
    {
        GameObject gameObject = new GameObject("Arrow" + EditorUtilities.GetSuffix("Arrow"));
        Symbol symbol = gameObject.AddComponent<Symbol>();
        symbol.type = SymbolType.Arrow;
        symbol.length = 5.0f;
        gameObject.transform.localScale = Vector3.one * 0.1f;
        Selection.activeGameObject = gameObject;
    }

    [MenuItem("GameObject/3D Object/Curly Bracket")]
    public static void CreateCurlyBracket()
    {
        GameObject gameObject = new GameObject("Curly_Bracket" + EditorUtilities.GetSuffix("Curly_Bracket"));
        gameObject.AddComponent<Symbol>().type = SymbolType.CurlyBracket;
        gameObject.transform.localScale = Vector3.one * 0.1f;
        Selection.activeGameObject = gameObject;
    }

    void OnEnable()
    {
        type = serializedObject.FindProperty("type");
        length = serializedObject.FindProperty("length");
        color = serializedObject.FindProperty("color");

        EnsureRenderer();
    }

    private void EnsureRenderer()
    {
        GameObject gameObject = ((Symbol)target).gameObject;
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }
        }
        if (meshFilter == null)
        {
            meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }
        }
    }


    private Mesh GetArrowMesh(float length)
    {
        Mesh arrowMesh = new Mesh();

        Vector3[] vertices = new Vector3[34];
        //Top vertices
        vertices[0] = new Vector3(0, 0.5f, 0);
        vertices[1] = new Vector3(1.5f, 0.5f, 3);
        vertices[2] = new Vector3(-1.5f, 0.5f, 3);
        vertices[3] = new Vector3(0.5f, 0.5f, 3);
        vertices[4] = new Vector3(-0.5f, 0.5f, 3);
        vertices[5] = new Vector3(0.5f, 0.5f, 3 + length);
        vertices[6] = new Vector3(-0.5f, 0.5f, 3 + length);
        //Bottom vertices
        vertices[7] = new Vector3(0, -0.5f, 0);
        vertices[8] = new Vector3(1.5f, -0.5f, 3);
        vertices[9] = new Vector3(-1.5f, -0.5f, 3);
        vertices[10] = new Vector3(0.5f, -0.5f, 3);
        vertices[11] = new Vector3(-0.5f, -0.5f, 3);
        vertices[12] = new Vector3(0.5f, -0.5f, 3 + length);
        vertices[13] = new Vector3(-0.5f, -0.5f, 3 + length);
        //Left vertices
        vertices[14] = new Vector3(0, 0.5f, 0);
        vertices[15] = new Vector3(1.5f, 0.5f, 3);
        vertices[16] = new Vector3(0.5f, 0.5f, 3);
        vertices[17] = new Vector3(0.5f, 0.5f, 3 + length);
        vertices[18] = new Vector3(0, -0.5f, 0);
        vertices[19] = new Vector3(1.5f, -0.5f, 3);
        vertices[20] = new Vector3(0.5f, -0.5f, 3);
        vertices[21] = new Vector3(0.5f, -0.5f, 3 + length);
        //Right vertices
        vertices[22] = new Vector3(0, 0.5f, 0);
        vertices[23] = new Vector3(-1.5f, 0.5f, 3);
        vertices[24] = new Vector3(-0.5f, 0.5f, 3);
        vertices[25] = new Vector3(-0.5f, 0.5f, 3 + length);
        vertices[26] = new Vector3(0, -0.5f, 0);
        vertices[27] = new Vector3(-1.5f, -0.5f, 3);
        vertices[28] = new Vector3(-0.5f, -0.5f, 3);
        vertices[29] = new Vector3(-0.5f, -0.5f, 3 + length);
        //Back vertices
        vertices[30] = new Vector3(0.5f, 0.5f, 3 + length);
        vertices[31] = new Vector3(-0.5f, 0.5f, 3 + length);
        vertices[32] = new Vector3(0.5f, -0.5f, 3 + length);
        vertices[33] = new Vector3(-0.5f, -0.5f, 3 + length);
        arrowMesh.vertices = vertices;

        int[] triangles = {3,1,0  ,4,3,0  ,2,4,0  ,6,5,3  ,6,3,4 //Top
                          ,7,8,10  ,7,10,11  ,7,11,9  ,10,12,13  ,11,10,13 //Bottom
                          ,15,19,18  ,15,18,14,  15,16,20,  15,20,19,  16,17,21,  16,21,20 //Left
                          ,26,27,23  ,22,26,23,  28,24,23,  27,28,23,  29,25,24,  28,29,24 //Right
                          ,30,31,33  ,30,33,32 //Back
        };

        arrowMesh.triangles = triangles;
        arrowMesh.RecalculateNormals();
        arrowMesh.RecalculateBounds();
        return arrowMesh;
    }


    private static void AddCircleBorder(ref List<Vector3> vertices, ref List<int> triangles, float startDegree, float endDegree, Vector3 center, float radius, float height, bool outer)
    {
        float start = startDegree / 360.0f * 2.0f * Mathf.PI;
        float end = endDegree / 360.0f * 2.0f * Mathf.PI;
        const float epsilon = Mathf.PI / 180.0f;
        Vector3 nVertexStartTop = center + new Vector3(Mathf.Sin(start) * radius, height, Mathf.Cos(start) * radius);
        Vector3 nVertexStartBot = center + new Vector3(Mathf.Sin(start) * radius, -height, Mathf.Cos(start) * radius);

        vertices.Add(nVertexStartTop);
        vertices.Add(nVertexStartBot);

        for (float v = start + epsilon; v <= end - epsilon; v += epsilon)
        {
            Vector3 nVertexTop = center + new Vector3(Mathf.Sin(v) * radius, height, Mathf.Cos(v) * radius);
            Vector3 nVertexBot = center + new Vector3(Mathf.Sin(v) * radius, -height, Mathf.Cos(v) * radius);
            vertices.Add(nVertexTop);
            vertices.Add(nVertexBot);
            if (outer)
            {
                triangles.AddRange(new int[] { vertices.Count - 4, vertices.Count - 3, vertices.Count - 1, vertices.Count - 4, vertices.Count - 1, vertices.Count - 2 });
            }
            else
            {
                triangles.AddRange(new int[] { vertices.Count - 1, vertices.Count - 3, vertices.Count - 4, vertices.Count - 2, vertices.Count - 1, vertices.Count - 4 });
            }
        }

        Vector3 nVertexEndTop = center + new Vector3(Mathf.Sin(end) * radius, height, Mathf.Cos(end) * radius);
        Vector3 nVertexEndBot = center + new Vector3(Mathf.Sin(end) * radius, -height, Mathf.Cos(end) * radius);
        vertices.Add(nVertexEndTop);
        vertices.Add(nVertexEndBot);
        if (outer)
        {
            triangles.AddRange(new int[] { vertices.Count - 4, vertices.Count - 3, vertices.Count - 1, vertices.Count - 4, vertices.Count - 1, vertices.Count - 2 });
        }
        else
        {
            triangles.AddRange(new int[] { vertices.Count - 1, vertices.Count - 3, vertices.Count - 4, vertices.Count - 2, vertices.Count - 1, vertices.Count - 4 });
        }
    }


    private static void AddOuterCircle(ref List<Vector3> vertices, ref List<int> triangles, float startDegree, float endDegree, Vector3 center, float radius)
    {
        float start = startDegree / 360.0f * 2.0f * Mathf.PI;
        float end = endDegree / 360.0f * 2.0f * Mathf.PI;
        const float epsilon = Mathf.PI / 180.0f;
        vertices.Add(center);
        Vector3 nVertexStart = center + new Vector3(Mathf.Sin(start) * radius, 0.0f, Mathf.Cos(start) * radius);
        int index = 1;
        vertices.Add(nVertexStart);
        for (float v = start + epsilon; v <= end - epsilon; v += epsilon)
        {
            Vector3 nVertex = center + new Vector3(Mathf.Sin(v) * radius, 0.0f, Mathf.Cos(v) * radius);
            vertices.Add(nVertex);
            triangles.AddRange(new int[] { vertices.Count - index - 2, vertices.Count - 2, vertices.Count - 1 });
            index++;
        }
        Vector3 nVertexEnd = center + new Vector3(Mathf.Sin(end) * radius, 0.0f, Mathf.Cos(end) * radius);
        vertices.Add(nVertexEnd);
        triangles.AddRange(new int[] { vertices.Count - index - 2, vertices.Count - 2, vertices.Count - 1 });
    }

    private static void AddInnerCircle(ref List<Vector3> vertices, ref List<int> triangles, float startDegree, float endDegree, Vector3 center, Vector3 innerCenter, float radius)
    {
        float start = startDegree / 360.0f * 2.0f * Mathf.PI;
        float end = endDegree / 360.0f * 2.0f * Mathf.PI;
        const float epsilon = Mathf.PI / 180.0f;
        
        vertices.Add(innerCenter);
        Vector3 nVertexStart = center + new Vector3(Mathf.Sin(start) * radius, 0.0f, Mathf.Cos(start) * radius);
        int index = 1;
        vertices.Add(nVertexStart);
        for (float v = start + epsilon; v <= end - epsilon; v += epsilon)
        {
            Vector3 nVertex = center + new Vector3(Mathf.Sin(v) * radius, 0.0f, Mathf.Cos(v) * radius);
            vertices.Add(nVertex);
            triangles.AddRange(new int[] { vertices.Count - 1, vertices.Count - 2, vertices.Count - index - 2 });
            index++;
        }
        Vector3 nVertexEnd = center + new Vector3(Mathf.Sin(end) * radius, 0.0f, Mathf.Cos(end) * radius);
        vertices.Add(nVertexEnd);
        triangles.AddRange(new int[] { vertices.Count - index - 2, vertices.Count - 2, vertices.Count - 1 });
    }


    private static void mirrorOnXZ(ref List<Vector3> vertices, ref List<int> triangles)
    {
        int vertCount = vertices.Count;
        for (int i = 0; i < vertCount; i++)
        {
            Vector3 mirroredVertex = vertices[i];
            mirroredVertex.y = -mirroredVertex.y;
            vertices.Add(mirroredVertex);
        }
        int triCount = triangles.Count / 3;
        for (int i = 0; i < triCount; i++)
        {
            triangles.AddRange(new int[] { triangles[i * 3 + 2] + vertCount, triangles[i * 3 + 1] + vertCount, triangles[i * 3] + vertCount });
        }
    }


    private Mesh GetCurlyBracketMesh(float length)
    {
        Mesh bracketMesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        //Top Rectangles
        vertices.AddRange(Enumerable.Repeat(Vector3.zero, 20));
        vertices[0] = new Vector3(-0.5f, 0.5f, 0);
        vertices[1] = new Vector3(0.5f, 0.5f, 0);
        vertices[2] = new Vector3(-0.5f, 0.5f, 1);
        vertices[3] = new Vector3(0.5f, 0.5f, 1);
        triangles.AddRange(new int[] { 1, 0, 2, 1, 2, 3 });

        vertices[4] = new Vector3(1, 0.5f, 1);
        vertices[5] = new Vector3(2 + length, 0.5f, 1);
        vertices[6] = new Vector3(1, 0.5f, 2);
        vertices[7] = new Vector3(2 + length, 0.5f, 2);
        triangles.AddRange(new int[] { 5, 4, 6, 5, 6, 7 });

        vertices[8] = new Vector3(-1, 0.5f, 1);
        vertices[9] = new Vector3(-2 - length, 0.5f, 1);
        vertices[10] = new Vector3(-1, 0.5f, 2);
        vertices[11] = new Vector3(-2 - length, 0.5f, 2);
        triangles.AddRange(new int[] { 10, 8, 9, 11, 10, 9 });

        vertices[12] = new Vector3(2.5f + length, 0.5f, 2);
        vertices[13] = new Vector3(3 + length, 0.5f, 2);
        vertices[14] = new Vector3(2.5f + length, 0.5f, 3);
        vertices[15] = new Vector3(3 + length, 0.5f, 3);
        triangles.AddRange(new int[] { 13, 12, 14, 13, 14, 15 });

        vertices[16] = new Vector3(-2.5f - length, 0.5f, 2);
        vertices[17] = new Vector3(-3 - length, 0.5f, 2);
        vertices[18] = new Vector3(-2.5f - length, 0.5f, 3);
        vertices[19] = new Vector3(-3 - length, 0.5f, 3);
        triangles.AddRange(new int[] { 18, 16, 17, 19, 18, 17 });

        //Top Curves        
        AddOuterCircle(ref vertices, ref triangles, 270.0f, 360.0f, new Vector3(1, 0.5f, 1), 1.0f);
        AddOuterCircle(ref vertices, ref triangles, 0.0f, 90.0f, new Vector3(-1, 0.5f, 1), 1.0f);
        AddOuterCircle(ref vertices, ref triangles, 90.0f, 180.0f, new Vector3(2 + length, 0.5f, 2), 1.0f);
        AddOuterCircle(ref vertices, ref triangles, 180.0f, 270.0f, new Vector3(-2 - length, 0.5f, 2), 1.0f);
        AddInnerCircle(ref vertices, ref triangles, 270.0f, 360.0f, new Vector3(1.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 1), 1.0f);
        AddInnerCircle(ref vertices, ref triangles, 0.0f, 90.0f, new Vector3(-1.5f, 0.5f, 0), new Vector3(-0.5f, 0.5f, 1), 1.0f);
        AddInnerCircle(ref vertices, ref triangles, 90.0f, 180.0f, new Vector3(1.5f + length, 0.5f, 3), new Vector3(2.5f + length, 0.5f, 2), 1.0f);
        AddInnerCircle(ref vertices, ref triangles, 180.0f, 270.0f, new Vector3(-1.5f - length, 0.5f, 3), new Vector3(-2.5f - length, 0.5f, 2), 1.0f);

        mirrorOnXZ(ref vertices, ref triangles);

        //Side Rectangles
        int index = vertices.Count;
        vertices.AddRange(Enumerable.Repeat(Vector3.zero, 36));
        vertices[0 + index] = new Vector3(-0.5f, 0.5f, 0);
        vertices[1 + index] = new Vector3(0.5f, 0.5f, 0);
        vertices[2 + index] = new Vector3(-0.5f, -0.5f, 0);
        vertices[3 + index] = new Vector3(0.5f, -0.5f, 0);
        triangles.AddRange(new int[] { 2 + index, 0 + index, 1 + index, 3 + index, 2 + index, 1 + index });

        vertices[4 + index] = new Vector3(1, 0.5f, 1);
        vertices[5 + index] = new Vector3(2 + length, 0.5f, 1);
        vertices[6 + index] = new Vector3(1, -0.5f, 1);
        vertices[7 + index] = new Vector3(2 + length, -0.5f, 1);
        triangles.AddRange(new int[] { 6 + index, 4 + index, 5 + index, 7 + index, 6 + index, 5 + index });

        vertices[8 + index] = new Vector3(1, 0.5f, 2);
        vertices[9 + index] = new Vector3(2 + length, 0.5f, 2);
        vertices[10 + index] = new Vector3(1, -0.5f, 2);
        vertices[11 + index] = new Vector3(2 + length, -0.5f, 2);
        triangles.AddRange(new int[] { 9 + index, 8 + index, 10 + index, 9 + index, 10 + index, 11 + index });

        vertices[12 + index] = new Vector3(-1, 0.5f, 1);
        vertices[13 + index] = new Vector3(-2 - length, 0.5f, 1);
        vertices[14 + index] = new Vector3(-1, -0.5f, 1);
        vertices[15 + index] = new Vector3(-2 - length, -0.5f, 1);
        triangles.AddRange(new int[] { 13 + index, 12 + index, 14 + index, 13 + index, 14 + index, 15 + index });

        vertices[16 + index] = new Vector3(-1, 0.5f, 2);
        vertices[17 + index] = new Vector3(-2 - length, 0.5f, 2);
        vertices[18 + index] = new Vector3(-1, -0.5f, 2);
        vertices[19 + index] = new Vector3(-2 - length, -0.5f, 2);
        triangles.AddRange(new int[] { 18 + index, 16 + index, 17 + index, 19 + index, 18 + index, 17 + index });

        vertices[20 + index] = new Vector3(2.5f + length, 0.5f, 3);
        vertices[21 + index] = new Vector3(3 + length, 0.5f, 3);
        vertices[22 + index] = new Vector3(2.5f + length, -0.5f, 3);
        vertices[23 + index] = new Vector3(3 + length, -0.5f, 3);
        triangles.AddRange(new int[] { 21 + index, 20 + index, 22 + index, 21 + index, 22 + index, 23 + index });

        vertices[24 + index] = new Vector3(-2.5f - length, 0.5f, 3);
        vertices[25 + index] = new Vector3(-3 - length, 0.5f, 3);
        vertices[26 + index] = new Vector3(-2.5f - length, -0.5f, 3);
        vertices[27 + index] = new Vector3(-3 - length, -0.5f, 3);
        triangles.AddRange(new int[] { 26 + index, 24 + index, 25 + index, 27 + index, 26 + index, 25 + index });

        vertices[28 + index] = new Vector3(3 + length, 0.5f, 2);
        vertices[29 + index] = new Vector3(3 + length, 0.5f, 3);
        vertices[30 + index] = new Vector3(3 + length, -0.5f, 2);
        vertices[31 + index] = new Vector3(3 + length, -0.5f, 3);
        triangles.AddRange(new int[] { 30 + index, 28 + index, 29 + index, 31 + index, 30 + index, 29 + index });

        vertices[32 + index] = new Vector3(-3 - length, 0.5f, 2);
        vertices[33 + index] = new Vector3(-3 - length, 0.5f, 3);
        vertices[34 + index] = new Vector3(-3 - length, -0.5f, 2);
        vertices[35 + index] = new Vector3(-3 - length, -0.5f, 3);
        triangles.AddRange(new int[] { 33 + index, 32 + index, 34 + index, 33 + index, 34 + index, 35 + index });

        //Side Curves
        AddCircleBorder(ref vertices, ref triangles, 270.0f, 360.0f, new Vector3(1, 0.0f, 1), 1.0f, 0.5f, true);
        AddCircleBorder(ref vertices, ref triangles, 0.0f, 90.0f, new Vector3(-1, 0.0f, 1), 1.0f, 0.5f, true);
        AddCircleBorder(ref vertices, ref triangles, 90.0f, 180.0f, new Vector3(2 + length, 0.0f, 2), 1.0f, 0.5f, true);
        AddCircleBorder(ref vertices, ref triangles, 180.0f, 270.0f, new Vector3(-2 - length, 0.0f, 2), 1.0f, 0.5f, true);
        AddCircleBorder(ref vertices, ref triangles, 270.0f, 360.0f, new Vector3(1.5f, 0.0f, 0), 1.0f, 0.5f, false);
        AddCircleBorder(ref vertices, ref triangles, 0.0f, 90.0f, new Vector3(-1.5f, 0.0f, 0), 1.0f, 0.5f, false);
        AddCircleBorder(ref vertices, ref triangles, 90.0f, 180.0f, new Vector3(1.5f + length, 0.0f, 3), 1.0f, 0.5f, false);
        AddCircleBorder(ref vertices, ref triangles, 180.0f, 270.0f, new Vector3(-1.5f - length, 0.0f, 3), 1.0f, 0.5f, false);



        bracketMesh.vertices = vertices.ToArray();
        bracketMesh.triangles = triangles.ToArray();
        bracketMesh.RecalculateNormals();
        bracketMesh.RecalculateBounds();
        return bracketMesh;
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EnsureRenderer();
        Symbol symbolComp = ((Symbol)target);

        SymbolType symbolType = (SymbolType)type.enumValueIndex;
        symbolType = (SymbolType)EditorGUILayout.EnumPopup("Type", symbolType);
        type.enumValueIndex = (int)symbolType;
        float nLength = (float)EditorGUILayout.DoubleField("Length", length.floatValue);
        if (symbolType == SymbolType.CurlyBracket)
        {
            if (Mathf.Abs(symbolComp.gameObject.transform.localScale.x / symbolComp.gameObject.transform.localScale.z - 1.0f) > 0.000001f)
            {
                nLength = ((nLength + 3.0f) * symbolComp.gameObject.transform.localScale.x - 3.0f * symbolComp.gameObject.transform.localScale.z) / symbolComp.gameObject.transform.localScale.z;
                symbolComp.gameObject.transform.localScale = new Vector3(symbolComp.gameObject.transform.localScale.z, symbolComp.gameObject.transform.localScale.y, symbolComp.gameObject.transform.localScale.z);
            }
        }



        length.floatValue = Mathf.Clamp(nLength, 0.0f, 1000.0f);
        color.colorValue = EditorGUILayout.ColorField("Color", color.colorValue);




        Mesh nMesh = null;
        switch (symbolType)
        {
            case SymbolType.Arrow:
                nMesh = GetArrowMesh(length.floatValue);
                break;
            case SymbolType.CurlyBracket:
                nMesh = GetCurlyBracketMesh(length.floatValue);
                break;
        }

        this.meshFilter.mesh = nMesh;
        
        if (this.mat == null)
        {
            this.mat = new Material(Shader.Find("Standard"));
            this.mat.SetFloat("_Glossiness", 0.0f);

        }
        this.mat.color = color.colorValue;
        this.mat.EnableKeyword("_EMISSION");
        this.mat.SetColor("_EmissionColor", color.colorValue * 0.3f);
        this.meshRenderer.material = this.mat;

        serializedObject.ApplyModifiedProperties();
    }
}
#endif