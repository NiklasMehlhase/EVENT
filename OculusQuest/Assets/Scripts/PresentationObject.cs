using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System.Linq;
using Unity.Collections;

public class MaterialUtils
{

    public static void SetRenderQueue(ref Material material, int queue)
    {
        material.renderQueue = queue;
    }

    public static void SetToFade(ref Material material, bool isText = false)
    {
        //Setting material to fade mode

        material.SetFloat("_Mode", 2.0f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 1); //Important for face sorting
        material.SetInt("_ZTest", 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        if (isText)
        {
            material.renderQueue = 3001;
        }
        else
        {
            material.renderQueue = 3000;
        }
    }

    public static void SetToTransparent(ref Material material)
    {
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 1); //Important for face sorting
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
    }

    public static void IncrementRenderQueue(ref Material material)
    {
        int queue = material.renderQueue;
        queue++;
        material.renderQueue = queue;
    }
}

public class PresentationUtilities
{
    public static void GetMinMaxRoom(out int minRoom, out int maxRoom)
    {
        minRoom = int.MaxValue;
        maxRoom = int.MinValue;
        foreach (PresentationObject obj in Object.FindObjectsOfType<PresentationObject>())
        {
            minRoom = Mathf.Min(minRoom, obj.startRoom);
            maxRoom = Mathf.Max(maxRoom, obj.startRoom);
            if (obj.endRoom > obj.startRoom)
            {
                maxRoom = Mathf.Max(maxRoom, (int)obj.endRoom);
            }

        }
    }
}



[System.Serializable, ExecuteAlways]
public class PresentationObject : MonoBehaviour
{
    [SerializeField]
    public int startRoom;
    [SerializeField]
    public int endRoom;
    [SerializeField]
    public bool fall;
    [SerializeField]
    public bool infinite;
    [SerializeField,HideInInspector]
    private int editId = 0;

    private Vector3 targetPosition;
    private static Dictionary<GameObject, List<Material>> materialMap;
    private bool showing;
    //private List<Material> materials;
    private List<Light> lights;
    private List<float> lightIntensities;
    private List<VideoPlayer> videoClips;
    private bool isActive;
    private bool makesUpdates;
    private bool startedPlayingVideo;
    private bool videoLooped;

    private const float showSpeed = 4.0f;
    private const float vanishingHeight = 5.0f;
    private const float epsilon = 0.001f;

    private List<PresentationObject> other;

    void Reset()
    {
        EnsureUniqueId();
        int minRoom;
        int maxRoom;
        PresentationUtilities.GetMinMaxRoom(out minRoom, out maxRoom);
        this.startRoom = maxRoom + 1;
        this.infinite = true;
    }

    static PresentationObject()
    {
        PresentationObject.materialMap = new Dictionary<GameObject, List<Material>>();
    }

    public int GetEditId()
    {
        return this.editId;
    }

    public void SetEditId(int id)
    {
        this.editId = id;
    }

    private void EnsureUniqueId()
    {
        PresentationObject[] objects = Object.FindObjectsOfType<PresentationObject>();
        int maxId = 0;
        bool idUsed = false;
        foreach (PresentationObject obj in objects)
        {
            if (!obj.Equals(this))
            {
                maxId = Mathf.Max(maxId, obj.editId);
                if (obj.editId == this.editId)
                {
                    idUsed = true;
                }
            }
        }

        if (this.editId == 0 || idUsed)
        {
            this.editId = maxId + 1;
        }

    }

    // Use this for initialization
    void Awake()
    {
        if (Application.isPlaying)
        {
            this.startedPlayingVideo = false;
            this.targetPosition = this.transform.position;
            bool isText = this.GetComponent<TextMesh>() != null;
            if (!materialMap.ContainsKey(this.gameObject))
            {
                materialMap.Add(this.gameObject, new List<Material>());
                MeshRenderer[] renderers = this.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer renderer in renderers)
                {
                    Material[] nMaterials = new Material[renderer.materials.Length];

                    for (int i = 0; i < renderer.materials.Length; i++)
                    {
                        Material newMaterial = new Material(renderer.materials[i]);
                        MaterialUtils.SetToFade(ref newMaterial, isText);
                        nMaterials[i] = newMaterial;
                        materialMap[this.gameObject].Add(newMaterial);
                    }
                    renderer.materials = nMaterials;
                }
            }
            this.lights = new List<Light>();
            this.lightIntensities = new List<float>();
            foreach (Light light in this.GetComponentsInChildren<Light>())
            {
                this.lights.Add(light);
                this.lightIntensities.Add(light.intensity);
            }
            this.isActive = true;
            this.videoClips = new List<VideoPlayer>();
            foreach (VideoPlayer videoClip in this.GetComponentsInChildren<VideoPlayer>())
            {
                videoClip.playOnAwake = false;
                this.videoClips.Add(videoClip);
                videoClip.loopPointReached += VideoEndReached;
                videoClip.Stop();
            }

            this.other = this.GetComponents<PresentationObject>().ToList();
            this.other.Remove(this);
            if (this.other == null)
            {
                this.other = new List<PresentationObject>();
            }
            this.makesUpdates = false;
        }
        
