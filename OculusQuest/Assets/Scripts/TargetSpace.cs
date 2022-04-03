using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSpace : MonoBehaviour {
    private new Collider collider;
    private HashSet<GrabObject> containedObjects;
    // Use this for initialization
    void Start() {
        this.collider = this.GetComponent<Collider>();
        this.collider.isTrigger = true;
        this.containedObjects = new HashSet<GrabObject>();
    }

    void OnTriggerEnter(Collider other)
    {
        GrabObject grabObject = other.GetComponent<GrabObject>();
        if (grabObject == null)
        {
            grabObject = other.GetComponentInParent<GrabObject>();
        }
        if (grabObject != null)
        {
            this.containedObjects.Add(grabObject);
        }        
    }

    void OnTriggerExit(Collider other)
    {
        GrabObject grabObject = other.GetComponent<GrabObject>();
        if (grabObject == null)
        {
            grabObject = other.GetComponentInParent<GrabObject>();
        }
        if (grabObject != null && this.containedObjects.Contains(grabObject))
        {
            this.containedObjects.Remove(grabObject);
        }
    }

    public bool Contains(GrabObject obj)
    {
        return this.containedObjects.Contains(obj);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
