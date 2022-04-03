using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivationObject : MonoBehaviour {
    public Action action;
    public bool touchColorChange = true;
    private int rangeCount;
    private List<Color> normCols;
    private List<Color> touchCols;
	private List<Material> mats;
    // Use this for initialization
    void Start () {
		this.mats = new List<Material> ();
		this.touchCols = new List<Color> ();
		this.normCols = new List<Color> ();
        MeshRenderer[] renderer = this.GetComponentsInChildren<MeshRenderer>();
        
        foreach (MeshRenderer rend in renderer)
        {
            if (!rend.gameObject.tag.Equals("FixedColor"))
            {
                Material nMat = new Material(rend.material);
				rend.material = nMat;
				mats.Add(nMat);
				Color normCol = Color.grey;
				if (nMat.HasProperty("_Color"))
				{
					normCol = nMat.color;
					this.normCols.Add(nMat.color);
				}
				float h, s, v;
				Color.RGBToHSV(normCol, out h, out s, out v);
				if (v > 0.5f)
				{
					this.touchCols.Add(Color.HSVToRGB(h, s, v - 0.25f));
				}
				else
				{
					this.touchCols.Add(Color.HSVToRGB(h, s, v + 0.25f));
				}
            }
        }
        
        this.rangeCount = 0;
    }

    public void EnterRange()
    {
        this.rangeCount++;
        if (this.rangeCount > 0)
        {
			for (int i = 0; i < this.mats.Count; i++) {
				Material mat = this.mats [i];
				if (mat.HasProperty ("_Color")) {
					mat.color = this.touchCols[i];
				}
			}
        }
    }

    public void ExitRange()
    {
        this.rangeCount--;
        if (this.rangeCount <= 0)
        {
			for (int i = 0; i < this.mats.Count; i++) {
				Material mat = this.mats [i];
				if (mat.HasProperty ("_Color")) {
					mat.color = this.normCols[i];
				}
			}
        }
    }

    public void Activate()
    {
        if(this.action!=null) { 
            this.action.Invoke();
        }
    }


    public void SetAction(Action nAction)
    {
        this.action = nAction;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
