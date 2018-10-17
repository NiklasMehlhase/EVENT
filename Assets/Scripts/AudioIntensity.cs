using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioIntensity : MonoBehaviour {
    private AudioSource source;
    private Material material;
    private float[] baseColor;
    // Use this for initialization
	void Start () {
        this.baseColor = new float[3];
        this.source = this.GetComponentInChildren<AudioSource>();
        MeshRenderer renderer = this.GetComponent<MeshRenderer>();
        this.material = new Material(renderer.material);
        renderer.material = this.material;
        Color.RGBToHSV(this.material.color, out this.baseColor[0], out this.baseColor[1], out this.baseColor[2]);
    }
	
	// Update is called once per frame
	void Update () {
        float[] samples = new float[32];
        this.source.GetOutputData(samples, 1);
        float sum = 0.0f;
        for(int i=0;i<samples.Length;i++)
        {
            sum += Mathf.Abs(samples[i]);
        }
        float avg = sum / ((float)samples.Length);
        float normed = Mathf.Clamp(avg / 0.005f,0.0f,1.0f);
        this.material.color = Color.HSVToRGB(this.baseColor[0], this.baseColor[1], normed);
	}
}
