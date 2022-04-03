using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;

public class PresentationMaster : MonoBehaviour
{
    private List<PresentationObject>[] rooms;
    public List<AudioClip> audioClips;
    public List<Collider> triggerPoints;
    public List<GameObject> quizObjects;
    public List<IQuiz> quizzes;
    public List<float> times;
    public List<float> delays;
    public List<bool> userActivated;
    public AudioSource audioSource;
    public bool debug = false;
    public bool enableVREdit = false;

    private int firstRoom;
    private int lastRoom;
    private int curRoom;
    private Camera mainCamera;
    private float roomCountdown = 0.0f;
    private float delayCountdown = 0.0f;
    private bool curRoomShowing = false;
    //private SceneChoice sceneChoice = null;
    private GameObject sceneChoice = null;
    private bool paused = false;
    private int maxVisitedRoom;
    private LightManager lightManager;
    private bool videoPlayingLast = false;
    private MeshRenderer[] playButton;
    private MeshRenderer[] pauseButton;
    private Pulse[] rightArrowPulse;
    private Pulse[] playPulse;
    private bool showingMenu = false;

    private List<GameObject> disabledGameObjects = new List<GameObject>();
    private List<GameObject> enabledGameObjects = new List<GameObject>();
    private List<GameObject> scriptDisabledGameObjects = new List<GameObject>();
    private Vector3 oPosition;
    private Quaternion oRotation;
    private Vector3 oScale;

    private float vibratingCountdown = vibrationDuration;

    private TextMesh debugText;
    private TextMesh roomNumberText;
    private TextMesh assetNameText;
    private int assetIndex = 0;
    private Vector2 lastStickPos;
    private Vector2 lastStickPosL;
    private Vector2 lastStickPosR;


    const float vibrationDuration = 0.5f;
    const float vibrationDelay = 1.0f;

    private GameObject[] resourceModels;
    private List<PresentationObject> editableObjects;
    private GrabVR rightGrab;
    private Transform eyeCam;
    private float lastUpdate = 0f;

