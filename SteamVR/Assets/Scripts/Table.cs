using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VerticalAlignment { Top,Middle,Bottom}

public enum HorizontalAlignment {Left,Center,Right}

public enum TableStyle {AllLines,NoSideLines,MinimalLines}

public class Table : MonoBehaviour {
    public string content;
    public Material lineMaterial;
    public Material textMaterialRegular;
    public Material textMaterialBold;
    public float padding = 0.1f;
    public float lineWidth = 0.1f;
    public VerticalAlignment verticalAlignment = VerticalAlignment.Middle;
    public HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center;
    public TableStyle tableStyle = TableStyle.AllLines;
}
