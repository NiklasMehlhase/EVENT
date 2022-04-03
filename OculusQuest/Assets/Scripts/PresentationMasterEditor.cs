#if (UNITY_EDITOR) 
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
class AutoUpdater
{

    static GameObject[] resourceModels;
    static AutoUpdater()
    {
        Application.logMessageReceived += Update;
        resourceModels = Resources.LoadAll<GameObject>("Models");
    }


    private static int Overlap(string a,string b)
    {
        int overlap = 0;
        bool stillSame = true;
        for(int i=0;i<Mathf.Min(a.Length,b.Length) && stillSame;i++)
        {
            if(a.ElementAt(i)==b.ElementAt(i))
            {
                overlap++;
            }
            else
            {
                stillSame = false;
            }
        }

        return overlap;
    }

    private static float parse(string str)
    {
        return float.Parse(str.Replace(',','.'), System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    public static void Update(string condition, string stackTrace, LogType type)
    {
        if(condition.Contains("EDITDATA:")) {
            condition = condition.Split('\n')[0];
            int index = condition.IndexOf("EDITDATA:");

            string dataStr = condition.Substring(index+9);
            //Debug.Log(dataStr);
            
            string[] data = dataStr.Split('|');
            string name = data[0];
            int id = int.Parse(data[1]);
            int startRoom = int.Parse(data[2]);
            int endRoom = int.Parse(data[3]);
            string[] posStr = data[4].Split('/');
            string[] scaleStr = data[5].Split('/');
            string[] rotStr = data[6].Split('/');
            Vector3 pos = new Vector3(parse(posStr[0]), parse(posStr[1]), parse(posStr[2]));
            Vector3 scale = new Vector3(parse(scaleStr[0]), parse(scaleStr[1]), parse(scaleStr[2]));
            Quaternion rot = new Quaternion(parse(rotStr[0]), parse(rotStr[1]), parse(rotStr[2]), parse(rotStr[3]));
            //File.WriteAllText("D:/Documents/tempLog.txt", posStr[0]);

            //Debug.Log(rotStr[0]+"   "+ rotStr[1] + "   " + rotStr[2] + "   " + rotStr[3]);
            PresentationObject[] presentationObjects = Object.FindObjectsOfType<PresentationObject>();
            bool foundObj = false;
            foreach (PresentationObject obj in presentationObjects) {
                if(obj.GetEditId()==id)
                {
                    foundObj = true;
                    if(obj.transform.localPosition!=pos||obj.transform.localScale!=scale||obj.transform.localRotation!=rot)
                    {
                        Undo.RecordObject(obj.transform, "Change transform");
                        obj.transform.localPosition = pos;
                        obj.transform.localScale = scale;
                        obj.transform.localRotation = rot;
                        Undo.FlushUndoRecordObjects();
                    }
                    if(obj.startRoom!=startRoom||obj.endRoom!=endRoom)
                    {
                        Undo.RecordObject(obj, "Change rooms");
                        obj.startRoom = startRoom;
                        obj.endRoom = endRoom;
                        Undo.FlushUndoRecordObjects();
                    }
                    
                    
                }
            }

            if(!foundObj)
            {
                int maxOverlap = -1;
                GameObject maxObj = null;
                foreach(GameObject gObj in resourceModels)
                {
                    int overlap = Overlap(name, gObj.name);
                    if(overlap>maxOverlap)
                    {
                        maxOverlap = overlap;
                        maxObj = gObj;
                    }
                }
                GameObject obj = GameObject.Instantiate(maxObj);
                obj.transform.localPosition = pos;
                obj.transform.localScale = scale;
                obj.transform.localRotation = rot;
                PresentationObject pObj = obj.AddComponent<PresentationObject>();
                pObj.SetEditId(id);
                pObj.startRoom = startRoom;
                pObj.endRoom = endRoom;
                Undo.RegisterCreatedObjectUndo(obj, "Create new object");
                
            }
        }
        else if(condition.Contains("EDITIDS:"))
        {
            condition = condition.Split('\n')[0];
            int index = condition.IndexOf("EDITIDS:");
            string dataStr = condition.Substring(index + 8);
            string[] idStrs = dataStr.Split('/');
            List<int> ids = new List<int>();
            foreach (string idStr in idStrs)
            {
                int id = int.Parse(idStr);
                ids.Add(id);
            }

            PresentationObject[] presentationObjects = Object.FindObjectsOfType<PresentationObject>();
            foreach (PresentationObject obj in presentationObjects)
            {
                if(!ids.Contains(obj.GetEditId()))
                {
                    Undo.DestroyObjectImmediate(obj);
                }                
            }
            Undo.FlushUndoRecordObjects();
        }
    }
}


[CustomEditor(typeof(PresentationMaster))]
[CanEditMultipleObjects]
public class PresentationMasterEditor : Editor
{
    SerializedProperty audioClips;
    SerializedProperty audioSource;
    SerializedProperty triggerPoints;
    SerializedProperty quizObjects;
    SerializedProperty enableVREdit;
    private List<bool> folded;

    void OnEnable()
    {
        this.folded = new List<bool>();
        audioClips = serializedObject.FindProperty("audioClips");
        audioSource = serializedObject.FindProperty("audioSource");
        triggerPoints = serializedObject.FindProperty("triggerPoints");
        quizObjects = serializedObject.FindProperty("quizObjects");
        enableVREdit = serializedObject.FindProperty("enableVREdit");
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

        enableVREdit.boolValue = EditorGUILayout.Toggle("Enable VR Edit", enableVREdit.boolValue);

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