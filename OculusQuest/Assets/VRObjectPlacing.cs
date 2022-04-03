using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRObjectPlacing : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] models = Resources.LoadAll<GameObject>("Models");
        foreach(GameObject obj in models)
        {
            Debug.Log(obj.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
