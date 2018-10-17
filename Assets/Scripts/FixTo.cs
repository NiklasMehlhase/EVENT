using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixTo : MonoBehaviour {
	public GameObject other;
	private Vector3 pos;
	private Quaternion rot;
	private Vector3 sca;
	private bool firstUpdate;

	// Use this for initialization
	void Start () {
		this.firstUpdate = false;
		this.pos = this.transform.localPosition;
		this.rot = this.transform.localRotation;
		this.sca = this.transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		if(!this.firstUpdate && other!=null) {
			this.transform.parent = other.transform;
			this.transform.localRotation = this.rot;
			this.transform.localPosition = this.pos;
			this.transform.localScale = this.sca;
		}
	}
}
