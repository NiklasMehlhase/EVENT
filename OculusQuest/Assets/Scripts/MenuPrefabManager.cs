using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPrefabManager : MonoBehaviour {
    public GameObject menuPrefab;
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

    }

    // Use this for initialization
    void Start () {
	    	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
