using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlaylist : MonoBehaviour {
    private AudioSource source;

    private List<Pair<float, AudioClip>> playList;
	// Use this for initialization
	void Start () {
        this.source = this.GetComponent<AudioSource>();
        this.playList = new List<Pair<float, AudioClip>>();
	}
	
    public void AddClip(AudioClip clip,float delay)
    {
        this.playList.Add(new Pair<float, AudioClip>(delay, clip));
    }

    public void Stop()
    {
        this.playList.Clear();
        this.source.Stop();
    }

    public void Play()
    {
        this.source.Play();
    }

    public void PlayDelayed(float delay)
    {
        this.source.PlayDelayed(delay);
    }

    public void SetClip(AudioClip clip)
    {
        this.playList.Clear();
        this.source.clip = clip;
    }


    public void Clear()
    {
		this.playList.Clear();
    }

	// Update is called once per frame
	void Update () {
		if(!this.source.isPlaying)
        {
            if(this.playList.Count>0)
            {
                Pair<float, AudioClip> next = this.playList[0];
                this.playList.RemoveAt(0);
				this.source.clip = next.Second;
                this.source.PlayDelayed(next.First);
            }
        }
	}
}
