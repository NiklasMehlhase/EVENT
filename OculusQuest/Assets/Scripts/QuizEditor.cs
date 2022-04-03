#if (UNITY_EDITOR) 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Quiz))]
[CanEditMultipleObjects]
public class NewBehaviourScript : Editor
{
    SerializedProperty randomOrder;
    SerializedProperty questions;
	SerializedProperty hideRoom;
    
    private List<bool> folded;

    

    [MenuItem("GameObject/3D Object/Quiz")]
    public static void CreateQuiz()
    {
        GameObject gameObject = new GameObject("Quiz" + EditorUtilities.GetSuffix("Quiz"));
        gameObject.AddComponent<Quiz>();        
        QuizShow show = gameObject.AddComponent<QuizShow>();
        show.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Text/OpenSans-Regular.ttf");
		show.fontMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Text/TextMaterial.mat");
        show.size = new Vector3(1.5f,1f,0.1f);
        show.wrongEffect = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/wrong.wav");
        show.rightEffect = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/right.wav");
        gameObject.transform.position = new Vector3(0.0f,1.5f,0.0f);
        Selection.activeGameObject = gameObject;

        GameObject textAudioObject = new GameObject("Quiz_TextAudioSource");
        textAudioObject.transform.parent = gameObject.transform;
        textAudioObject.transform.localPosition = Vector3.zero;
        AudioSource textAudioSource = textAudioObject.AddComponent<AudioSource>();
        textAudioSource.playOnAwake = false;
        textAudioSource.spatialBlend = 0.5f;
        textAudioSource.minDistance = 2.0f;       
        show.textAudioSource = textAudioObject.AddComponent<AudioPlaylist>();

        GameObject effectAudioObject = new GameObject("Quiz_EffectAudioSource");
        effectAudioObject.transform.parent = gameObject.transform;
        effectAudioObject.transform.localPosition = Vector3.zero;
        AudioSource effectAudioSource = effectAudioObject.AddComponent<AudioSource>();
        effectAudioSource.playOnAwake = false;
        effectAudioSource.spatialBlend = 0.5f;
        effectAudioSource.minDistance = 2.0f;
        show.effectAudioSource = effectAudioSource;

    }


    void OnEnable()
    {
        randomOrder = serializedObject.FindProperty("randomOrder");
        questions = serializedObject.FindProperty("questions");
		hideRoom = serializedObject.FindProperty ("hideRoom");
        this.folded = new List<bool>();
        for(int i=0;i<questions.arraySize;i++)
        {
            this.folded.Add(false);
        }
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();
		hideRoom.boolValue = EditorGUILayout.Toggle ("Hide room?",hideRoom.boolValue);
        EditorGUILayout.LabelField(questions.arraySize+(questions.arraySize>1?" questions":" question"));
        
        for (int i = 0; i < questions.arraySize; i++)
        {
            SerializedProperty question = questions.GetArrayElementAtIndex(i);
            this.folded[i] = EditorGUILayout.Foldout(this.folded[i], "Question " + (i + 1));
            if(this.folded[i]) {
                bool removeQuestion = GUILayout.Button("Remove question");
                if(removeQuestion)
                {
                    questions.GetArrayElementAtIndex(i).objectReferenceValue = null;
                    questions.DeleteArrayElementAtIndex(i);
                    this.folded.RemoveAt(i);
                }
                else { 
                    Question questionObject = (Question)question.objectReferenceValue;
                    Undo.RecordObject(questionObject, "Edit question");
                    EditorGUILayout.LabelField("Question text");
                    questionObject.questionText = EditorGUILayout.TextArea(questionObject.questionText, GUILayout.MaxHeight(50));
                    questionObject.questionAudio = (AudioClip)EditorGUILayout.ObjectField("Question audio", questionObject.questionAudio, typeof(AudioClip),true);
                    EditorGUILayout.LabelField("Right answer");
                    questionObject.rightAnswer = EditorGUILayout.TextField(questionObject.rightAnswer);
                    questionObject.rightAnswerAudio = (AudioClip)EditorGUILayout.ObjectField("Answer audio", questionObject.rightAnswerAudio, typeof(AudioClip), true);
                    questionObject.rightExplenation = EditorGUILayout.TextArea(questionObject.rightExplenation, GUILayout.MaxHeight(50));
                    questionObject.rightExplenationAudio = (AudioClip)EditorGUILayout.ObjectField("Explenation audio", questionObject.rightExplenationAudio, typeof(AudioClip), true);

                    for (int j=0;j<questionObject.wrongAnswers.Count;j++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Wrong answer #"+(j+1));
                        bool removeAnswer = GUILayout.Button("Remove");
                        EditorGUILayout.EndHorizontal();
                        questionObject.wrongAnswers[j] = EditorGUILayout.TextField(questionObject.wrongAnswers[j]);       
                        questionObject.answersAudio[j] = (AudioClip)EditorGUILayout.ObjectField("Answer audio", questionObject.answersAudio[j], typeof(AudioClip), true);
                        questionObject.explenations[j] = EditorGUILayout.TextArea(questionObject.explenations[j], GUILayout.MaxHeight(50));
                        questionObject.explenationsAudio[j] = (AudioClip)EditorGUILayout.ObjectField("Explenation audio", questionObject.explenationsAudio[j], typeof(AudioClip), true);
                        if (removeAnswer)
                        {
                            questionObject.wrongAnswers.RemoveAt(j);
                        }
                    }
                    bool addAnswer = GUILayout.Button("Add wrong answer");
                    if(addAnswer)
                    {
                        questionObject.wrongAnswers.Add("");
                        questionObject.explenations.Add("");
                        questionObject.explenationsAudio.Add(null);
                        questionObject.answersAudio.Add(null);
                    }
                    
                }
            }
        }

        bool addQuestion = GUILayout.Button("Add question");
        if(addQuestion)
        {
            this.questions.InsertArrayElementAtIndex(questions.arraySize);
            this.questions.GetArrayElementAtIndex(questions.arraySize-1).objectReferenceValue = ScriptableObject.CreateInstance(typeof(Question));
            this.folded.Add(true);
        }
        serializedObject.ApplyModifiedProperties();

    }
}
#endif