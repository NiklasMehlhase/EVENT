using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoEmission : MonoBehaviour {
    private Renderer myRenderer;
	// Use this for initialization
	void Start () {
        this.myRenderer = this.GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
        myRenderer.UpdateGIMaterials();
	}
}
