using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowOnCollider : MonoBehaviour {
	public CollisionMessager targetCollider;
	public GameObject objectToDeactivate;
	bool updated = false;
	// Use this for initialization
	void Start () {
		
		this.targetCollider.enterAction = new System.Action<Collider>(delegate (Collider collider) {			
			this.Show ();
			this.objectToDeactivate.SetActive(false);
		});
	}

	public void Show() {
		MeshRenderer[] renderers = this.GetComponentsInChildren<MeshRenderer> ();
		foreach(MeshRenderer renderer in renderers) {
			renderer.enabled = true;
		}

		Collider[] colliders = this.GetComponentsInChildren<Collider> ();
		foreach(Collider collider in colliders) {
			collider.enabled = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(!this.updated) {
			MeshRenderer[] renderers = this.GetComponentsInChildren<MeshRenderer> ();
			foreach(MeshRenderer renderer in renderers) {
				renderer.enabled = false;
			}

			Collider[] colliders = this.GetComponentsInChildren<Collider> ();
			foreach(Collider collider in colliders) {
				collider.enabled = false;
			}
			this.updated = true;
		}
	}
}
