using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Quiz : MonoBehaviour,IQuiz {
    public bool randomOrder;
    public List<Question> questions;
	public bool hideRoom;

    private int curQuestion;
    private bool quizDone;
    private bool quizStarted;



	// Use this for initialization
	void Start () {
        this.quizDone = false;
	}
	
    public int GetCurQuestionNumber()
    {
        return this.curQuestion;
    }

    public int GetNumberOfQuestions()
    {
        return this.questions.Count;
    }

    public void startQuiz()
    {
        ResetQuiz();
        this.quizStarted = true;
    }

    private void ResetQuiz()
    {
        this.curQuestion = 0;
        this.quizDone = false;
        this.quizStarted = false;
    }

    public void StartQuizShow()
    {
        this.GetComponent<QuizShow>().StartQuiz();
    }

    public void StopQuizShow()
    {
        this.GetComponent<QuizShow>().Stop();
        ResetQuiz();
    }

    public Question getCurrentQuestion()
    {
        if (this.curQuestion < this.questions.Count)
        {
            return this.questions[this.curQuestion];
        }
        else if(this.curQuestion==this.questions.Count && this.questions.Count>1) 
        {
            Question end = ScriptableObject.CreateInstance<Question>();
            end.rightAnswer = "Hier klicken um weiter zu kommen";
            end.questionText = "Quiz beendet";
            return end;
        }
        else
        {
            return null;
        }
    }

    public Question nextQuestion()
    {
        this.curQuestion++;
        if (this.curQuestion > this.questions.Count)
        {
            this.quizDone = true;
        }
        else if(this.questions.Count==1 && this.curQuestion==1)
        {
            this.quizDone = true;
        }
        return getCurrentQuestion();        
    }

    public bool IsDone()
    {
        return this.quizDone;
    }

    public bool HasStarted()
    {
        return this.quizStarted;
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }

	public bool HideRoom() {
		return this.hideRoom;
	}
}
