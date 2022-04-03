using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SymbolType {CurlyBracket,Arrow };

public class Symbol : MonoBehaviour {
    public Color color = Color.grey;
    public SymbolType type;
    public float length = 1.0f;
    
}
