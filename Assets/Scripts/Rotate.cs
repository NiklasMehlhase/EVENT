using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {
	public Transform rotationCenter;
	public float rotationSpeed;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		this.gameObject.transform.RotateAround (rotationCenter.transform.position, rotationCenter.up, Time.deltaTime*this.rotationSpeed);
	}
}
