using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IQuiz
{
    bool HasStarted();
    void StopQuizShow();
    void StartQuizShow();
    bool IsDone();
	bool HideRoom();
    GameObject GetGameObject();
}
