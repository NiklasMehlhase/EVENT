using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeKeeper : MonoBehaviour {

	private string userId;

    private Dictionary<string, float> timePerRoom;
    private Dictionary<string, int> numberOfVisits;
    private Dictionary<string, Dictionary<string,Dictionary<string,Dictionary<string,int>>>> wrongAnswers;

    private DateTime startTime;
    private string curRoom;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        this.timePerRoom = new Dictionary<string, float>();
        this.numberOfVisits = new Dictionary<string, int>();
        this.wrongAnswers = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, int>>>>();
        this.curRoom = "";
        startTime = DateTime.Now;
        this.userId = "UNKNOWN_USER";
    }


	public void setUserId(string id)
    {
        this.userId = id;
    }

	// Use this for initialization
	void Start () {

	}
	
    public void incWrongAnswers(string quizId,string questionId,string answerId)
    {
        if(!wrongAnswers.ContainsKey(this.curRoom)) {
            wrongAnswers.Add(this.curRoom, new Dictionary<string, Dictionary<string, Dictionary<string, int>>>());            
        }
        if(!wrongAnswers[this.curRoom].ContainsKey(quizId))
        {
            wrongAnswers[this.curRoom].Add(quizId, new Dictionary<string,Dictionary<string , int>>());
        }
        if(!wrongAnswers[this.curRoom][quizId].ContainsKey(questionId))
        {
            wrongAnswers[this.curRoom][quizId].Add(questionId, new Dictionary<string, int>());
        }
        if(!wrongAnswers[this.curRoom][quizId][questionId].ContainsKey(answerId))
        {
            wrongAnswers[this.curRoom][quizId][questionId].Add(answerId, 0);
        }

        wrongAnswers[this.curRoom][quizId][questionId][answerId]++;
    }


	// Update is called once per frame
	void Update () {
        string curSceneName = SceneManager.GetActiveScene().name;

        if (!curSceneName.Equals(this.curRoom))
        {
            if(!numberOfVisits.ContainsKey(curSceneName))
            {
                numberOfVisits.Add(curSceneName, 0);
            }
            numberOfVisits[curSceneName]++;
        }
        if(!timePerRoom.ContainsKey(curSceneName))
        {
            timePerRoom.Add(curSceneName, 0.0f);
        }
        timePerRoom[curSceneName] += Time.deltaTime;
        this.curRoom = curSceneName;
    }

    void OnApplicationQuit()
    {
        string csvStr = "";
        csvStr += "Start time;"+this.startTime.ToString("HH:mm dd.MM.yyyy")+ Environment.NewLine +"User id;"+this.userId+ Environment.NewLine+ Environment.NewLine + Environment.NewLine;
        float totalTime = 0.0f;
        int totalVisits = 0;
        csvStr += "Scene name;Time;Number of visits"+Environment.NewLine;
        foreach(KeyValuePair<string,float> entry in this.timePerRoom)
        {
            csvStr += entry.Key + ";" + entry.Value +";"+numberOfVisits[entry.Key] + Environment.NewLine;
            totalTime += entry.Value;
            totalVisits += numberOfVisits[entry.Key];
        }
        csvStr += Environment.NewLine+"Total;" + totalTime+";"+totalVisits+ Environment.NewLine + Environment.NewLine + Environment.NewLine;
        csvStr += "Room;Quiz;Question;Answer;Number of wrong answers" + Environment.NewLine;
        foreach (KeyValuePair<string, Dictionary<string,Dictionary<string,Dictionary<string,int>>>> entryRoom in this.wrongAnswers)
        {
            foreach(KeyValuePair<string,Dictionary<string,Dictionary<string,int>>> entryQuiz in entryRoom.Value)
            {
                foreach(KeyValuePair<string,Dictionary<string,int>> entryQuestion in entryQuiz.Value)
                {
                    csvStr += entryRoom.Key + ";" + entryQuiz.Key + ";" + entryQuestion.Key+";Total";
                    string answersStr = "";
                    int totalWrong = 0;
                    foreach (KeyValuePair<string,int> entryAnswer in entryQuestion.Value) { 
                        answersStr+=" ; ; ;"+entryAnswer.Key+";"+entryAnswer.Value+Environment.NewLine;
                        totalWrong += entryAnswer.Value;
                    }
                    csvStr += ";" + totalWrong + Environment.NewLine;
                    csvStr += answersStr;
                }
            }            
        }
        File.WriteAllText(Application.persistentDataPath+"/vrStudie_"+this.userId+"__"+this.startTime.ToString("yyyy-MM-dd_HH-mm-ss")+".csv", csvStr);
    }
}
