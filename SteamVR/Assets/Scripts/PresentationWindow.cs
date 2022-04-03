#if (UNITY_EDITOR) 
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PresentationWindow : EditorWindow
{
    private static float scrollPosY = 0.0f;
    private static float scrollPosX = 0.0f;
    private static GUIStyle centeredStyle = null;
    private static GUIStyle middleStyle = null;
    private static GUIStyle markedStyle = null;
    private static GUIStyle activeButton = null;
    private static GUIStyle hoverButton = null;
    private static int dragging = -1;
    private static bool draggingSide;
    private static PresentationMaster master;
    private static bool noMaster = false;
    private static PresentationObject[] presentationObjects;
    private static int minRoom;
    private static int maxRoom;
    private static bool leftMouseDown = false;
    private static double lastFrameTime;



    const int scrollbarWidth = 20;
    const int verticalPadding = 25;
    const int objectPadding = 10;
    const int heightPerObject = 50;
    const int headingHeight = 100;
    const int textWidth = 100;
    const int objectWidth = 150;
    const int horizontalPadding = 50;
    const float roomHeightFactor = 3.5f;
    
    private static void EnsureMaster()
    {
        PresentationMaster master = UnityEngine.Object.FindObjectOfType<PresentationMaster>();
        if (master == null)
        {
            GameObject masterObject = new GameObject("PresentationMaster");
            masterObject.transform.position = Vector3.zero;
            master = masterObject.AddComponent<PresentationMaster>();
        }

        if (master.audioSource == null)
        {
            AudioSource audioSource = UnityEngine.Object.FindObjectOfType<AudioSource>();
            if (audioSource == null)
            {
                GameObject audioObject = new GameObject("PresentationMaster_AudioSource");
                audioObject.transform.position = Vector3.zero;
                audioSource = audioObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.0f;
                audioSource.minDistance = 2.0f;
            }
            master.audioSource = audioSource;
        }
    }

    [MenuItem("Edit/Refresh Lighting %l")]
    static void Lighting()
    {
        Lightmapping.BakeAsync();
    }


    [MenuItem("Window/Presentation %w")]
    static void Init()
    {
        EnsureMaster();
        presentationObjects = new PresentationObject[0];
        PresentationWindow window = (PresentationWindow)EditorWindow.GetWindow(typeof(PresentationWindow), true, "Presentation Editor", false);
        window.autoRepaintOnSceneChange = true;
        window.wantsMouseMove = true;
        window.Show();
        window.UpdatePresentationObjects();
        lastFrameTime = EditorApplication.timeSinceStartup;
        
    }

    void InitStyle()
    {
        if (centeredStyle == null)
        {
            centeredStyle = new GUIStyle();
            centeredStyle.alignment = TextAnchor.MiddleLeft;
        }
        if (middleStyle == null)
        {
            middleStyle = new GUIStyle();
            middleStyle.alignment = TextAnchor.MiddleCenter;
        }
        if (markedStyle == null)
        {
            markedStyle = new GUIStyle();
            markedStyle.alignment = TextAnchor.MiddleLeft;
            markedStyle.normal.textColor = Color.red;
        }
        if (activeButton == null)
        {
            activeButton = new GUIStyle(GUI.skin.button);
            activeButton.normal = activeButton.active;
        }
        if (hoverButton == null)
        {
            hoverButton = new GUIStyle(GUI.skin.button);
            hoverButton.normal = hoverButton.hover;
        }
    }

    void FindMaster()
    {
        if (master == null)
        {
            master = UnityEngine.Object.FindObjectOfType<PresentationMaster>();
            if (master == null)
            {
                EnsureMaster();
                master = UnityEngine.Object.FindObjectOfType<PresentationMaster>();
            }
        }
    }


    private static int PresentationObjectComparison(PresentationObject a, PresentationObject b)
    {
        if (a.startRoom > b.startRoom)
        {
            return 1;
        }
        else if (a.startRoom < b.startRoom)
        {
            return -1;
        }
        else
        {
            if (a.infinite && !b.infinite)
            {
                return -1;
            }
            else if (!a.infinite && b.infinite)
            {
                return 1;
            }
            else if (a.infinite && b.infinite)
            {
                return string.Compare(a.gameObject.name, b.gameObject.name);
            }
            else if (a.endRoom > b.endRoom)
            {
                return -1;
            }
            else if (a.endRoom < b.endRoom)
            {
                return 1;
            }
            else
            {
                return string.Compare(a.gameObject.name, b.gameObject.name);
            }
        }
    }

    private void UpdatePresentationObjects()
    {
        presentationObjects = UnityEngine.Object.FindObjectsOfType<PresentationObject>();
        Array.Sort<PresentationObject>(presentationObjects, PresentationObjectComparison);
    }

    private void RemoveNullObjects()
    {
        List<PresentationObject> objectsToRemove = new List<PresentationObject>();
        if (presentationObjects == null)
        {
            UpdatePresentationObjects();
        }
        foreach (PresentationObject obj in presentationObjects)
        {
            if (obj == null)
            {
                objectsToRemove.Add(obj);
            }
        }
        presentationObjects = presentationObjects.Except(objectsToRemove).ToArray();
    }


    private void HotkeyCheck(ref PresentationWindow window)
    {
        if (Event.current.rawType == EventType.keyUp && Event.current.control && Event.current.keyCode == KeyCode.E)
        {
            Event.current.Use();
            window.Close();
        }
    }

    private void NormalizeMinMaxRoom()
    {
        foreach (PresentationObject obj in presentationObjects)
        {
            obj.startRoom = obj.startRoom - minRoom + 1;
            obj.endRoom = obj.endRoom - minRoom + 1;
        }
        maxRoom = maxRoom - minRoom + 1;
        minRoom = 1;
    }

    private int GetMouseIndex(Vector2 mousePos)
    {

        int lowerIndex = Mathf.CeilToInt(((float)(scrollPosY - heightPerObject - verticalPadding - heightPerObject * roomHeightFactor + mousePos.y)) / ((float)(heightPerObject + objectPadding)));
        int upperIndex = Mathf.FloorToInt(((float)(scrollPosY - verticalPadding - heightPerObject * roomHeightFactor + mousePos.y)) / ((float)(heightPerObject + objectPadding)));
        int index = -1;
        if (lowerIndex == upperIndex && lowerIndex >= 0 && lowerIndex < presentationObjects.Length)
        {
            index = lowerIndex;
        }
        return index;
    }


    private void DrawObjects(int index, Rect windowSize)
    {
        int startX = scrollbarWidth * 2;
        int startXScrolled = scrollbarWidth * 2 - (int)scrollPosX + horizontalPadding;
        int startXR = scrollbarWidth * 2 + textWidth;
        int height = -(int)scrollPosY;

        GUI.BeginGroup(new Rect(startXR, heightPerObject * roomHeightFactor + verticalPadding, windowSize.width - startXR, windowSize.height - heightPerObject * roomHeightFactor - verticalPadding));
        for (int i = 0; i < presentationObjects.Length; i++)
        {
            PresentationObject obj = presentationObjects[i];

            int start = -(int)scrollPosX + horizontalPadding + (obj.startRoom - minRoom) * objectWidth;
            int rEndRoom = maxRoom;
            if (!obj.infinite)
            {
                rEndRoom = obj.endRoom;
            }
            int end = -(int)scrollPosX + horizontalPadding + (rEndRoom - minRoom) * objectWidth + objectWidth;
            EditorGUI.DrawRect(new Rect(start, height, end - start, heightPerObject), new Color(0.9f, 0.0f, 0.0f));

            if ((index == i && (dragging < 0 || dragging == i)) || dragging == i)
            {
                EditorGUI.DrawRect(new Rect(-(int)scrollPosX, height + heightPerObject / 4.0f, start + (int)scrollPosX, heightPerObject / 2.0f), new Color(0.95f, 0.95f, 0.95f));
            }


            if (obj.startRoom == minRoom)
            {
                if (GUI.Button(new Rect(-(int)scrollPosX + horizontalPadding - 3 * horizontalPadding / 4, height, horizontalPadding / 2, heightPerObject), "+"))
                {
                    Undo.RecordObject(master, "Edit rooms");
                    Undo.RecordObject(obj, "Edit room");
                    obj.startRoom = minRoom - 1;
                    PresentationWindow.minRoom--;
                    CheckMasterLists();
                    for (int j = maxRoom; j > minRoom; j--)
                    {
                        master.audioClips[j - minRoom] = master.audioClips[j - minRoom - 1];
                        master.delays[j - minRoom] = master.delays[j - minRoom - 1];
						master.quizObjects[j - minRoom] = master.quizObjects[j - minRoom - 1];
                        master.times[j - minRoom] = master.times[j - minRoom - 1];
                        master.triggerPoints[j - minRoom] = master.triggerPoints[j - minRoom - 1];
                        master.userActivated[j - minRoom] = master.userActivated[j - minRoom - 1];
                    }
                    master.audioClips[0] = null;
                    master.delays[0] = 0;
					master.quizObjects[0] = null;
                    master.times[0] = 1;
                    master.triggerPoints[0] = null;
                    master.userActivated[0] = false;

                }


            }
            if (obj.endRoom == maxRoom || obj.infinite)
            {
                if (GUI.Button(new Rect(end + horizontalPadding / 4, height, horizontalPadding / 2, heightPerObject), "+"))
                {
                    obj.endRoom = maxRoom + 1;
                }
            }

            height += heightPerObject + objectPadding;
        }
        GUI.EndGroup();

        height = -(int)scrollPosY;
        GUI.BeginGroup(new Rect(startX, verticalPadding + heightPerObject * roomHeightFactor, textWidth, windowSize.height - verticalPadding - heightPerObject * roomHeightFactor));
        for (int i = 0; i < presentationObjects.Length; i++)
        {
            PresentationObject obj = presentationObjects[i];
            if ((index == i && (dragging < 0 || dragging == i)) || dragging == i)
            {
                EditorGUI.DrawRect(new Rect(0, height + heightPerObject / 4.0f, textWidth, heightPerObject / 2.0f), new Color(0.95f, 0.95f, 0.95f));
            }

            GUIStyle style = centeredStyle;
            if (Selection.activeGameObject != null && Selection.activeGameObject.Equals(obj.gameObject))
            {
                style = markedStyle;
            }
            string rName = obj.name.Trim();

            if (rName.Length > 12)
            {
                rName = rName.Substring(0, 6) + "~" + rName.Substring(rName.Length - 5, 5);
            }

            EditorGUI.LabelField(new Rect(0, height, textWidth, heightPerObject), rName, style);

            height += heightPerObject + objectPadding;
        }
        GUI.EndGroup();

    }


    private void CheckMasterLists()
    {
        if (master.audioClips == null)
        {
            Undo.RecordObject(master, "Initialize Audioclips");
            master.audioClips = new List<AudioClip>();
        }
        if (master.triggerPoints == null)
        {
            Undo.RecordObject(master, "Initialize Triggerpoints");
            master.triggerPoints = new List<Collider>();
        }
        if (master.quizObjects == null)
        {
            Undo.RecordObject(master, "Initialize Quizzes");
            master.quizObjects = new List<GameObject>();
        }
        if (master.times == null)
        {
            Undo.RecordObject(master, "Initialize Times");
            master.times = new List<float>();
        }
        if (master.delays == null)
        {
            Undo.RecordObject(master, "Initialize Delays");
            master.delays = new List<float>();
        }
        if (master.userActivated == null)
        {
            Undo.RecordObject(master, "Initialize User activated list");
            master.userActivated = new List<bool>();
        }

        while (master.audioClips.Count < (maxRoom - minRoom + 1))
        {
            Undo.RecordObject(master, "Fill Audioclips");
            master.audioClips.Add(null);
        }
        while (master.triggerPoints.Count < (maxRoom - minRoom + 1))
        {
            Undo.RecordObject(master, "Fill Triggerpoints");
            master.triggerPoints.Add(null);
        }
        while (master.quizObjects.Count < (maxRoom - minRoom + 1))
        {
            Undo.RecordObject(master, "Fill Quizzes");
            master.quizObjects.Add(null);
        }
        while (master.times.Count < (maxRoom - minRoom + 1))
        {
            Undo.RecordObject(master, "Fill Times");
            master.times.Add(1.0f);
        }
        while (master.delays.Count < (maxRoom - minRoom + 1))
        {
            Undo.RecordObject(master, "Fill Delays");
            master.delays.Add(0.0f);
        }
        while (master.userActivated.Count < (maxRoom - minRoom + 1))
        {
            Undo.RecordObject(master, "Fill User activated list");
            master.userActivated.Add(false);
        }
    }


    private void DrawRooms(Rect windowSize)
    {
        int startXScrolled = scrollbarWidth * 2 - (int)scrollPosX + horizontalPadding;
        int startX = scrollbarWidth * 2 + textWidth;
        GUI.BeginGroup(new Rect(startX, 0, windowSize.width - startX, heightPerObject * roomHeightFactor));
        for (int i = minRoom; i <= maxRoom; i++)
        {
            int start = horizontalPadding + (i - minRoom) * objectWidth - (int)scrollPosX;
            EditorGUI.LabelField(new Rect(start + objectWidth * 0.25f, heightPerObject / 6, objectWidth * 0.5f, heightPerObject), i.ToString(), middleStyle);

            if (master != null)
            {

                Undo.RecordObject(master, "Edit Room");
                master.audioClips[i - minRoom] = (AudioClip)EditorGUI.ObjectField(new Rect(start, heightPerObject, objectWidth, heightPerObject * 0.35f), master.audioClips[i - minRoom], typeof(AudioClip), true);
                master.triggerPoints[i - minRoom] = (Collider)EditorGUI.ObjectField(new Rect(start, heightPerObject * 1.35f, objectWidth, heightPerObject * 0.35f), master.triggerPoints[i - minRoom], typeof(Collider), true);
                GameObject curQuizObj = master.quizObjects[i - minRoom];
                GameObject nQuizObject = (GameObject)EditorGUI.ObjectField(new Rect(start, heightPerObject * 1.7f, objectWidth, heightPerObject * 0.35f), curQuizObj, typeof(GameObject), true);
				if (nQuizObject != null) {
					bool hasQuiz = false;
					foreach (Component comp in nQuizObject.GetComponents(typeof(Component))) {
						if (comp.GetType ().GetInterfaces ().Contains (typeof(IQuiz))) {
							hasQuiz = true;
						}
					}
					if (hasQuiz) {
						master.quizObjects [i - minRoom] = nQuizObject;
					}
				} else {
					master.quizObjects [i - minRoom] = null;
				}


                EditorGUI.LabelField(new Rect(start, heightPerObject * 2.05f, objectWidth / 2, heightPerObject * 0.35f), "Time");
                master.times[i - minRoom] = (float)EditorGUI.DoubleField(new Rect(start + objectWidth / 2, heightPerObject * 2.05f, objectWidth / 2, heightPerObject * 0.35f), master.times[i - minRoom]);
                EditorGUI.LabelField(new Rect(start, heightPerObject * 2.4f, objectWidth / 2, heightPerObject * 0.35f), "Delay");
                master.delays[i - minRoom] = EditorGUI.FloatField(new Rect(start + objectWidth / 2, heightPerObject * 2.4f, objectWidth / 2, heightPerObject * 0.35f), master.delays[i - minRoom]);
                EditorGUI.LabelField(new Rect(start, heightPerObject * 2.75f, objectWidth / 4 * 3, heightPerObject * 0.35f), "User activated");
                master.userActivated[i - minRoom] = EditorGUI.Toggle(new Rect(start + objectWidth / 4 * 3, heightPerObject * 2.75f, objectWidth / 4, heightPerObject * 0.35f), master.userActivated[i - minRoom]);
            }
        }
        bool[] roomsMutable = new bool[maxRoom - minRoom + 1];
        for (int i = 0; i < roomsMutable.Length; i++)
        {
            roomsMutable[i] = true;
        }
        foreach (PresentationObject obj in presentationObjects)
        {
            if (obj.startRoom == obj.endRoom || (obj.startRoom == maxRoom && obj.infinite))
            {
                roomsMutable[obj.startRoom - minRoom] = false;
            }
        }
        for (int i = minRoom; i < maxRoom; i++)
        {
            int start = horizontalPadding + (i - minRoom) * objectWidth - (int)scrollPosX + objectWidth / 8 * 3;
            if (roomsMutable[i - minRoom])
            {
                if (GUI.Button(new Rect(start, heightPerObject / 10, objectWidth / 4, heightPerObject / 3), "-"))
                {
                    foreach (PresentationObject obj in presentationObjects)
                    {
                        if (obj.endRoom >= i)
                        {
                            obj.endRoom--;
                        }
                        if (obj.startRoom > i)
                        {
                            obj.startRoom--;
                        }
                    }

                    for (int j = i; j < maxRoom; j++)
                    {
                        master.audioClips[j - minRoom] = master.audioClips[j - minRoom + 1];
                        master.delays[j - minRoom] = master.delays[j - minRoom + 1];
						master.quizObjects[j - minRoom] = master.quizObjects[j - minRoom + 1];
                        master.times[j - minRoom] = master.times[j - minRoom + 1];
                        master.triggerPoints[j - minRoom] = master.triggerPoints[j - minRoom + 1];
                        master.userActivated[j - minRoom] = master.userActivated[j - minRoom + 1];
                    }
                }
            }
            if (i < maxRoom)
            {
                if (GUI.Button(new Rect(start + objectWidth / 2.0f, heightPerObject / 10, objectWidth / 4, heightPerObject / 3), "+"))
                {
                    foreach (PresentationObject obj in presentationObjects)
                    {
                        if (obj.endRoom >= i)
                        {
                            obj.endRoom++;
                        }
                        if (obj.startRoom > i)
                        {
                            obj.startRoom++;
                        }
                    }
					maxRoom++;
                    CheckMasterLists();
                    for (int j = maxRoom; j > i + 1; j--)
                    {
						master.audioClips[j - minRoom] = master.audioClips[j - minRoom - 1];
                        master.delays[j - minRoom] = master.delays[j - minRoom - 1];
						master.quizObjects[j - minRoom] = master.quizObjects[j - minRoom - 1];
                        master.times[j - minRoom] = master.times[j - minRoom - 1];
                        master.triggerPoints[j - minRoom] = master.triggerPoints[j - minRoom - 1];
                        master.userActivated[j - minRoom] = master.userActivated[j - minRoom - 1];
                    }
                    master.audioClips[i - minRoom + 1] = null;
                    master.delays[i - minRoom + 1] = 0;
					master.quizObjects[i - minRoom + 1] = null;
                    master.times[i - minRoom + 1] = 1;
                    master.triggerPoints[i - minRoom + 1] = null;
                    master.userActivated[i - minRoom + 1] = false;
                }
            }
        }
        GUI.EndGroup();
    }

    private void AdjustScrollPos(Rect windowSize)
    {
        Vector2 mousePos = Event.current.mousePosition;
        float mouseDeltaLeft = mousePos.x - scrollbarWidth * 2.0f - textWidth - horizontalPadding;
        float mouseDeltaRight = windowSize.width - mousePos.x;
        double deltaTime = EditorApplication.timeSinceStartup - lastFrameTime;
        if (dragging >= 0)
        {
            if (mouseDeltaLeft < 0)
            {
                scrollPosX += mouseDeltaLeft * (float)deltaTime * 2.0f;
            }
            else if (mouseDeltaRight < 0)
            {
                scrollPosX -= mouseDeltaRight * (float)deltaTime * 2.0f;
            }
        }


        int totalHeight = heightPerObject * (presentationObjects.Length + 1) + headingHeight + verticalPadding * 2 + objectPadding * (presentationObjects.Length + 1);
        if (totalHeight > windowSize.height)
        {
            if (Event.current.isScrollWheel)
            {
                scrollPosY += Event.current.delta.y * 2.0f;
                Event.current.Use();
            }

            scrollPosY = GUILayout.VerticalScrollbar(scrollPosY, windowSize.height, 0, totalHeight, GUILayout.Height(windowSize.height), GUILayout.Width(scrollbarWidth));
        }
        else
        {
            scrollPosY = 0;
        }

        int totalWidth = scrollbarWidth * 2 + textWidth + (maxRoom - minRoom + 1) * objectWidth + 2 * horizontalPadding;
        if (totalWidth > windowSize.width)
        {
            scrollPosX = GUI.HorizontalScrollbar(new Rect(scrollbarWidth * 2 + textWidth, windowSize.height - scrollbarWidth + 6, windowSize.width - scrollbarWidth * 2 - textWidth, scrollbarWidth), scrollPosX, windowSize.width, 0, totalWidth);
        }
        else
        {
            scrollPosX = 0;
        }
    }


    private void RefreshButton()
    {
        if (GUI.Button(new Rect(scrollbarWidth + textWidth * 0.2f, headingHeight * 0.2f, textWidth * 0.6f, headingHeight * 0.6f), "Refresh"))
        {
            UpdatePresentationObjects();
        }
    }


    private void DebugToggle()
    {
        master.debug = GUI.Toggle(new Rect(scrollbarWidth + textWidth * 0.2f, headingHeight * 1.2f, textWidth * 0.6f, headingHeight * 0.4f), master.debug, "Debug");
    }

    private void CheckDragAndDrop(int index, Vector2 mousePos, out float startDelta, out float endDelta)
    {

        int startXScrolled = scrollbarWidth * 2 - (int)scrollPosX + horizontalPadding;
        startDelta = float.PositiveInfinity;
        endDelta = float.PositiveInfinity;
        float mouseRoom = (mousePos.x - (float)startXScrolled - (float)textWidth) / ((float)objectWidth) + ((float)minRoom);
        if (index >= 0)
        {
            PresentationObject curObj = presentationObjects[index];

            startDelta = Mathf.Abs(mouseRoom - (float)curObj.startRoom);
            int rEndRoom = maxRoom;
            if (!curObj.infinite)
            {
                rEndRoom = (int)curObj.endRoom;
            }
            endDelta = Mathf.Abs(mouseRoom - ((float)rEndRoom + 1.0f));
        }


        if (Event.current.rawType.Equals(EventType.MouseDown))
        {
            if (startDelta < 0.05f)
            {
                dragging = index;
                draggingSide = false;
            }
            else if (endDelta < 0.05f)
            {
                dragging = index;
                draggingSide = true;
            }
            Event.current.Use();
        }

        if (startDelta < 0.05f || endDelta < 0.05f || dragging >= 0)
        {
            EditorGUIUtility.AddCursorRect(new Rect(mousePos - Vector2.one * 100, new Vector2(200, 200)), MouseCursor.ResizeHorizontal);

        }
    }


    private void RoomClick(int index, Vector2 mousePos, float startDelta, float endDelta)
    {

        int startX = scrollbarWidth * 2;
        if (mousePos.x < (startX + textWidth) && mousePos.x > startX && index >= 0)
        {
            EditorGUIUtility.AddCursorRect(new Rect(mousePos - Vector2.one * 100, new Vector2(200, 200)), MouseCursor.Link);
        }

        if (Event.current.rawType.Equals(EventType.MouseUp))
        {
            if (mousePos.x < startX + textWidth && mousePos.x > startX && mousePos.y > heightPerObject * roomHeightFactor && dragging < 0)
            {
                if (index >= 0)
                {
                    Selection.activeGameObject = presentationObjects[index].gameObject;
                    EditorApplication.ExecuteMenuItem("Edit/Frame Selected");
                }
            }
            dragging = -1;



            Event.current.Use();
        }

    }


    private void ApplyDragAndDrop(Vector2 mousePos)
    {
        int startXScrolled = scrollbarWidth * 2 - (int)scrollPosX + horizontalPadding;
        float mouseRoom = (mousePos.x - (float)startXScrolled - (float)textWidth) / ((float)objectWidth) + ((float)minRoom);
        if (dragging >= 0)
        {
            PresentationObject curObj = presentationObjects[dragging];
            if (draggingSide)
            {
                int nEnd = Mathf.RoundToInt(mouseRoom) - 1;
                float nDelta = Mathf.Abs(((float)nEnd + 1) - mouseRoom);
                if (nDelta < 0.2f && nEnd >= curObj.startRoom && nEnd <= maxRoom)
                {
                    Undo.RecordObject(curObj, "Change End");
                    curObj.endRoom = nEnd;
                    if (nEnd == maxRoom)
                    {
                        curObj.infinite = true;
                    }
                    else
                    {
                        curObj.infinite = false;
                    }
                }
            }
            else
            {
                int nStart = Mathf.RoundToInt(mouseRoom);
                float nDelta = Mathf.Abs(((float)nStart) - mouseRoom);
                if (nDelta < 0.2f && (nStart <= curObj.endRoom || (curObj.infinite && nStart <= maxRoom)) && nStart >= minRoom)
                {
                    Undo.RecordObject(curObj, "Change Start");
                    curObj.startRoom = nStart;
                }
            }
        }
    }




    void OnGUI()
    {
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            leftMouseDown = true;
        }
        else if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            leftMouseDown = false;
        }
        InitStyle();
        FindMaster();
        PresentationWindow window = (PresentationWindow)EditorWindow.GetWindow(typeof(PresentationWindow), true, "Presentation Editor", false);
        HotkeyCheck(ref window);
        Rect windowSize = window.position;
        RemoveNullObjects();
        if (dragging < 0)
        {
            PresentationUtilities.GetMinMaxRoom(out minRoom, out maxRoom);
        }
        NormalizeMinMaxRoom();
        Vector2 mousePos = Event.current.mousePosition;
        int index = GetMouseIndex(mousePos);
        DrawObjects(index, windowSize);
        CheckMasterLists();
        DrawRooms(windowSize);
        AdjustScrollPos(windowSize);
        RefreshButton();
        DebugToggle();
        if (mousePos.y > heightPerObject * roomHeightFactor)
        {
            float startDelta, endDelta;
            CheckDragAndDrop(index, mousePos, out startDelta, out endDelta);
            RoomClick(index, mousePos, startDelta, endDelta);
        }
        ApplyDragAndDrop(mousePos);

        lastFrameTime = EditorApplication.timeSinceStartup;
        Undo.FlushUndoRecordObjects();
        Repaint();


    }

}
#endif