        EnsureUniqueId();
        
    }

    public void VideoEndReached(UnityEngine.Video.VideoPlayer player)
    {
        this.videoLooped = true;
    }

    public bool VideoIsLooping()
    {
        foreach (VideoPlayer videoPlayer in this.videoClips)
        {
            if (videoPlayer.isLooping)
            {
                return true;
            }
        }
        return false;
    }

    public bool AlreadyLooped()
    {
        return this.videoLooped;
    }


    public bool IsPlayingVideo()
    {
        foreach (VideoPlayer videoPlayer in this.videoClips)
        {
            if (videoPlayer.isPlaying || startedPlayingVideo)
            {
                return true;
            }
        }
        return false;
    }

    public void PauseVideo()
    {
        foreach (VideoPlayer videoPlayer in this.videoClips)
        {
            videoPlayer.Pause();
        }
    }

    public void UnpauseVideo()
    {
        foreach (VideoPlayer videoPlayer in this.videoClips)
        {
            videoPlayer.Play();
        }
    }

    public bool IsShowing()
    {
        return this.showing;
    }


    public void Show()
    {
        DeactivateOthers();

        this.SetActiveUpdate(true);
        this.showing = true;
        if (fall)
        {
            this.transform.position = this.targetPosition + Vector3.up * vanishingHeight;
        }

        foreach (Material mat in materialMap[this.gameObject])
        {
            if (mat.HasProperty("_Color"))
            {
                Color col = mat.color;
                col.a = 0.0f;
                mat.color = col;
            }
        }
        foreach (Light light in this.lights)
        {
            light.intensity = 0.0f;
        }
        foreach (VideoPlayer videoPlayer in this.videoClips)
        {
            startedPlayingVideo = true;
            this.videoLooped = false;
            videoPlayer.Play();
        }
    }

    public bool IsHidden()
    {


        foreach (Renderer renderer in this.GetComponentsInChildren<Renderer>())
        {
            if (renderer.enabled)
            {
                return false;
            }
        }

        foreach (Light light in this.GetComponentsInChildren<Light>())
        {
            if (light.enabled)
            {
                return false;
            }
        }

        return true;
    }

    public void Hide()
    {
        DeactivateOthers();

        this.showing = false;
    }

    public void HideImmediate()
    {
        DeactivateOthers();

        this.showing = false;
        this.SetActiveUpdate(false);
    }

    public void DeactivateOthers()
    {
        this.makesUpdates = true;
        if (this.other != null)
        {
            foreach (PresentationObject obj in this.other)
            {
                obj.makesUpdates = false;
            }
        }
    }

    public void ShowImmediate()
    {
        DeactivateOthers();
        this.showing = true;
        this.SetActiveUpdate(true);
    }

    private void SetActiveUpdate(bool active)
    {
        if (!Application.isPlaying || active != this.isActive)
        {
            foreach (Renderer renderer in this.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = active;
            }

            foreach (Collider collider in this.GetComponentsInChildren<Collider>())
            {
                collider.enabled = active;
            }

            foreach (Light light in this.GetComponentsInChildren<Light>())
            {
                light.enabled = active;
            }

            this.isActive = active;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying)
        {
            if (makesUpdates)
            {

                foreach (VideoPlayer videoPlayer in this.videoClips)
                {
                    if (videoPlayer.isPlaying)
                    {
                        startedPlayingVideo = false;
                    }
                }
                List<Material> materials = materialMap[this.gameObject];
                if (showing)
                {

                    foreach (Material mat in materials)
                    {
                        if (mat.HasProperty("_Color"))
                        {
                            if (mat.color.a < 1.0f)
                            {
                                Color col = mat.color;
                                col.a = Mathf.Clamp(col.a + Time.deltaTime * showSpeed, 0.0f, 1.0f);
                                mat.color = col;
                            }
                        }
                    }

                    for (int i = 0; i < this.lights.Count; i++)
                    {
                        if (this.lights[i].intensity < this.lightIntensities[i])
                        {
                            float step = this.lightIntensities[i] * showSpeed;
                            this.lights[i].intensity = Mathf.Clamp(this.lights[i].intensity + Time.deltaTime * step, 0.0f, this.lightIntensities[i]);
                        }
                    }

                    if (this.fall && Mathf.Abs(this.transform.position.y - this.targetPosition.y) > epsilon)
                    {
                        Vector3 pos = this.transform.position;
                        pos.y -= Time.deltaTime * showSpeed * vanishingHeight;
                        if (pos.y < targetPosition.y)
                        {
                            pos.y = targetPosition.y;
                        }
                        this.transform.position = pos;
                    }
                }
                else
                {
                    bool allTransparent = true;
                    foreach (Material mat in materials)
                    {
                        if (mat.HasProperty("_Color"))
                        {
                            if (mat.color.a > 0.0f)
                            {
                                Color col = mat.color;
                                col.a = Mathf.Clamp(col.a - Time.deltaTime * showSpeed, 0.0f, 1.0f);
                                mat.color = col;
                                allTransparent = false;
                            }
                        }
                    }

                    for (int i = 0; i < this.lights.Count; i++)
                    {
                        if (this.lights[i].intensity > 0.0f)
                        {
                            allTransparent = false;
                            float step = this.lightIntensities[i] * showSpeed;
                            this.lights[i].intensity = Mathf.Clamp(this.lights[i].intensity - Time.deltaTime * step, 0.0f, this.lightIntensities[i]);
                        }
                    }

                    if (this.fall && Mathf.Abs(this.transform.position.y - this.targetPosition.y) < vanishingHeight)
                    {
                        Vector3 pos = this.transform.position;
                        pos.y += Time.deltaTime * showSpeed * vanishingHeight;
                        if (pos.y > this.targetPosition.y + vanishingHeight)
                        {
                            pos.y = this.targetPosition.y + vanishingHeight;
                        }
                        this.transform.position = pos;
                    }

                    if (allTransparent && (!this.fall || Mathf.Abs(this.transform.position.y - this.targetPosition.y) >= vanishingHeight))
                    {
                        this.SetActiveUpdate(false);
                    }
                }
            }
        }
    }
}
