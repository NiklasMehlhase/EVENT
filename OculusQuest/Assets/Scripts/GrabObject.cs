using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabObject : MonoBehaviour {
	public bool momentum = true;
	public bool fuzzy = false;
	private int rangeCount;
	private Material mat;
	private Color normCol;
	private Color touchCol;
	private new Rigidbody rigidbody;
	private HingeJoint leftHinge;
	private HingeJoint rightHinge;
	private Transform initParent;
	private Transform grabParent;
	private Vector3 lastPos;
	private Quaternion lastRot;
	private int grabCount;
	private Vector3 targetPos;
	private Quaternion targetRot;

	/*const float rotSpeed = 5.0f;
	const float posSpeed = 1.0f;
    */

	// Use this for initialization
	void Start () {
		this.grabCount = 0;
		this.initParent = this.transform.parent;
		this.rigidbody = this.GetComponent<Rigidbody> ();
        if(this.rigidbody==null)
        {
            if(this.gameObject.GetComponent<Collider>()==null) {
                MeshCollider collider = this.gameObject.AddComponent<MeshCollider>();
                collider.convex = true;
                collider.isTrigger = true;
            }
            this.rigidbody = this.gameObject.AddComponent<Rigidbody>();
            this.rigidbody.isKinematic = true;
        }
        MeshRenderer[] renderer = this.GetComponentsInChildren<MeshRenderer> ();
		this.mat = new Material (renderer[0].material);
		foreach(MeshRenderer rend in renderer) {
			if(!rend.gameObject.tag.Equals("FixedColor")) {
				rend.material = this.mat;
			}
		}
		if(this.mat.HasProperty("_Color")) {
			this.normCol = this.mat.color;
		}
		float h, s, v;
		Color.RGBToHSV (this.normCol, out h, out s, out v);
		if(v>0.5f) {
			this.touchCol = Color.HSVToRGB (h, s, v - 0.25f);
		}
		else {
			this.touchCol = Color.HSVToRGB (h, s, v + 0.25f);
		}
		this.rangeCount = 0;
	}

	public void EnterRange() {
		this.rangeCount++;
		if(this.rangeCount>0 && this.mat.HasProperty("_Color")) {
			this.mat.color = this.touchCol;
		}
	}

	public void ExitRange() {
		this.rangeCount--;
		if(this.rangeCount<=0 && this.mat.HasProperty("_Color")) {
			this.mat.color = this.normCol;
		}
	}


	public bool IsKinematic() {
		return this.rigidbody.isKinematic;
	}


	public void StartGrab(Rigidbody other,bool leftRight) {
		this.grabCount++;
		if (this.rigidbody.isKinematic) {
            this.grabParent = other.transform;
			this.transform.parent = other.transform;
		} else {

			if (leftRight && this.leftHinge == null) {
                Destroy(this.rightHinge);
				this.leftHinge = this.gameObject.AddComponent<HingeJoint> ();
				this.leftHinge.connectedBody = other;
				JointLimits limits = this.leftHinge.limits;
				//limits.bounciness = 1.0f;
				limits.min = this.leftHinge.angle;
				limits.max = this.leftHinge.angle;
				this.leftHinge.limits = limits;
				this.leftHinge.useLimits = true;

			} else if (!leftRight && this.rightHinge == null) {
                Destroy(this.leftHinge);
				this.rightHinge = this.gameObject.AddComponent<HingeJoint> ();
				this.rightHinge.connectedBody = other;
				JointLimits limits = this.rightHinge.limits;
				//limits.bounciness = 1.0f;
				limits.min = this.rightHinge.angle;
				limits.max = this.rightHinge.angle;
				this.rightHinge.limits = limits;
				this.rightHinge.useLimits = true;
			}
		}
	}

	public void EndGrab(Vector3 velocity,Vector3 angularVelocity,bool leftRight) {		
		this.grabCount--;
		if (this.rigidbody.isKinematic && this.grabCount==0) {
			this.grabParent = null;
			this.transform.parent = this.initParent;
		} else {
			if (leftRight) {
				Destroy (this.leftHinge);
			} else {
				Destroy (this.rightHinge);
			}
			if (momentum) {
				this.rigidbody.velocity = velocity;
				this.rigidbody.angularVelocity = angularVelocity;
			}
		}

	}

	private static void AxisAngle (Quaternion rot, out Vector3 axis, out float angle) {
		angle = 2.0f * Mathf.Acos (rot.w);
		float s = Mathf.Sqrt(1-rot.w*rot.w);
		if (s < 0.0001f) {
			axis = new Vector3 (1, 0, 0);
		} else {
			axis = new Vector3 (rot.x / s, rot.y / s, rot.z / s);
		}
	}

	// Update is called once per frame
	void Update () {

	}
}
