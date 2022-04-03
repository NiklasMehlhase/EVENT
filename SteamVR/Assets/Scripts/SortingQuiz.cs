using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortingQuiz : MonoBehaviour,IQuiz {

    public TargetSpace[] targets;
    public GrabObject[] sortObjects;
    public ActivationObject[] buttons;
    public AudioClip explenation;
    private bool started = false;
    private bool sortedCorrectly = false;
	private Vector3[] originalPositions;
	private Quaternion[] originalRotations;
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
        return this.sortedCorrectly;
    }

	public bool HideRoom() {
		return false;
	}

	private void SetRelevantObjectsActive(bool act) {
		foreach (GrabObject obj in sortObjects) {
			obj.gameObject.SetActive (act);
		}

		foreach (ActivationObject obj in buttons) {
			obj.gameObject.SetActive (act);
		}
	}

	private void ResetPositions() {
		for(int i=0;i<sortObjects.Length;i++) {
			sortObjects [i].transform.position = this.originalPositions [i];
			sortObjects [i].transform.rotation = this.originalRotations [i];
			sortObjects [i].GetComponent<Rigidbody> ().velocity = Vector3.zero;
		}
	}

    public void StartQuizShow()
    {
        this.started = true;
        this.sortedCorrectly = false;

		SetRelevantObjectsActive (true);
		ResetPositions ();

    }

    public void StopQuizShow()
    {
        this.started = false;
        this.sortedCorrectly = false;

		SetRelevantObjectsActive (false);
    }

    public void PlayExplenation()
    {
        if (this.explenation != null)
        {
            PresentationMaster master = GameObject.FindObjectOfType<PresentationMaster>();
            master.audioSource.Stop();
            master.audioSource.clip = this.explenation;
            master.audioSource.Play();
        }
    }

    public bool TestTargets(int startIndex,int endIndex)
    {
        for(int i=startIndex;i<=endIndex;i++)
        {
            TargetSpace target = null;
            if(i<targets.Length)
            {
                target = targets[i];
            }
            else
            {
                target = targets[targets.Length - 1];
            }
            if(target!=null && !target.Contains(this.sortObjects[i]))
            {
                return false;
            }
        }
        Invoke("TestAll",2f);
        return true;
    }

    public void SetCorrect()
    {
        this.sortedCorrectly = true;

		SetRelevantObjectsActive (false);
    }

    public void TestAll()
    {
        for (int i = 0; i < this.sortObjects.Length; i++)
        {
            TargetSpace target = null;
            if (i < targets.Length)
            {
                target = targets[i];
            }
            else
            {
                target = targets[targets.Length - 1];
            }
            bool anyWrongTarget = false;
            foreach(TargetSpace otherTarget in targets)
            {
                if(otherTarget.Contains(this.sortObjects[i]) && !otherTarget.Equals(target))
                {
                    anyWrongTarget = true;
                }
            }
            if (target != null && (!target.Contains(this.sortObjects[i]) || anyWrongTarget))
            {
                return; //Answer not correct
            }
        }
        float delay = (explenation == null ? 0.0f : explenation.length) + 1.0f;
        PlayExplenation();
        Invoke("SetCorrect", delay);
    }

    // Use this for initialization
    void Start () {
        this.timeKeeper = GameObject.FindObjectOfType<TimeKeeper>();
        this.originalPositions = new Vector3[sortObjects.Length];
		this.originalRotations = new Quaternion[sortObjects.Length];

		for(int i=0;i<sortObjects.Length;i++) {
			this.originalPositions [i] = sortObjects [i].transform.position;
			this.originalRotations [i] = sortObjects [i].transform.rotation;
            foreach(MeshRenderer renderer in sortObjects[i].gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                Material mat = renderer.material;
            }

		}


        Material rightMat = Resources.Load<Material>("Materials/SolidRightMat");
        Material wrongMat = Resources.Load<Material>("Materials/SolidWrongMat");
        Material neutralMat = Resources.Load<Material>("Materials/SolidNeutralMat");



        for (int i=0;i<buttons.Length;i++)
        {
            Renderer renderer = buttons[i].GetComponent<MeshRenderer>();
            renderer.material = neutralMat;
            if(i==buttons.Length-1)
            {
                int i2 = Mathf.Min(i, sortObjects.Length - 1);
                buttons[i].SetAction(() => {
                    bool correct = TestTargets(i2, sortObjects.Length - 1);
                    if(!correct && this.timeKeeper!=null) { 
                        timeKeeper.incWrongAnswers(this.name,"-","-");
                    }
                    renderer.material = correct ? rightMat : wrongMat;
                    StartCoroutine(SetColor(renderer, neutralMat, 2.0f));
                });
            }
            else
            {
                int i2 = Mathf.Min(i,sortObjects.Length-1);
                buttons[i].SetAction(() => {
                    bool correct = TestTargets(i2, i2);
                    if(!correct && this.timeKeeper!=null) { 
                        timeKeeper.incWrongAnswers(this.name,"-","-");
                    }
                    renderer.material = correct ? rightMat : wrongMat;
                    StartCoroutine(SetColor(renderer, neutralMat, 2.0f));
                });
            }
        }


		SetRelevantObjectsActive (false);
	}
	
    IEnumerator SetColor(Renderer renderer,Material mat,float delay)
    {
        yield return new WaitForSeconds(delay);
        renderer.material = mat;
    }

	// Update is called once per frame
	void Update () {
		for(int i=0;i<sortObjects.Length;i++) {
			if(Mathf.Abs(sortObjects [i].transform.position.x)>1.0f||Mathf.Abs(sortObjects [i].transform.position.z)>1.0f) {
				ResetPositions ();
			}				
		}
	}
}
