using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class Clickable : MonoBehaviour {

    private Action clickAction;
    private Action mouseInAction;
    private Action mouseOutAction;
	public bool mouseOver = true;
    public float clickCooldown = 0.0f;

    private float clickTime = 0.0f;
    private int queuedMouseAction = 0;
	private static ClickManager manager;
	private new Collider collider;

	public void Awake() {
		this.collider = this.GetComponent<Collider> ();

	}


	public void Start() {
		
		if(manager==null) {
			GameObject managerObj = new GameObject ("ClickManager");
			manager = managerObj.AddComponent<ClickManager> ();
		}
		manager.Subcribe (this);

	}


	public void DeactivateCollider() {
		this.collider.enabled = false;
	}

	public void ActivateCollider() {
		this.collider.enabled = true;
	}

    public void click()
    {
        if (this.clickTime <= 0.0f)
        {
            clickAction.Invoke();
            this.clickTime = this.clickCooldown;
        }
    }

    public void mouseIn()
    {
        if(this.clickTime<=0.0f) { 
            mouseInAction.Invoke();
            this.queuedMouseAction = 0;
        }
        else
        {
            this.queuedMouseAction = 1;
        }
    }

    public void mouseOut()
    {
        if (this.clickTime <= 0.0f)
        {
            mouseOutAction.Invoke();
            this.queuedMouseAction = 0;
        }
        else
        {
            this.queuedMouseAction = -1;
        }
    }

    public void setClickAction(Action nAction)
    {
        this.clickAction = nAction;
    }

    public void setMouseInAction(Action nAction)
    {
        this.mouseInAction = nAction;
    }
	
    public void setMouseOutAction(Action nAction)
    {
        this.mouseOutAction = nAction;
    }




    void Update()
    {
		if(this.clickTime > 0.0f)
        {
            this.clickTime -= Time.deltaTime;
            if(this.clickTime <= 0.0f)
            {
                if(this.queuedMouseAction==-1)
                {
                    mouseOutAction.Invoke();
                }
                else if(this.queuedMouseAction == 1)
                {
                    mouseInAction.Invoke();
                }
            }
        }
    }

}
