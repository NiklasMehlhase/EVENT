using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class NonVRController : MonoBehaviour
{
    public Transform eyeCam;

    void Awake()
    {
        if (!UnityEngine.XR.XRSettings.enabled)
        {
            /*this.GetComponent<SteamVR_TrackedObject>().enabled = false;
            this.GetComponentInParent<SteamVR_ControllerManager>().enabled = false;
            this.GetComponentInParent<SteamVR_PlayArea>().enabled = false;*/
        }
    }

    // Use this for initialization
    void Start()
    {
        if (!UnityEngine.XR.XRSettings.enabled)
        {
            this.transform.parent = eyeCam;
            this.transform.localPosition = Vector3.zero;
            if (this.name.Contains("left"))
            {
                this.transform.localPosition -= eyeCam.transform.right * 0.5f;
            }
            else if (this.name.Contains("right"))
            {
                this.transform.localPosition += eyeCam.transform.right * 0.5f;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
