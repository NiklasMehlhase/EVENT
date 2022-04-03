using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulse : MonoBehaviour {

    private Vector3 originalScale;
    private float curScale;
    private float time;
	private float time2;
    private bool pulsing = false;
    private Material mat;
    private float[] originalColor;
	private float originalAlpha;
    const float speed = 7.0f;
	const float speed2 = 3.0f;
    const float scaleFactor = 0.7f;
	// Use this for initialization
	void Start () {
        this.originalScale = this.transform.localScale;
        MeshRenderer renderer = this.GetComponent<MeshRenderer>();
        this.mat = new Material(renderer.material);
		this.originalAlpha = this.mat.color.a;
        this.originalColor = new float[3];

		Color.RGBToHSV(this.mat.color, out this.originalColor[0], out this.originalColor[1], out this.originalColor[2]);

        renderer.material = this.mat;
        
	}
	
    public void Begin()
    {
		this.pulsing = true;
        this.time = 0.0f;
		this.time2 = 0.0f;
        this.curScale = 1.0f;
    }

    public void End()
    {
		this.pulsing = false;
    }

	public bool IsPulsing() {

		return this.pulsing;
	}

	// Update is called once per frame
	void Update () {
        if (this.pulsing)
        {
            this.curScale = Mathf.Pow(scaleFactor, Mathf.Sin(this.time));
			float scale2 = Mathf.Sin (this.time2);
			float colH = (scale2+1.0f)*0.5f * 0.4f + this.originalColor [0];
			while (colH > 1.0f) {
				colH -= 1.0f;
			}
			while (colH < 0.0f) {
				colH += 1.0f;
			}
			Color nColor = Color.HSVToRGB (colH, this.originalColor [1], this.originalColor [2]);
			nColor.a = this.originalAlpha;
			this.mat.color = nColor;

            this.transform.localScale = originalScale * this.curScale;
            this.time += Time.deltaTime * speed;
			this.time2 += Time.deltaTime*speed2;
        }
        else if (this.curScale != 1.0f)
        {
            float diff = Mathf.Abs(this.curScale - 1.0f);
            if (diff < Time.deltaTime * speed)
            {
                this.curScale = 1.0f;
				Color oColor = Color.HSVToRGB (originalColor [0], originalColor [1], originalColor [2]);
				oColor.a = this.originalAlpha;
				this.mat.color = oColor;
            }
            else if (this.curScale < 1.0f)
            {
                this.curScale += Time.deltaTime * speed;
            }
            else if (this.curScale > 1.0f)
            {
                this.curScale -= Time.deltaTime * speed;
            }
            this.transform.localScale = originalScale * this.curScale;
			if (this.curScale != 1.0f) {
				float colH = 0.4f * diff + this.originalColor[0];
				while (colH > 1.0f) {
					colH -= 1.0f;
				}
				while (colH < 0.0f) {
					colH += 1.0f;
				}
				Color nColor = Color.HSVToRGB (colH, this.originalColor [1], this.originalColor [2]);
				nColor.a = this.originalAlpha;
				this.mat.color = nColor;
			}
            
        }
	}
}
