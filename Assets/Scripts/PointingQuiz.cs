using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointingQuiz : MonoBehaviour,IQuiz {

    public Clickable rightArea;
    public Clickable[] wrongAreas;
    public AudioClip explenation;

    private bool clickedRightAnswer = false;
    private bool started = false;
    private TimeKeeper timeKeeper;



    public GameObject GetGameObject()
    {
        return this.gameObject;
    }

    public bool HasStarted()
    {
        return this.started;
    }

    public bool IsDone()
    {
        return this.clickedRightAnswer;
    }

    public void StartQuizShow()
    {
        this.clickedRightAnswer = false;
        this.started = true;

		this.rightArea.ActivateCollider ();
		foreach (Clickable wrongArea in wrongAreas) {
			wrongArea.ActivateCollider ();
		}
    }

    public void StopQuizShow()
    {
        this.started = false;

		this.rightArea.DeactivateCollider ();
		foreach (Clickable wrongArea in wrongAreas) {
			wrongArea.DeactivateCollider ();
		}
    }

	public bool HideRoom() {
		return false;
	}

    public void RightAnswer()
    {
        if(this.started) { 
            this.clickedRightAnswer = true;
			this.rightArea.DeactivateCollider ();
			foreach (Clickable wrongArea in wrongAreas) {
				wrongArea.DeactivateCollider ();
			}
        }
    }

    public void WrongAnswer(string id)
    {        
        if(this.started && this.timeKeeper!=null) {
            this.timeKeeper.incWrongAnswers(this.name,"-",id);
        }
    }

    public void PlayExplenation()
    {
        if(this.explenation!=null) { 
            PresentationMaster master = GameObject.FindObjectOfType<PresentationMaster>();
            master.audioSource.Stop();
            master.audioSource.clip = this.explenation;
            master.audioSource.Play();
        }
    }

    // Use this for initialization
    void Start () {
        this.timeKeeper = GameObject.FindObjectOfType<TimeKeeper>();
        Material invisibleMat = Resources.Load<Material>("Materials/InvisibleMat");
        Material mouseOverMat = Resources.Load<Material>("Materials/MouseOverMat");
        Material rightMat = Resources.Load<Material>("Materials/RightMat");
        Material wrongMat = Resources.Load<Material>("Materials/WrongMat");
		MaterialUtils.SetRenderQueue (ref invisibleMat, 3001);
		MaterialUtils.SetRenderQueue (ref mouseOverMat, 3001);
		MaterialUtils.SetRenderQueue (ref rightMat, 3001);
		MaterialUtils.SetRenderQueue (ref wrongMat, 3001);

        if(this.rightArea!=null) {
            MeshRenderer renderer = this.rightArea.GetComponent<MeshRenderer>();
            renderer.material = invisibleMat;
            float delay = (explenation==null?0.0f:explenation.length) + 1.5f;
			this.rightArea.setClickAction(() => { if(this.started) {renderer.material = rightMat;PlayExplenation(); Invoke("RightAnswer",delay); }});
			this.rightArea.setMouseInAction(() => {  if(this.started) {renderer.material = mouseOverMat; }});
			this.rightArea.setMouseOutAction(() => {  if(this.started) {renderer.material = invisibleMat; }});
            this.rightArea.clickCooldown = 2.0f;
			this.rightArea.DeactivateCollider ();
		}
        foreach (Clickable wrongArea in wrongAreas)
        {
            MeshRenderer renderer = wrongArea.GetComponent<MeshRenderer>();
            renderer.material = invisibleMat;
			wrongArea.setClickAction(() => { if(this.started) {renderer.material = wrongMat; WrongAnswer(wrongArea.name);  }});
			wrongArea.setMouseInAction(() => { if(this.started) {renderer.material = mouseOverMat; }});
			wrongArea.setMouseOutAction(() => {if(this.started) {renderer.material = invisibleMat; }});
            wrongArea.clickCooldown = 2.0f;
			wrongArea.DeactivateCollider ();
        }


    }

    // Update is called once per frame
    void Update () {
		
	}
}
