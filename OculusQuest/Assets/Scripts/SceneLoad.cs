using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoad : MonoBehaviour {
    public int sceneNumber;

	// Use this for initialization
	void Start () {
    	ActivationObject actObj = this.gameObject.AddComponent<ActivationObject> ();
		actObj.SetAction(new System.Action(delegate {
			SceneManager.LoadScene(this.sceneNumber);		
		}));
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
