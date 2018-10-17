#if (UNITY_EDITOR) 
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GetAssetPath : MonoBehaviour
{
    public Object asset;
    // Use this for initialization
    void Start()
    {
        Debug.Log("Path: "+AssetDatabase.GetAssetPath(asset)+", "+asset.GetType());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
#endif