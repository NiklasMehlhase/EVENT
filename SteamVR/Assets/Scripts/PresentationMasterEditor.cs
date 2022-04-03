#if (UNITY_EDITOR) 
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PresentationMaster))]
[CanEditMultipleObjects]
public class PresentationMasterEditor : Editor
{
    SerializedProperty audioClips;
    SerializedProperty audioSource;
    SerializedProperty triggerPoints;
    SerializedProperty quizObjects;
    private List<bool> folded;

    void OnEnable()
    {
        this.folded = new List<bool>();
        audioClips = serializedObject.FindProperty("audioClips");
        audioSource = serializedObject.FindProperty("audioSource");
        triggerPoints = serializedObject.FindProperty("triggerPoints");
        quizObjects = serializedObject.FindProperty("quizObjects");
    }

    private void ensureSize(SerializedProperty serializedArray,int targetSize)
    {
        while (serializedArray.arraySize < targetSize)
        {
            serializedArray.InsertArrayElementAtIndex(serializedArray.arraySize);
            serializedArray.GetArrayElementAtIndex(serializedArray.arraySize - 1).objectReferenceValue = null;
        }
        while (serializedArray.arraySize > targetSize)
        {
            serializedArray.GetArrayElementAtIndex(serializedArray.arraySize - 1).objectReferenceValue = null;
            serializedArray.DeleteArrayElementAtIndex(serializedArray.arraySize - 1);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        audioSource.objectReferenceValue = EditorGUILayout.ObjectField("Audio Source", audioSource.objectReferenceValue, typeof(AudioSource),true);

        PresentationObject[] presentationObjects = Object.FindObjectsOfType<PresentationObject>();
        while (this.folded.Count < presentationObjects.Length)
        {
            this.folded.Add(false);
        }
        while (this.folded.Count > presentationObjects.Length)
        {
            this.folded.RemoveAt(this.folded.Count - 1);
        }


        if (presentationObjects.Length == 0)
        {
            EditorGUILayout.LabelField("No active presentation objects in scene");
        }
        else
        {
            bool showAll = GUILayout.Button("Show all");
            bool hideAll = GUILayout.Button("Hide all");
            if (showAll)
            {
                foreach (PresentationObject obj in presentationObjects)
                {
                    obj.ShowImmediate();
                }
            }
            else if (hideAll)
            {
                foreach (PresentationObject obj in presentationObjects)
                {
                    obj.HideImmediate();
                }
            }

            int minRoom;
            int maxRoom;
            PresentationUtilities.GetMinMaxRoom(out minRoom, out maxRoom);

            ensureSize(audioClips, (maxRoom - minRoom + 1));
            ensureSize(triggerPoints, (maxRoom - minRoom + 1));
            ensureSize(quizObjects, (maxRoom - minRoom + 1));

            for (int room = minRoom; room <= maxRoom; room++)
            {
                int numberOfObjects = 0;
                int numberOfHiddenObjects = 0;
                foreach (PresentationObject obj in presentationObjects)
                {
                    if (obj.startRoom <= room && obj.endRoom >= room)
                    {
                        numberOfObjects++;
                        if (obj.IsHidden())
                        {
                            numberOfHiddenObjects++;
                        }
                    }
                }

                string qualifier = "";
                if (numberOfObjects == 0)
                {
                    qualifier = "empty";
                }
                else if (numberOfObjects == numberOfHiddenObjects)
                {
                    qualifier = "hidden";
                }
                else if (numberOfHiddenObjects > 0)
                {
                    qualifier = "partially hidden";
                }
                else
                {
                    qualifier = "visible";
                }
                this.folded[room - minRoom] = EditorGUILayout.Foldout(this.folded[room - minRoom], "Room " + room + " (" + qualifier + ")");

                if (this.folded[room - minRoom])
                {                                        
                    AudioClip clip = (AudioClip)this.audioClips.GetArrayElementAtIndex(room-minRoom).objectReferenceValue;
                    clip = (AudioClip)EditorGUILayout.ObjectField("Audio", clip, typeof(AudioClip),true);
                    this.audioClips.GetArrayElementAtIndex(room - minRoom).objectReferenceValue = clip;

                    Collider triggerPoint = (Collider)this.triggerPoints.GetArrayElementAtIndex(room - minRoom).objectReferenceValue;
                    triggerPoint = (Collider)EditorGUILayout.ObjectField("Trigger Point", triggerPoint, typeof(Collider), true);
                    this.triggerPoints.GetArrayElementAtIndex(room - minRoom).objectReferenceValue = triggerPoint;

                    Quiz quiz = (Quiz)this.quizObjects.GetArrayElementAtIndex(room - minRoom).objectReferenceValue;
                    quiz = (Quiz)EditorGUILayout.ObjectField("Quiz", quiz, typeof(Quiz), true);
                    this.quizObjects.GetArrayElementAtIndex(room - minRoom).objectReferenceValue = quiz;


                    if (numberOfObjects > 0)
                    {
                        bool hide = GUILayout.Button("Hide");
                        bool show = GUILayout.Button("Show");
                        if (hide || show)
                        {
                            foreach (PresentationObject obj in presentationObjects)
                            {
                                if (obj.startRoom <= room && obj.endRoom >= room)
                                {
                                    if (hide)
                                    {
                                        obj.HideImmediate();
                                    }
                                    else if (show)
                                    {
                                        obj.ShowImmediate();
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }
        serializedObject.ApplyModifiedProperties();
    }

}
#endif