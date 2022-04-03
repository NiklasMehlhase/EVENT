using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPointer : MonoBehaviour {
    private Clickable lastHover;
	// Use this for initialization
	void Start () {
        this.lastHover = null;
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 mousePos = Input.mousePosition;
        Ray mouseRay = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(mouseRay,out hitInfo);
        if (hit)
        {
            Clickable clickable = hitInfo.collider.gameObject.GetComponent<Clickable>();
            if (clickable != null)
            {

                if (Input.GetMouseButtonDown(0))
                {
                    clickable.click();
                }
                else
                {
                                        
                }            
            }
            if (clickable!=lastHover)
            {
                if(clickable!=null) { 
                    clickable.mouseIn();
                }
                if(lastHover!=null) { 
                    this.lastHover.mouseOut();
                }
                this.lastHover = clickable;
            }
        }
	}
}