    public static T[] FindObjectsOfTypeAll<T>()
    {
        List<T> results = new List<T>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.isLoaded)
            {
                GameObject[] allGameObjects = s.GetRootGameObjects();
                for (int j = 0; j < allGameObjects.Length; j++)
                {
                    GameObject go = allGameObjects[j];
                    results.AddRange(go.GetComponentsInChildren<T>(true));
                }
            }
        }
        return results.ToArray();
    }


    void Awake()
    {
        GameObject cameraRig = GameObject.Find("OVRCameraRig");

        GameObject leftController = cameraRig.transform.Find("TrackingSpace").Find("LeftHandAnchor").Find("LeftControllerAnchor").gameObject;
        GameObject leftSymbols = leftController.transform.Find("controllerSymbols").gameObject;
        GameObject rightController = cameraRig.transform.Find("TrackingSpace").Find("RightHandAnchor").Find("RightControllerAnchor").gameObject;
        GameObject rightSymbols = rightController.transform.Find("controllerSymbols").gameObject;

        this.rightGrab = rightController.GetComponent<GrabVR>();
        this.eyeCam = cameraRig.transform.Find("TrackingSpace").Find("CenterEyeAnchor");

        this.playButton = new MeshRenderer[2];
        this.pauseButton = new MeshRenderer[2];
        if (leftSymbols != null && rightSymbols != null)
        {
            this.playButton[0] = leftSymbols.transform.Find("Play").gameObject.GetComponent<MeshRenderer>();
            this.playButton[1] = rightSymbols.transform.Find("Play").gameObject.GetComponent<MeshRenderer>();
            this.pauseButton[0] = leftSymbols.transform.Find("Pause").gameObject.GetComponent<MeshRenderer>();
            this.pauseButton[1] = rightSymbols.transform.Find("Pause").gameObject.GetComponent<MeshRenderer>();
            this.playButton[0].enabled = false;
            this.playButton[1].enabled = false;

            this.rightArrowPulse = new Pulse[2];
            this.rightArrowPulse[0] = leftSymbols.transform.Find("RightArrow").gameObject.GetComponent<Pulse>();
            this.rightArrowPulse[1] = rightSymbols.transform.Find("RightArrow").gameObject.GetComponent<Pulse>();

            this.playPulse = new Pulse[2];
            this.playPulse[0] = leftSymbols.transform.Find("Play").gameObject.GetComponent<Pulse>();
            this.playPulse[1] = rightSymbols.transform.Find("Play").gameObject.GetComponent<Pulse>();

            //Quick way to disable pause/play button
            this.playButton[0].enabled = false;
            this.playButton[1].enabled = false;
            this.pauseButton[0].enabled = false;
            this.pauseButton[1].enabled = false;
            //////


        }
       

        this.roomNumberText = leftController.transform.Find("RoomNumber").GetComponent<TextMesh>();
        this.assetNameText = rightController.transform.Find("AssetText").GetComponent<TextMesh>();


        this.quizzes = new List<IQuiz>();
        foreach (GameObject quizObj in this.quizObjects)
        {
            this.quizzes.Add(GetQuizFromGameObject(quizObj));
        }

        if (enableVREdit)
        {
            leftSymbols.SetActive(false);
            rightSymbols.SetActive(false);
            this.resourceModels = Resources.LoadAll<GameObject>("Models");
            this.editableObjects = new List<PresentationObject>();
            foreach(PresentationObject obj in GameObject.FindObjectsOfType<PresentationObject>())
            {
                this.editableObjects.Add(obj);                
                GrabObject grabObj = obj.gameObject.AddComponent<GrabObject>();
            }

        }
        else
        {
            this.roomNumberText.gameObject.SetActive(false);
            this.assetNameText.gameObject.SetActive(false);
        }

    }


    private void InitRooms()
    {
        PresentationObject[] objects = Object.FindObjectsOfType<PresentationObject>();

        int minRoom;
        int maxRoom;
        PresentationUtilities.GetMinMaxRoom(out minRoom, out maxRoom);
        foreach (PresentationObject obj in objects)
        {
            obj.HideImmediate();
            if (obj.infinite)
            {
                obj.endRoom = maxRoom;
            }
        }
        this.firstRoom = minRoom;
        this.lastRoom = maxRoom;
        this.maxVisitedRoom = this.firstRoom - 1;
        this.rooms = new List<PresentationObject>[maxRoom - minRoom + 1];


        for (int i = 0; i < this.rooms.Length; i++)
        {
            this.rooms[i] = new List<PresentationObject>();
        }
        foreach (PresentationObject obj in objects)
        {
            for (int r = obj.startRoom; r <= obj.endRoom; r++)
            {
                //Debug.Log (obj.name);
                this.rooms[r - this.firstRoom].Add(obj);
            }
        }

        foreach (Collider collider in this.triggerPoints)
        {
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
    }


    // Use this for initialization
    void Start()
    {

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("EnableOnChoice"))
        {
            enabledGameObjects.Add(obj);
            obj.SetActive(false);
        }

        this.mainCamera = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();
        if (debug)
        {
            Transform cam = this.mainCamera.transform;
            GameObject debugObj = new GameObject("DebugText");
            this.debugText = debugObj.AddComponent<TextMesh>();
            this.debugText.text = "DEBUG";
            debugObj.transform.parent = cam;
            debugObj.transform.localPosition = Vector3.forward * 0.3f + Vector3.right * 0.1f;
            debugObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);//Vector3.one * 0.01f;
            debugObj.transform.localRotation = Quaternion.Euler(0, 90, 0);
        }


        this.lightManager = this.gameObject.AddComponent<LightManager>();
        this.sceneChoice = null;

        InitRooms();

        this.curRoom = this.firstRoom - 1;

        StartModifier modifier = GameObject.FindObjectOfType<StartModifier>();
        if (modifier != null)
        {
            this.curRoom = modifier.startRoom - 1;
            Destroy(modifier);
        }

        
    }

    private static IQuiz GetQuizFromGameObject(GameObject obj)
    {
        if (obj != null)
        {
            foreach (Component comp in obj.GetComponents(typeof(Component)))
            {
                if (comp.GetType().GetInterfaces().Contains(typeof(IQuiz)))
                {
                    return (IQuiz)comp;
                }
            }
        }
        return null;
    }


    public void ShowAllEdit()
    {
        this.lightManager.Reset();
        for (int i = 0; i < this.rooms.Length; i++)
        {
            this.rooms[i].ForEach((o) => o.ShowImmediate());
        }
    }

    public void ChangeRoomEdit(int delta)
    {
        this.lightManager.Reset();
        int realCurRoom = this.curRoom - this.firstRoom;
        if (realCurRoom+delta < this.rooms.Length && realCurRoom+delta>=0)
        {
            this.curRoom+=delta;
            realCurRoom = this.curRoom - this.firstRoom;
            for(int i=0;i<realCurRoom;i++)
            {
                this.rooms[i].ForEach((o) => o.HideImmediate());
            }
            for (int i = realCurRoom+1; i < this.rooms.Length; i++)
            {
                this.rooms[i].ForEach((o) => o.HideImmediate());
            }
            this.rooms[realCurRoom].ForEach((o) => o.ShowImmediate());
        }
    }

    public void ShowRoomEdit(int index)
    {
        this.curRoom = index;
        int realCurRoom = this.curRoom - this.firstRoom;
        for (int i = 0; i < realCurRoom; i++)
        {
            this.rooms[i].ForEach((o) => o.HideImmediate());
        }
        for (int i = realCurRoom + 1; i < this.rooms.Length; i++)
        {
            this.rooms[i].ForEach((o) => o.HideImmediate());
        }
        this.rooms[realCurRoom].ForEach((o) => o.ShowImmediate());
    }


    public void NextRoomImmediate()
    {
        this.lightManager.Reset();
        this.Unpause();
        int realCurRoom = this.curRoom - this.firstRoom;
        //Abbort quiz
        if (realCurRoom >= 0 && this.quizzes[realCurRoom] != null)
        {
            if (this.quizzes[realCurRoom].HasStarted())
            {
                this.quizzes[realCurRoom].StopQuizShow();
            }
        }

        if (realCurRoom < this.rooms.Length - 1)
        {
            ShowNextRoom();
        }
        else if (realCurRoom >= this.rooms.Length - 1 && this.sceneChoice == null)
        {
            //ShowSceneChoice();
        }
    }


    public void LastRoomImmediate()
    {
        this.lightManager.Reset();
        this.Unpause();
        int realCurRoom = this.curRoom - this.firstRoom;
        //Abbort quiz
        if (realCurRoom >= 0 && this.quizzes[realCurRoom] != null)
        {
            if (this.quizzes[realCurRoom].HasStarted())
            {
                this.quizzes[realCurRoom].StopQuizShow();
            }
        }
        if (realCurRoom > 0)
        {
            ShowLastRoom();
        }

    }

    private void ShowLastRoom()
    {
        if (this.sceneChoice == null)
        {
            this.curRoom--;
        }
        this.curRoomShowing = false;
        this.roomCountdown = this.times[this.curRoom - this.firstRoom] + this.delays[this.curRoom - this.firstRoom];
        this.PlayRoomAudio(this.curRoom);
        this.HideNextRoom();
        this.delayCountdown = this.delays[this.curRoom - this.firstRoom];
    }

    private void ShowNextRoom()
    {
        this.vibratingCountdown = vibrationDuration + vibrationDelay;
        this.curRoom++;
        this.curRoomShowing = false;
        this.roomCountdown = this.times[this.curRoom - this.firstRoom] + this.delays[this.curRoom - this.firstRoom];
        this.PlayRoomAudio(this.curRoom);
        this.HideLastRoom();
        this.delayCountdown = this.delays[this.curRoom - this.firstRoom];
    }

    private void ShowSceneChoice()
    {
        this.lightManager.Reset();
        HideCurRoom();
        int realCurRoom = this.curRoom - this.firstRoom;
        if (realCurRoom >= 0 && this.quizzes[realCurRoom] != null)
        {
            if (this.quizzes[realCurRoom].HasStarted())
            {
                this.quizzes[realCurRoom].StopQuizShow();
            }
        }


        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("DisableScriptsOnChoice"))
        {
            scriptDisabledGameObjects.Add(obj);
            foreach (MonoBehaviour script in obj.GetComponents<MonoBehaviour>())
            {
                script.enabled = false;
            }
        }

        this.audioSource.Stop();
        GameObject rig = GameObject.Find("OVRCameraRig");
        this.oPosition = rig.transform.position;
        this.oRotation = rig.transform.rotation;

        rig.transform.position = Vector3.zero;
        rig.transform.rotation = Quaternion.identity;

        if (this.sceneChoice != null)
        {
            this.sceneChoice.gameObject.SetActive(false);
            Destroy(this.sceneChoice);
        }

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("DisableOnChoice"))
        {
            disabledGameObjects.Add(obj);
            obj.SetActive(false);
        }



        foreach (GameObject obj in enabledGameObjects)
        {
            obj.SetActive(true);
        }


        GameObject prefab = GameObject.FindObjectOfType<MenuPrefabManager>().menuPrefab;
        this.sceneChoice = GameObject.Instantiate(prefab);
        this.sceneChoice.transform.position = Vector3.zero;
        this.sceneChoice.transform.rotation = Quaternion.identity;
        this.lightManager.Reset();

    }

    private void HideSceneChoice()
    {
        if (this.sceneChoice != null)
        {
            foreach (GameObject obj in enabledGameObjects)
            {
                obj.SetActive(false);
            }

            GameObject rig = GameObject.Find("OVRCameraRig");
            rig.transform.position = this.oPosition;
            rig.transform.rotation = this.oRotation;

            Destroy(this.sceneChoice);
            foreach (GameObject obj in disabledGameObjects)
            {
                obj.SetActive(true);
            }


            foreach (GameObject obj in scriptDisabledGameObjects)
            {
                foreach (MonoBehaviour script in obj.GetComponents<MonoBehaviour>())
                {
                    script.enabled = true;
                }
            }



            disabledGameObjects.Clear();
            scriptDisabledGameObjects.Clear();
        }
    }

    private void ShowQuiz(int realCurRoom)
    {
        if (!this.quizzes[realCurRoom].HasStarted())
        {
            if (this.quizzes[realCurRoom].HideRoom())
            {
                HideCurRoom();
            }
            this.quizzes[realCurRoom].StartQuizShow();
        }
    }

    public void CheckNextRoom()
    {
        int realCurRoom = this.curRoom - this.firstRoom;
        if (realCurRoom >= 0 && this.quizzes[realCurRoom] != null && !this.quizzes[realCurRoom].IsDone()) //Show quiz if there is one and it's not already done
        {
            ShowQuiz(realCurRoom);
        }
        else if (realCurRoom < this.rooms.Length - 1) //Show next room if there is no quiz or it's done
        {
            this.maxVisitedRoom = Mathf.Max(this.maxVisitedRoom, this.curRoom);
            ShowNextRoom();
        }
        else if (realCurRoom >= this.rooms.Length - 1 && this.sceneChoice == null)
        {
            this.maxVisitedRoom = Mathf.Max(this.maxVisitedRoom, this.curRoom);
        }
    }

    private bool VideoAlreadyLooped()
    {
        int realCurRoom = this.curRoom - this.firstRoom;
        if (realCurRoom < 0)
        {
            return false;
        }
        foreach (PresentationObject obj in this.rooms[realCurRoom])
        {
            if (obj.VideoIsLooping() && obj.AlreadyLooped())
            {
                return true;
            }
        }
        return false;
    }

    private bool IsVideoLooping()
    {
        int realCurRoom = this.curRoom - this.firstRoom;
        if (realCurRoom < 0)
        {
            return false;
        }
        foreach (PresentationObject obj in this.rooms[realCurRoom])
        {
            if (obj.VideoIsLooping())
            {
                return true;
            }
        }
        return false;
    }

    private bool IsVideoPlaying()
    {
        int realCurRoom = this.curRoom - this.firstRoom;
        if (realCurRoom < 0)
        {
            return false;
        }
        foreach (PresentationObject obj in this.rooms[realCurRoom])
        {
            if (obj.IsPlayingVideo())
            {
                return true;
            }
        }
        return false;
    }

    private void HideCurRoom()
    {
        int realCurRoom = this.curRoom - this.firstRoom;
        if (realCurRoom >= 0)
        {
            foreach (PresentationObject obj in this.rooms[realCurRoom])
            {
                obj.Hide();
            }
            if (this.triggerPoints[realCurRoom] != null)
            {
                this.triggerPoints[realCurRoom].enabled = false;
            }
        }
    }

    private void PlayRoomAudio(int i)
    {
        i = i - this.firstRoom;
        this.audioSource.Stop();
        if (this.audioClips[i] != null)
        {
            this.audioSource.clip = this.audioClips[i];
            this.audioSource.PlayDelayed(0.0f);
        }
    }

    private void HideNextRoom()
    {
        int realCurRoom = this.curRoom - this.firstRoom;
        if (realCurRoom + 1 < this.rooms.Length)
        {
            foreach (PresentationObject obj in this.rooms[realCurRoom + 1])
            {
                if (!this.rooms[realCurRoom].Contains(obj))
                {
                    obj.Hide();
                }
            }
            if (this.triggerPoints[realCurRoom + 1] != null)
            {
                this.triggerPoints[realCurRoom + 1].enabled = false;
            }
        }
        if (this.sceneChoice != null)
        {
            this.sceneChoice.GetComponent<PresentationObject>().Hide();
            Destroy(this.sceneChoice);
        }
    }

    public void HideLastRoom()
    {
        int realCurRoom = this.curRoom - this.firstRoom;
        if (realCurRoom - 1 >= 0)
        {
            foreach (PresentationObject obj in this.rooms[realCurRoom - 1])
            {
                if (!this.rooms[realCurRoom].Contains(obj))
                {
                    obj.Hide();
                }
            }
            if (this.triggerPoints[realCurRoom - 1] != null)
            {
                this.triggerPoints[realCurRoom - 1].enabled = false;
            }
        }
    }

    public void ShowRoom(int i)
    {
        i = i - this.firstRoom;

        this.curRoom -= this.firstRoom;
        if (!this.curRoomShowing && i >= 0 && this.rooms.Length > i && this.rooms[i] != null)
        {
            foreach (PresentationObject obj in this.rooms[i])
            {
                if (obj.IsHidden() || !obj.IsShowing())
                {
                    obj.Show();
                }
            }

            if (this.triggerPoints[i] != null)
            {
                this.triggerPoints[i].enabled = false;
            }
            if (this.quizzes[i] != null && this.triggerPoints[i] != null)
            {
                this.triggerPoints[i].enabled = true;
            }
            else if (i + 1 < this.triggerPoints.Count && this.triggerPoints[i + 1] != null)
            {
                this.triggerPoints[i + 1].enabled = true;
            }
        }
        this.curRoomShowing = true;
        this.curRoom = i + this.firstRoom;
    }

    
    private void Unpause()
    {
        
        if (this.playButton != null && this.playButton[0] != null && this.playButton[1] != null)
        {
            //Disabled for now
            /*
            this.playButton[0].enabled = false;
            this.playButton[1].enabled = false;
            this.pauseButton[0].enabled = true;
            this.pauseButton[1].enabled = true;
            this.playPulse[0].End();
            this.playPulse[1].End();
            */
        }

        this.paused = false;
        this.audioSource.UnPause();
        if (this.curRoom - this.firstRoom >= 0 && this.curRoom - this.firstRoom < this.rooms.Length)
        {
            foreach (PresentationObject obj in this.rooms[this.curRoom - this.firstRoom])
            {
                obj.UnpauseVideo();
            }
        }
    }

    
    private void Pause()
    {
        
        if (this.playButton != null && this.playButton[0] != null && this.playButton[1] != null)
        {
            //Disabled for now
            /*
            this.playButton[0].enabled = true;
            this.playButton[1].enabled = true;
            this.pauseButton[0].enabled = false;
            this.pauseButton[1].enabled = false;
            this.playPulse[0].Begin();
            this.playPulse[1].Begin();
            */
        }

        this.paused = true;
        this.audioSource.Pause();
        if (this.curRoom - this.firstRoom >= 0 && this.curRoom - this.firstRoom < this.rooms.Length)
        {
            foreach (PresentationObject obj in this.rooms[this.curRoom - this.firstRoom])
            {
                obj.PauseVideo();
            }
        }
        
    }

    private bool CheckTrigger(bool manualInput)
    {
        if (this.curRoom - this.firstRoom < 0)
        {
            return true;
        }

        if ((this.curRoom - this.firstRoom + 1 < this.userActivated.Count) && this.userActivated[this.curRoom - this.firstRoom + 1])
        {
            if (this.vibratingCountdown > 0.0f)
            {
                if (UnityEngine.XR.XRSettings.enabled && this.vibratingCountdown <= vibrationDuration)
                {
                    /*int indexLeft = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
					SteamVR_Controller.Input(indexLeft).TriggerHapticPulse(500);
                    int indexRight = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
					SteamVR_Controller.Input(indexRight).TriggerHapticPulse(500);*/
                    OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.All);
                    //OVRHaptics.LeftChannel.
                }
                this.vibratingCountdown -= Time.deltaTime;
            }

            if (this.rightArrowPulse != null && this.rightArrowPulse[0] != null && this.rightArrowPulse[1] != null)
            {
                if (!this.rightArrowPulse[0].IsPulsing() && !manualInput)
                {
                    this.rightArrowPulse[0].Begin();
                }
                else if (this.rightArrowPulse[0].IsPulsing() && manualInput)
                {
                    this.rightArrowPulse[0].End();
                }

                if (!this.rightArrowPulse[1].IsPulsing() && !manualInput)
                {
                    this.rightArrowPulse[1].Begin();
                }
                else if (this.rightArrowPulse[1].IsPulsing() && manualInput)
                {
                    this.rightArrowPulse[1].End();
                }
            }
            return manualInput;
        }
        else
        {
            Collider triggerPoint = null;
            if (this.quizzes[this.curRoom - this.firstRoom] != null && !this.quizzes[this.curRoom - this.firstRoom].IsDone())
            {
                triggerPoint = this.triggerPoints[this.curRoom - this.firstRoom];
            }
            else if (this.curRoom - this.firstRoom + 1 < this.triggerPoints.Count)
            {
                if (this.triggerPoints[this.curRoom - this.firstRoom + 1] == null)
                {
                    return true;
                }
                else
                {
                    triggerPoint = this.triggerPoints[this.curRoom - this.firstRoom + 1];
                }
            }
            else
            {
                return true;
            }

            if (triggerPoint == null)
            {
                return true;
            }

            int width = mainCamera.pixelWidth;
            int height = mainCamera.pixelHeight;

            float centralBias = 0.25f;
            float stepSize = 0.05f;

            for (float x = width * centralBias; x <= width * (1.0f - centralBias); x += width * stepSize)
            {
                for (float y = height * centralBias; y <= height * (1.0f - centralBias); y += height * stepSize)
                {
                    Ray ray = mainCamera.ScreenPointToRay(new Vector3(x, y, 0));
                    RaycastHit hitInfo;
                    bool hit = Physics.Raycast(ray, out hitInfo);
                    if (hit)
                    {
                        Collider collider = hitInfo.collider;
                        if (triggerPoint == null || triggerPoint.Equals(collider))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public int GetCurRoom()
    {
        return this.curRoom;
    }

    public int GetNumberOfStageRooms()
    {
        return this.lastRoom;
    }


    private void SaveSceneConfig()
    {
        if(Time.time-lastUpdate>=0.5f) {
            string idStr = "";
            foreach(PresentationObject obj in this.editableObjects)
            {
                idStr+=obj.GetEditId()+"/";
                string dataStr = obj.transform.name+"|"+obj.GetEditId()+"|"+obj.startRoom+"|"+obj.endRoom+"|"+ obj.transform.localPosition.x + "/" + obj.transform.localPosition.y + "/" + obj.transform.localPosition.z+"|"+ obj.transform.localScale.x + "/" + obj.transform.localScale.y + "/" + obj.transform.localScale.z+"|"+ obj.transform.localRotation.x + "/" + obj.transform.localRotation.y + "/" + obj.transform.localRotation.z + "/" + obj.transform.localRotation.w;
                Debug.Log("EDITDATA:" + dataStr);
            }
            idStr = idStr.Substring(0, idStr.Length - 1);
            Debug.Log("EDITIDS:"+idStr);
            lastUpdate = Time.time;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (enableVREdit)
        {
            this.roomNumberText.text = "#" + this.curRoom;
            Vector2 curStickPosL = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            bool leftInputL = (Mathf.Abs(curStickPosL.y) < 0.5 && curStickPosL.x <= -0.7 && lastStickPosL.x > -0.7);
#if UNITY_EDITOR
            leftInputL = leftInputL || Input.GetKeyDown(KeyCode.LeftArrow);
#endif
            bool rightInputL = (Mathf.Abs(curStickPosL.y) < 0.5 && curStickPosL.x >= 0.7 && lastStickPosL.x < 0.7);
#if UNITY_EDITOR
            rightInputL = rightInputL || Input.GetKeyDown(KeyCode.RightArrow);
#endif
            lastStickPosL = curStickPosL;

            Vector2 curStickPosR = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            bool leftInputR = (Mathf.Abs(curStickPosR.y) < 0.5 && curStickPosR.x <= -0.7 && lastStickPosR.x > -0.7);
            bool rightInputR = (Mathf.Abs(curStickPosR.y) < 0.5 && curStickPosR.x >= 0.7 && lastStickPosR.x < 0.7);
            bool bottomInputR = (Mathf.Abs(curStickPosR.x) < 0.5 && curStickPosR.y <= -0.7);
            bool topInputR = (Mathf.Abs(curStickPosR.x) < 0.5 && curStickPosR.y >= 0.7);

            lastStickPosR = curStickPosR;


            //bool leftInput = Input.GetKeyDown(KeyCode.LeftArrow);
            //bool rightInput = Input.GetKeyDown(KeyCode.RightArrow);


            if (rightGrab.IsGrabbingAnything())
            {
                HashSet<GrabObject> grabbedObjects = new HashSet<GrabObject>(rightGrab.GetGrabbedObjects());
                if(topInputR || bottomInputR) { 
                    foreach (GrabObject obj in grabbedObjects)
                    {
                        Bounds boundingBox = obj.gameObject.GetComponent<Renderer>().bounds;
                        foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
                        {
                            boundingBox.Encapsulate(renderer.bounds);
                        }
                        float size = boundingBox.size.x* boundingBox.size.y* boundingBox.size.z;

                        if (topInputR)
                        {
                            float maxFactor = Mathf.Pow(100f / size,1f/3f);
                            float timeFactor = Mathf.Pow(1.00014f, Time.deltaTime*1000f); //~2x scale in 5s
                            float factor = Mathf.Min(maxFactor, timeFactor);
                            obj.transform.localScale *= factor;
                        }
                        else if (bottomInputR)
                        {
                            float minFactor = Mathf.Pow(0.000125f / size, 1f / 3f);
                            float timeFactor = Mathf.Pow(0.99986f, Time.deltaTime * 1000f); //~0.5 scale in 5s
                            float factor = Mathf.Max(minFactor, timeFactor);
                            obj.transform.localScale *= factor;
                        }
                    }
                }
                if(OVRInput.GetDown(OVRInput.Button.Two))
                {
                    foreach (GrabObject obj in grabbedObjects)
                    {
                        int cRoom = this.curRoom;
                        rightGrab.RemoveObject(obj);
                        this.editableObjects.Remove(obj.GetComponent<PresentationObject>());
                        GameObject.Destroy(obj.gameObject);                        
                        InitRooms();
                        int realCurRoom = cRoom - this.firstRoom;
                        if(realCurRoom<0)
                        {
                            this.curRoom = this.firstRoom;
                        }
                        else if(realCurRoom>=this.rooms.Length)
                        {
                            this.curRoom = this.lastRoom;
                        }
                        else
                        {
                            this.curRoom = cRoom;
                        }
                        ShowRoomEdit(this.curRoom);
                    }
                }
            }



            

            if (leftInputL)
            {
                ChangeRoomEdit(-1);
            }
            else if(rightInputL)
            {
                ChangeRoomEdit(1);
            }

            if(leftInputR)
            {
                this.assetIndex--;                
            }
            else if(rightInputR)
            {
                this.assetIndex++;
            }

            if(this.assetIndex<0)
            {
                this.assetIndex += this.resourceModels.Length;
            }
            if(this.assetIndex>=this.resourceModels.Length)
            {
                this.assetIndex -= this.resourceModels.Length;
            }

            this.assetNameText.text = this.resourceModels[this.assetIndex].name;

            if(OVRInput.GetDown(OVRInput.Button.Four))
            {
                ShowAllEdit();
            }
            else if (OVRInput.GetDown(OVRInput.Button.Three))
            {
                ChangeRoomEdit(0);
            }

            bool placeObj = OVRInput.GetDown(OVRInput.Button.One);
#if UNITY_EDITOR
            placeObj = placeObj || Input.GetKeyDown(KeyCode.P);
#endif

            if (placeObj)
            {
                GameObject nObj = GameObject.Instantiate(this.resourceModels[this.assetIndex]);
                nObj.transform.position = rightGrab.transform.position;
                Bounds boundingBox = nObj.gameObject.GetComponent<Renderer>().bounds;
                foreach (Renderer renderer in nObj.GetComponentsInChildren<Renderer>())
                {
                    boundingBox.Encapsulate(renderer.bounds);
                }
                float size = boundingBox.size.x * boundingBox.size.y * boundingBox.size.z;
                float xDist = Mathf.Abs(eyeCam.transform.position.x - rightGrab.transform.position.x);
                float yDist = Mathf.Abs(eyeCam.transform.position.y - rightGrab.transform.position.y);
                float zDist = Mathf.Abs(eyeCam.transform.position.z - rightGrab.transform.position.z);

                float maxFactor = 0f;
                if(xDist>yDist && xDist>zDist) //x is max
                {
                    maxFactor = xDist*0.8f/boundingBox.size.x;
                }
                else if(yDist>xDist && yDist>zDist) //y is max
                {
                    maxFactor = yDist * 0.8f / boundingBox.size.y;
                }
                else //z is max (or they are all the same)
                {
                    maxFactor = zDist * 0.8f / boundingBox.size.z;
                }
                //float minAxisDist = Mathf.Min(xDist, yDist, zDist);
                float minFactor = Mathf.Pow(0.000125f / size, 1f / 3f);
                float normFactor = Mathf.Pow(1f/size,1f/3f);
                float factor = Mathf.Max(Mathf.Min(normFactor, maxFactor),minFactor);
                nObj.transform.localScale *= factor;
                nObj.AddComponent<GrabObject>();
                PresentationObject pObj = nObj.AddComponent<PresentationObject>();
                editableObjects.Add(pObj);
                int cRoom = this.curRoom;
                pObj.startRoom = cRoom;
                pObj.endRoom = cRoom;
                InitRooms();
                ShowRoomEdit(cRoom);
                //pObj.ShowImmediate();


            }

            SaveSceneConfig();

        }
        else
        {
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.All); //Reset vibration, may be activated again in CheckTrigger

            bool rightInput = Input.GetKeyDown(KeyCode.RightArrow);
            bool leftInput = Input.GetKeyDown(KeyCode.LeftArrow);
            //Disabled for now
            bool pauseInput = false;//Input.GetKeyDown(KeyCode.P);
            bool menuInput = Input.GetKeyDown(KeyCode.M);


            if (UnityEngine.XR.XRSettings.enabled)
            {
                /*TrackpadPos pos = TrackpadPos.UNKNOWN;
                if (Input.GetButtonDown("RightTrackpadPress"))
                {
                    pos = ControllerUtils.GetTrackpadPos(ControllerSide.RIGHT);
                }
                else if (Input.GetButtonDown("LeftTrackpadPress"))
                {
                    pos = ControllerUtils.GetTrackpadPos(ControllerSide.LEFT);
                }
                rightInput = pos == TrackpadPos.RIGHT;
                leftInput = pos == TrackpadPos.LEFT;*/
                Vector2 curStickPos = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
                leftInput = (Mathf.Abs(curStickPos.y) < 0.5 && curStickPos.x <= -0.7 && lastStickPos.x > -0.7);
                rightInput = (Mathf.Abs(curStickPos.y) < 0.5 && curStickPos.x >= 0.7 && lastStickPos.x < 0.7);

                pauseInput = OVRInput.GetDown(OVRInput.Button.Three) || OVRInput.GetDown(OVRInput.Button.One);//pos == TrackpadPos.BOTTOM;

                menuInput = OVRInput.GetDown(OVRInput.Button.Start);//Input.GetButtonDown("LeftMenu")||Input.GetButtonDown("RightMenu");

                lastStickPos = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            }

            if (!this.paused)
            {
                this.roomCountdown -= Time.deltaTime;
                this.delayCountdown -= Time.deltaTime;
            }

            if (this.IsVideoPlaying() && !this.videoPlayingLast)
            {
                this.videoPlayingLast = true;
                this.lightManager.Dim(0.1f);
            }
            else if (!this.IsVideoPlaying() && this.videoPlayingLast)
            {
                this.videoPlayingLast = false;
                this.lightManager.Reset();
            }

            bool videoPlaying = IsVideoPlaying();
            bool videoLooping = IsVideoLooping();
            bool triggerReady = !this.audioSource.isPlaying && !videoPlaying && this.roomCountdown <= 0.0f;
            bool trigger = false;
            if (triggerReady && !this.paused)
            {
                trigger = CheckTrigger(rightInput);
                if (trigger)
                {
                    this.CheckNextRoom();
                }

                int realCurRoom = this.curRoom - this.firstRoom;
                if (this.quizzes[realCurRoom] != null && this.quizzes[realCurRoom].IsDone() && realCurRoom + 1 < this.triggerPoints.Count && this.triggerPoints[realCurRoom + 1] != null)
                {
                    this.triggerPoints[realCurRoom + 1].enabled = true;
                }
            }
            else if (videoPlaying && videoLooping && VideoAlreadyLooped())
            {
                this.userActivated[this.curRoom - this.firstRoom + 1] = true;
                trigger = CheckTrigger(rightInput);
                if (trigger)
                {
                    this.CheckNextRoom();
                }

                int realCurRoom = this.curRoom - this.firstRoom;
                if (this.quizzes[realCurRoom] != null && this.quizzes[realCurRoom].IsDone() && realCurRoom + 1 < this.triggerPoints.Count && this.triggerPoints[realCurRoom + 1] != null)
                {
                    this.triggerPoints[realCurRoom + 1].enabled = true;
                }
            }

            if (this.delayCountdown <= 0.0f && !this.curRoomShowing)
            {
                this.ShowRoom(this.curRoom);
            }



            bool userActivated = (this.curRoom - this.firstRoom + 1 < this.userActivated.Count) && this.userActivated[this.curRoom - this.firstRoom + 1];
            bool waitingForUser = userActivated && triggerReady;
            if (rightInput && (this.maxVisitedRoom >= this.curRoom || debug) && !waitingForUser && !trigger && !showingMenu)
            {
                NextRoomImmediate();
            }
            else if (leftInput && !showingMenu)
            {
                LastRoomImmediate();
            }
            if (pauseInput)
            {
                if (showingMenu)
                {
                    this.curRoomShowing = false;
                    this.showingMenu = false;
                    HideSceneChoice();
                    Unpause();
                }
                else
                {
                    if (this.paused)
                    {
                        Unpause();
                    }
                    else
                    {
                        Pause();
                    }
                }
            }

            if (menuInput)
            {
                if (showingMenu)
                {
                    this.curRoomShowing = false;
                    this.showingMenu = false;
                    HideSceneChoice();
                    Unpause();
                }
                else
                {
                    Pause();
                    ShowSceneChoice();
                    this.showingMenu = true;
                }
            }

            if (this.debug && this.debugText != null)
            {
                bool triggerPoint = (this.curRoom - this.firstRoom + 1 < this.triggerPoints.Count) && this.triggerPoints[this.curRoom - this.firstRoom + 1] != null;

                float audioTimeLeft = (this.audioSource != null && this.audioSource.clip != null ? this.audioSource.clip.length - this.audioSource.time : 0.0f);
                this.debugText.text = "Room: " + (this.roomCountdown >= 0.0f ? this.roomCountdown.ToString() : "-") + "\nDelay" + (this.delayCountdown >= 0.0f ? this.delayCountdown.ToString() : "-") + "\nAudio: " + (this.audioSource.isPlaying ? audioTimeLeft.ToString() : "-") + "\nVideo: " + videoPlaying + "\nTrigger: " + triggerPoint + "\nUser: " + userActivated + "\nRoom: " + this.curRoom;

            }
        }
    }
}