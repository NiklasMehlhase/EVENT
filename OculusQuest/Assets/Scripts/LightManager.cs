using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour {
    private Light[] lights;
    private float[] originalIntensities;

	private float originalAmbient;
	private float originalSkybox;
	//private Skybox skybox;


    private float percentageIntensity;
    const float dimSpeed = 2.0f;
	// Use this for initialization
	void Start () {
        this.lights = null;
        this.originalIntensities = null;
		this.originalAmbient = RenderSettings.ambientIntensity;
		this.percentageIntensity = 1.0f;
		this.originalSkybox = RenderSettings.skybox.GetFloat ("_Exposure");
	}
	
    public void Dim(float percentageIntensity)
    {
        this.lights = Object.FindObjectsOfType<Light>();
        this.originalIntensities = new float[this.lights.Length];
        for(int i=0;i<this.lights.Length;i++)
        {
            this.originalIntensities[i] = this.lights[i].intensity;
        }
        this.percentageIntensity = percentageIntensity;
    }

    public void Reset()
    {
        this.percentageIntensity = 1.0f;
    }

	// Update is called once per frame
	void Update () {

		if(Mathf.Abs(RenderSettings.ambientIntensity-this.originalAmbient*percentageIntensity)<Time.deltaTime*dimSpeed)
		{
			RenderSettings.ambientIntensity = this.originalAmbient * percentageIntensity;
		}
		else if(RenderSettings.ambientIntensity< this.originalAmbient * percentageIntensity)
		{
			RenderSettings.ambientIntensity = RenderSettings.ambientIntensity + Time.deltaTime * dimSpeed;
		}
		else
		{
			RenderSettings.ambientIntensity = RenderSettings.ambientIntensity - Time.deltaTime * dimSpeed;
		}

		float skyboxExposure = RenderSettings.skybox.GetFloat ("_Exposure");
		if(Mathf.Abs(skyboxExposure-this.originalSkybox*percentageIntensity)<Time.deltaTime*dimSpeed)
		{
			RenderSettings.skybox.SetFloat("_Exposure",this.originalSkybox * percentageIntensity);
		}
		else if(skyboxExposure< this.originalSkybox * percentageIntensity)
		{
			RenderSettings.skybox.SetFloat("_Exposure",skyboxExposure + Time.deltaTime * dimSpeed);
		}
		else
		{
			RenderSettings.skybox.SetFloat("_Exposure",skyboxExposure - Time.deltaTime * dimSpeed);
		}

		if(this.lights!=null)
        {		
            for(int i=0;i<this.lights.Length;i++)
            {
                Light light = this.lights[i];
                if(light!=null) { 
                    if(Mathf.Abs(light.intensity-this.originalIntensities[i]*percentageIntensity)<Time.deltaTime*dimSpeed)
                    {
                        light.intensity = this.originalIntensities[i] * percentageIntensity;
                    }
                    else if(light.intensity< this.originalIntensities[i] * percentageIntensity)
                    {
                        light.intensity = light.intensity + Time.deltaTime * dimSpeed;
                    }
                    else
                    {
                        light.intensity = light.intensity - Time.deltaTime * dimSpeed;
                    }
                }
            }
        }
	}
}
