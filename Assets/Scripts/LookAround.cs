using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class LookAround : MonoBehaviour {
    public bool adaptForNonVR = true;
    private float xRot;
    private float yRot;
    private Quaternion initialRotation;
	// Use this for initialization
	void Start () {
        if (!VRSettings.enabled)
        {
            this.initialRotation = this.transform.localRotation;
            this.xRot = 0f;
            this.yRot = 0f;
            if(adaptForNonVR) { 
                this.transform.localPosition += this.transform.up * 1.8f;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        if(!VRSettings.enabled) { 
            Cursor.lockState = CursorLockMode.Locked;        
            float deltaX = Input.GetAxis("Mouse X");
            float deltaY = Input.GetAxis("Mouse Y");
            this.xRot += deltaX;
            this.yRot -= deltaY;
            this.transform.localRotation = this.initialRotation*Quaternion.Euler(yRot, xRot, 0);
            if(Input.GetKey(KeyCode.W))
            {
                this.transform.localPosition += Time.deltaTime * this.transform.forward;
            }
            if(Input.GetKey(KeyCode.S))
            {
                this.transform.localPosition -= Time.deltaTime * this.transform.forward;
            }
            if(Input.GetKey(KeyCode.A))
            {
                this.transform.localPosition -= Time.deltaTime * this.transform.right;             
            }
            if (Input.GetKey(KeyCode.D))
            {
                this.transform.localPosition += Time.deltaTime * this.transform.right;
            }
            if(Input.GetKey(KeyCode.E))
            {
                this.transform.position -= Time.deltaTime * Vector3.up;
            }
            if(Input.GetKey(KeyCode.Q))
            {
                this.transform.position += Time.deltaTime * Vector3.up;
            }

                
        }
    }
}
