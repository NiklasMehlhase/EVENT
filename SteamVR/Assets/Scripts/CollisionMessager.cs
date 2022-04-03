using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



public class CollisionMessager : MonoBehaviour {
    public Action<Collider> enterAction;
    public Action<Collider> exitAction;
	// Use this for initialization
	void Start () {
		
	}
	

    void OnTriggerEnter(Collider collider)
    {
        enterAction.Invoke(collider);
    }

    void OnTriggerExit(Collider collider)
    {
        exitAction.Invoke(collider);
    }

	// Update is called once per frame
	void Update () {
		
	}
}
