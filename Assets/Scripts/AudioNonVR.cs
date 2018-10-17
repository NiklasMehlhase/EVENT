using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class AudioNonVR : MonoBehaviour {
    public Transform eyeCamera;
	// Use this for initialization
	void Start () {
        if(!VRSettings.enabled) { 
            this.transform.parent = eyeCamera;
            this.transform.localPosition = Vector3.zero;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
