using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class GrabVR : MonoBehaviour {
	private HashSet<GrabObject> grabbedObjectsInRange;
	private HashSet<GrabObject> grabbedObjects;

    private HashSet<ActivationObject> activationObjectsInRange;

	private Vector3 lastPos;
	private Quaternion lastRot;
	private new Rigidbody rigidbody;

	private bool lastAxis;
	private bool leftRight;
	// Use this for initialization
	void Start () {
		this.leftRight = this.name.Equals ("Controller (left)");
		this.lastPos = this.transform.position;
		this.lastRot = this.transform.rotation;
		this.grabbedObjectsInRange = new HashSet<GrabObject> ();
		this.grabbedObjects = new HashSet<GrabObject> ();
        this.activationObjectsInRange = new HashSet<ActivationObject>();
		SphereCollider collider = this.gameObject.AddComponent<SphereCollider> ();
		collider.radius = 0.1f;
		this.lastAxis = false;
		collider.isTrigger = true;
		this.rigidbody = this.gameObject.AddComponent<Rigidbody> ();
		rigidbody.isKinematic = true;
		rigidbody.useGravity = false;
	}

	void OnTriggerEnter(Collider other) {
		GrabObject grabObject = other.GetComponent<GrabObject>();
		if(grabObject==null) {
			grabObject = other.GetComponentInParent<GrabObject> ();
		}
		if (grabObject != null) {
			this.grabbedObjectsInRange.Add (grabObject);
			grabObject.EnterRange ();
		}

        ActivationObject activationObject = other.GetComponent<ActivationObject>();
        if(activationObject== null)
        {
            activationObject = other.GetComponentInParent<ActivationObject>();
        }
        if(activationObject!=null)
        {
            this.activationObjectsInRange.Add(activationObject);
            activationObject.EnterRange();
        }
	}

	void OnTriggerExit(Collider other) {
		GrabObject grabObject = other.GetComponent<GrabObject>();
		if(grabObject==null) {
			grabObject = other.GetComponentInParent<GrabObject> ();
		}
		if (grabObject != null) {
			grabObject.ExitRange ();
			if(this.grabbedObjectsInRange.Contains(grabObject)) {
				this.grabbedObjectsInRange.Remove (grabObject);
			}
			if(this.grabbedObjects.Contains(grabObject)) {
				grabObject.EndGrab (Vector3.zero, Vector3.zero, this.leftRight);
				this.grabbedObjects.Remove (grabObject);
			}
		}

        ActivationObject activationObject = other.GetComponent<ActivationObject>();
        if (activationObject == null)
        {
            activationObject = other.GetComponentInParent<ActivationObject>();
        }
        if (activationObject != null)
        {
            this.activationObjectsInRange.Remove(activationObject);
            activationObject.ExitRange();
        }
    }


	private void Grab(bool leftRight) {
        foreach(GrabObject obj in this.grabbedObjectsInRange) {
			if(!this.grabbedObjects.Contains(obj)) {
        		this.grabbedObjects.Add (obj);
				obj.StartGrab (this.rigidbody,leftRight);
			}
		}

        foreach(ActivationObject obj in this.activationObjectsInRange)
        {
            obj.Activate();
        }
	}


	private void Release(bool leftRight) {
		foreach (GrabObject obj in this.grabbedObjects) {
			Quaternion deltaRot = this.transform.rotation*Quaternion.Inverse (this.lastRot);
			deltaRot.w = -deltaRot.w;
			Vector3 deltaEuler = deltaRot.eulerAngles / Time.deltaTime;
			deltaEuler.x = Mathf.DeltaAngle (0, deltaEuler.x);
			deltaEuler.y = Mathf.DeltaAngle (0, deltaEuler.y);
			deltaEuler.z = Mathf.DeltaAngle (0, deltaEuler.z);

			obj.EndGrab ((this.transform.position-this.lastPos)/Time.deltaTime*1.75f,deltaEuler/50.0f,leftRight);
		}
		this.grabbedObjects.Clear ();
	}

	// Update is called once per frame
	void Update () {
        
		bool curAxis = false;
		if(this.leftRight) {			
			curAxis = Input.GetAxisRaw ("LeftControllerGrip") > 0.0f;
			if (!curAxis) {
				curAxis = Input.GetAxisRaw ("LeftControllerTrigger") > 0.0f;
			}
		}
		else {
			curAxis = Input.GetAxisRaw ("RightControllerGrip") > 0.0f;		
			if (!curAxis) {
				curAxis = Input.GetAxisRaw ("RightControllerTrigger") > 0.0f;
			}
		}

        if(!VRSettings.enabled)
        {
            if(Input.GetMouseButton(0) && this.leftRight)
            {
                curAxis = true;
            }
            else if(Input.GetMouseButton(1) && !this.leftRight)
            {
                curAxis = true;
            }
            else
            {
                curAxis = false;
            }
        }

		if(this.lastAxis && !curAxis) {
			Release (this.leftRight);
		}
		else if(!this.lastAxis && curAxis) {
			Grab (this.leftRight);
		}

		this.lastAxis = curAxis;

		this.lastPos = this.transform.position;
		this.lastRot = this.transform.rotation;
	}
}
