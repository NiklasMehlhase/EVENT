using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Question : ScriptableObject
{
    [SerializeField]
    public string questionText;
    [SerializeField]
    public string rightAnswer;
    [SerializeField]
    public string rightExplenation;
    [SerializeField]
    public AudioClip questionAudio;
    [SerializeField]
    public AudioClip rightAnswerAudio;
    [SerializeField]
    public AudioClip rightExplenationAudio;
    [SerializeField]
    public List<string> wrongAnswers;
    [SerializeField]
    public List<string> explenations;
    [SerializeField]
    public List<AudioClip> explenationsAudio;
    [SerializeField]
    public List<AudioClip> answersAudio;


    public Question()
    {
        this.questionText = "";
        this.rightExplenation = "";
        this.rightAnswer = "";
        this.questionAudio = null;
        this.rightExplenationAudio = null;
        this.wrongAnswers = new List<string>();
        this.explenations = new List<string>();
        this.explenationsAudio = new List<AudioClip>();
        this.answersAudio = new List<AudioClip>();
    }
	
    public string getRightAnswer()
    {
        return this.rightAnswer;
    }

    public List<string> getWrongAnswers()
    {
        return this.wrongAnswers;
    }

    public List<Pair<int,Pair<string,string>>> getRandomAnswers()
    {
        List<Pair<int, Pair<string, string>>> answers = new List<Pair<int, Pair<string, string>>>();
        answers.Add(new Pair<int, Pair<string, string>>(0,new Pair<string,string>(this.rightAnswer,"")));
        for(int i=0;i<wrongAnswers.Count;i++)
        {
            answers.Add(new Pair<int, Pair<string, string>>(i+1, new Pair<string,string>(this.wrongAnswers[i],this.explenations[i])));
        }

        for(int i=0;i<answers.Count;i++)
        {
            int r1 = Random.Range(0, answers.Count);
            int r2 = Random.Range(0, answers.Count);
            Pair<int, Pair<string, string>> a1 = answers[r1];
            answers[r1] = answers[r2];
            answers[r2] = a1;
        }

        return answers;
    }

}

public class Pair<T, U>
{
    public T First { get; set; }
    public U Second { get; set; }

    public Pair(T first, U second)
    {
        this.First = first;
        this.Second = second;
    }
}
