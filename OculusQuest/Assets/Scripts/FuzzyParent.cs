using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuzzyParent : MonoBehaviour {
    public Transform parent;

    public float posSpeed;
    public float rotSpeed;
    private Transform emptyChild;

    // Use this for initialization
	void Start () {
        GameObject child = new GameObject("EmptyChild");
        child.transform.parent = this.parent;
        child.transform.position = this.transform.position;
        child.transform.rotation = this.transform.rotation;
        child.transform.localScale = this.transform.localScale;
        this.emptyChild = child.transform;
	}

    public void SetParent(Transform nParent)
    {
        this.emptyChild.parent = nParent;
        this.emptyChild.position = this.transform.position;
        this.emptyChild.rotation = this.transform.rotation;
        this.emptyChild.localScale = this.transform.localScale;
    }


    // Update is called once per frame
    void Update () {
        Vector3 targetPos = emptyChild.position;
        Quaternion targetRot = emptyChild.rotation;
        


		if(Vector3.Distance(this.transform.position, targetPos)<=posSpeed*Time.deltaTime)
        {
            this.transform.position = targetPos;
        }
        else
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, targetPos, posSpeed * Time.deltaTime);
        }

        if(Quaternion.Angle(this.transform.rotation,targetRot)<=rotSpeed*Time.deltaTime)
        {
            this.transform.rotation = targetRot;
        }
        else
        {
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, targetRot, rotSpeed * Time.deltaTime);
        }
	}
}
