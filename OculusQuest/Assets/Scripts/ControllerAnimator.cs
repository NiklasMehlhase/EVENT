using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerAnimator : MonoBehaviour
{
    public bool leftRight;

    /*public float triggerValue;
    public float lowButtonValue;
    public float highButtonValue;
    public float stickXValue;
    public float stickYValue;*/
    

    private Transform trigger;
    private Transform lowButton;
    private Transform highButton;
    private Transform stick;
    private Transform grip;

    private Vector3 oLowPosition;
    private Vector3 oHighPosition;
    private Quaternion oTriggerRotation;
    private Quaternion oStickRotation;
    private Vector3 oGripPosition;
    private Quaternion oGripRotation;

    // Start is called before the first frame update
    void Start()
    {
        string prefix = "";
        if(leftRight)
        {
            prefix = "r";
        }
        else
        {
            prefix = "l";
        }
        this.trigger = this.transform.Find(prefix+"_trigger");
        this.lowButton = this.transform.Find(prefix + "_low_button");
        this.highButton = this.transform.Find(prefix + "_high_button");
        this.stick = this.transform.Find(prefix + "_stick");
        this.grip = this.transform.Find(prefix + "_grip");

        oLowPosition = this.lowButton.localPosition;
        oHighPosition = this.highButton.localPosition;
        oTriggerRotation = this.trigger.localRotation;
        oStickRotation = this.stick.localRotation;
        oGripPosition = this.grip.localPosition;
        oGripRotation = this.grip.localRotation;

    }

    // Update is called once per frame
    void Update()
    {
        float lowButtonValue = 0f;
        float highButtonValue = 0f;
        float triggerValue = 0f;
        float stickXValue = 0f;
        float stickYValue = 0f;
        float gripValue = 0f;

        if(leftRight) //Right
        {
            lowButtonValue = (OVRInput.Get(OVRInput.Button.One) ? 1f : 0f);
            highButtonValue = (OVRInput.Get(OVRInput.Button.Two) ? 1f : 0f);
            triggerValue = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
            gripValue = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger);
            Vector2 stickVector = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            stickXValue = -stickVector.x;
            stickYValue = stickVector.y;
        }
        else //Left
        {
            lowButtonValue = (OVRInput.Get(OVRInput.Button.Three) ? 1f : 0f);
            highButtonValue = (OVRInput.Get(OVRInput.Button.Four) ? 1f : 0f);
            triggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
            gripValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);
            Vector2 stickVector = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            stickXValue = -stickVector.x;
            stickYValue = stickVector.y;
        }


        this.lowButton.localPosition = oLowPosition - Vector3.up*0.0017f*lowButtonValue;
        this.highButton.localPosition = oHighPosition - Vector3.up * 0.0017f * highButtonValue;
        this.trigger.localRotation = oTriggerRotation * Quaternion.Euler(20*triggerValue,0,0);
        this.stick.localRotation = oStickRotation * Quaternion.Euler(stickYValue*25f,0,0)*Quaternion.Euler(0,0,stickXValue*25f);
        if(this.leftRight) {
            this.grip.localPosition = oGripPosition + Vector3.right * gripValue * 0.003f;
            this.grip.localRotation = oGripRotation * Quaternion.Euler(0f, -gripValue * 10f, 0f);
        }
        else
        {
            this.grip.localPosition = oGripPosition - Vector3.right * gripValue * 0.003f;
            this.grip.localRotation = oGripRotation * Quaternion.Euler(0f, gripValue * 10f, 0f);            
        }
    }
}
