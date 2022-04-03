using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class ClickManager : MonoBehaviour {
	private GameObject leftController;
	private GameObject rightController;
	private static List<Clickable> allClickables;
	private GameObject indicator;

	static ClickManager() {
		allClickables = new List<Clickable> ();
	}



	// Use this for initialization
	void Start () {
		
		if(UnityEngine.XR.XRSettings.enabled) {
			GameObject cam = GameObject.Find ("CenterEyeAnchor");
			leftController = cam.transform.parent.Find("LeftHandAnchor").Find("LeftControllerAnchor").Find("LeftIndicator").gameObject;
			rightController = cam.transform.parent.Find("RightHandAnchor").Find("RightControllerAnchor").Find("RightIndicator").gameObject;
        }

		this.indicator = GameObject.CreatePrimitive (PrimitiveType.Capsule);
		Destroy (this.indicator.GetComponent<Collider> ());
		this.indicator.transform.localScale = new Vector3 (0.05f,1.0f,0.05f);
		Material indicatorMat = new Material (Shader.Find("Standard"));
		indicatorMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
		indicatorMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
		indicatorMat.SetInt("_ZWrite", 0);
		indicatorMat.DisableKeyword("_ALPHATEST_ON");
		indicatorMat.DisableKeyword("_ALPHABLEND_ON");
		indicatorMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
		indicatorMat.SetFloat ("_Mode", 3.0f);
		indicatorMat.renderQueue = 3000;
		indicatorMat.SetColor ("_Color",new Color(0.0f,0.0f,0.85f,0.5f));
		this.indicator.GetComponent<MeshRenderer> ().material = indicatorMat;
		

	}


	public void Subcribe(Clickable clickable) {
		allClickables.Add (clickable);
	}

	private void CheckRay(Ray ray,bool button) {
        RaycastHit hitInfo;
		bool hit = Physics.Raycast (ray, out hitInfo);
		Vector3 indicatorTarget = ray.origin + ray.direction * 4.0f;
		Clickable curClickable = null;
		if (hit) {
			curClickable = hitInfo.collider.gameObject.GetComponent<Clickable> ();
			if (curClickable != null) {
				if (!curClickable.mouseOver) {
					curClickable.mouseOver = true;
					curClickable.mouseIn ();
				}
				if (button) {
					curClickable.click ();
				}
			}
			indicatorTarget = hitInfo.point;


		}

		Vector3 scale = this.indicator.transform.localScale;
		scale.y = Vector3.Distance (ray.origin, indicatorTarget)/2.0f;
		this.indicator.transform.localScale = scale;
		this.indicator.transform.position = (ray.origin + indicatorTarget) / 2.0f;
		this.indicator.transform.rotation = Quaternion.LookRotation (ray.direction)*Quaternion.Euler(90,0,0);

		foreach(Clickable clickable in allClickables) {
			if(!clickable.Equals(curClickable) && clickable.mouseOver) {
				clickable.mouseOver = false;
				clickable.mouseOut ();
			}
		}			

	}
	
	// Update is called once per frame
	void Update () {
		if (!UnityEngine.XR.XRSettings.enabled) {
            Camera cam = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();
            Ray mouseRay;
			if (Cursor.lockState == CursorLockMode.Locked) {
				Vector3 center = new Vector3 (Screen.width / 2, Screen.height / 2, 0);
				mouseRay = cam.ScreenPointToRay (center);

			} else { 
				mouseRay = cam.ScreenPointToRay (Input.mousePosition);

			}
			CheckRay (mouseRay,Input.GetMouseButtonUp (0));
			this.indicator.SetActive (false);

		} else {

            if (OVRInput.Get(OVRInput.Touch.SecondaryIndexTrigger)) {//Right
                this.indicator.SetActive(true);
                Ray rightRay = new Ray(this.rightController.transform.position, this.rightController.transform.forward);
                CheckRay(rightRay,OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger));
            } else if(OVRInput.Get(OVRInput.Touch.PrimaryIndexTrigger)) //Left
            {
                this.indicator.SetActive(true);
                Ray leftRay = new Ray(this.leftController.transform.position, this.leftController.transform.forward);
                CheckRay(leftRay, OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger));
            }
            else
            {
                this.indicator.SetActive(false);
                foreach (Clickable clickable in allClickables)
                {
                    if (clickable.mouseOver)
                    {
                        clickable.mouseOver = false;
                        clickable.mouseOut();
                    }
                }
            }

            /*
			if (Input.GetButton ("RightTrackpadTouch") && ControllerUtils.GetTrackpadPos(ControllerSide.RIGHT)==TrackpadPos.TOP) {
				this.indicator.SetActive (true);
				Ray rightRay = new Ray (this.rightController.transform.position, this.rightController.transform.forward);
				CheckRay (rightRay, Input.GetButtonDown ("RightTrackpadPress"));
			} else if (Input.GetButton ("LeftTrackpadTouch") && ControllerUtils.GetTrackpadPos(ControllerSide.LEFT) == TrackpadPos.TOP) {
				this.indicator.SetActive (true);
				Ray leftRay = new Ray (this.leftController.transform.position, this.leftController.transform.forward);
				CheckRay (leftRay, Input.GetButtonDown ("LeftTrackpadPress"));
			} else {
				this.indicator.SetActive (false);
				foreach(Clickable clickable in allClickables) {
					if(clickable.mouseOver) {
						clickable.mouseOver = false;
						clickable.mouseOut ();
					}
				}
			}
            */


		}
	}
}
