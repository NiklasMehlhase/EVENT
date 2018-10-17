using System.Collections;
using System.Collections.Generic;
//using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChoice : MonoBehaviour
{
    float progress;
    private GameObject back;
    private TextMesh text;
    private List<GameObject> icons;
    private string curCode;
    private string newCode;

    private List<GameObject> disabledObjects;
    private Transform originalCamParent;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;
    private Vector3 originalCamSca;
    private Light choiceLight;
    private GameObject enterButton;
    private List<GameObject> codeComponents;
    private List<GameObject> newCodeComponents;

    private int numberOfHover = 0;
    private int vIcons;
    private int hIcons;
    private float iconSize;

    const float maxHeight = 1f;
    const float maxWidth = 1.5f;
    const float paddingPercentage = 0.2f;

    public static string GetCode(int number, int maxNumber)
    {
        char[] numToChar = { 'W', 'R', 'G', 'B' };
        int numberOfChars = Mathf.CeilToInt(Mathf.Log(maxNumber, 4.0f));
        string code = "";
        for (int i = numberOfChars - 1; i >= 0; i--)
        {
            int cur = Mathf.RoundToInt(Mathf.Pow(4.0f, i));
            if (number >= cur)
            {
                int c = number / cur;
                code += numToChar[c];
                number -= c * cur;
            }
            else
            {
                code += numToChar[0];
            }
        }
        return code;
    }

    public static int GetNumber(string code)
    {
        char[] numToChar = { 'W', 'R', 'G', 'B' };
        int number = 0;
        for (int i = 0; i < code.Length; i++)
        {
            char c = code[i];
            for (int j = 0; j < numToChar.Length; j++)
            {
                if (c == numToChar[j])
                {
                    int cur = Mathf.RoundToInt(Mathf.Pow(4.0f, code.Length - i - 1));
                    number += (cur * j);
                }
            }

        }
        return number;
    }


    Dictionary<int, Texture2D> GetIconTextures()
    {
        Dictionary<int, Texture2D> iconDict = new Dictionary<int, Texture2D>();
        Object[] icons = Resources.LoadAll("Stages");
        foreach (Object icon in icons)
        {
            Texture2D iconTex = (Texture2D)icon;
            string num = "";
            for (int i = 0; i < iconTex.name.Length && char.IsNumber(iconTex.name[i]); i++)
            {
                num += iconTex.name[i];
            }
            int rNum;
            bool parsable = int.TryParse(num, out rNum);
            if (parsable)
            {
                iconDict.Add(rNum, iconTex);
            }
        }

        return iconDict;
    }

    Dictionary<int, string> GetNames()
    {
        Dictionary<int, string> names = new Dictionary<int, string>();


        Object[] icons = Resources.LoadAll("Stages");
        foreach (Object icon in icons)
        {
            Texture2D iconTex = (Texture2D)icon;
            string num = "";
            int i;
            for (i = 0; i < iconTex.name.Length && char.IsNumber(iconTex.name[i]); i++)
            {
                num += iconTex.name[i];
            }
            int rNum;
            bool parsable = int.TryParse(num, out rNum);
            if (parsable)
            {
                names.Add(rNum, iconTex.name.Substring(i));
            }
        }

        return names;
    }

    public void AddToCode(char c, float totalHeight, bool isNew)
    {
        GameObject codeDisplay = GameObject.Instantiate(Resources.Load<GameObject>("Text/TextPrefab"));
        if (isNew)
        {
            this.newCodeComponents.Add(codeDisplay);
        }
        else
        {
            this.codeComponents.Add(codeDisplay);
        }
        codeDisplay.transform.parent = this.gameObject.transform;
        codeDisplay.transform.localPosition = Vector3.up * (totalHeight * 0.5f + 0.5f) + Vector3.right * (0.8f - (isNew ? newCode.Length : curCode.Length) * 0.1f);
        codeDisplay.transform.localScale = Vector3.one * 0.01f;
        codeDisplay.transform.localRotation = Quaternion.Euler(0, 180, 0);
        TextMesh codeMesh = codeDisplay.GetComponent<TextMesh>();
        codeMesh.text = c.ToString();
        switch (c)
        {
            case 'R':
                codeMesh.color = Color.red;
                break;
            case 'G':
                codeMesh.color = Color.green;
                break;
            case 'B':
                codeMesh.color = Color.blue;
                break;
            case 'W':
                codeMesh.color = Color.white;
                break;
        }
        if (isNew)
        {
            newCode += c;
        }
        else
        {
            curCode += c;
        }
    }

    public void LoadCode(string code)
    {
        Debug.Log("Load code " + code);

    }

    public void ShowCodeEnter(float totalHeight)
    {
        this.curCode = "";
        enterButton.SetActive(false);
        foreach (GameObject codeComp in codeComponents)
        {
            codeComp.SetActive(false);
        }
        GameObject redButton = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        redButton.transform.parent = this.transform;
        redButton.transform.localPosition = Vector3.up;
        redButton.transform.localScale = new Vector3(0.3f, 0.1f, 0.3f);
        redButton.transform.localRotation = Quaternion.Euler(90, 0, 0);
        Clickable redClickable = redButton.AddComponent<Clickable>();
        MeshRenderer redRenderer = redButton.GetComponent<MeshRenderer>();
        redRenderer.material.color = Color.red;
        redClickable.setMouseInAction(() => { redRenderer.material.color = new Color(0.5f, 0.0f, 0.0f); });
        redClickable.setMouseOutAction(() => { redRenderer.material.color = new Color(1.0f, 0.0f, 0.0f); });
        redClickable.setClickAction(() => { AddToCode('R', totalHeight, true); });

        GameObject greenButton = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        greenButton.transform.parent = this.transform;
        greenButton.transform.localPosition = Vector3.up * 0.75f - Vector3.right * 0.3f;
        greenButton.transform.localScale = new Vector3(0.3f, 0.1f, 0.3f);
        greenButton.transform.localRotation = Quaternion.Euler(90, 0, 0);
        Clickable greenClickable = greenButton.AddComponent<Clickable>();
        MeshRenderer greenRenderer = greenButton.GetComponent<MeshRenderer>();
        greenRenderer.material.color = Color.green;
        greenClickable.setMouseInAction(() => { greenRenderer.material.color = new Color(0.0f, 0.5f, 0.0f); });
        greenClickable.setMouseOutAction(() => { greenRenderer.material.color = new Color(0.0f, 1.0f, 0.0f); });
        greenClickable.setClickAction(() => { AddToCode('G', totalHeight, true); });

        GameObject blueButton = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        blueButton.transform.parent = this.transform;
        blueButton.transform.localPosition = Vector3.up * 0.75f + Vector3.right * 0.3f;
        blueButton.transform.localScale = new Vector3(0.3f, 0.1f, 0.3f);
        blueButton.transform.localRotation = Quaternion.Euler(90, 0, 0);
        Clickable blueClickable = blueButton.AddComponent<Clickable>();
        MeshRenderer blueRenderer = blueButton.GetComponent<MeshRenderer>();
        blueRenderer.material.color = Color.blue;
        blueClickable.setMouseInAction(() => { blueRenderer.material.color = new Color(0.0f, 0.0f, 0.5f); });
        blueClickable.setMouseOutAction(() => { blueRenderer.material.color = new Color(0.0f, 0.0f, 1.0f); });
        blueClickable.setClickAction(() => { AddToCode('B', totalHeight, true); });

        GameObject whiteButton = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        whiteButton.transform.parent = this.transform;
        whiteButton.transform.localPosition = Vector3.up * 0.5f;
        whiteButton.transform.localScale = new Vector3(0.3f, 0.1f, 0.3f);
        whiteButton.transform.localRotation = Quaternion.Euler(90, 0, 0);
        Clickable whiteClickable = whiteButton.AddComponent<Clickable>();
        MeshRenderer whiteRenderer = whiteButton.GetComponent<MeshRenderer>();
        whiteRenderer.material.color = Color.white;
        whiteClickable.setMouseInAction(() => { whiteRenderer.material.color = new Color(0.5f, 0.5f, 0.5f); });
        whiteClickable.setMouseOutAction(() => { whiteRenderer.material.color = new Color(1.0f, 1.0f, 1.0f); });
        whiteClickable.setClickAction(() => { AddToCode('W', totalHeight, true); });

        GameObject okButton = GameObject.Instantiate(Resources.Load<GameObject>("Text/TextPrefab"));
        okButton.transform.parent = this.gameObject.transform;
        okButton.transform.localPosition = Vector3.up * (totalHeight * 0.5f + 0.7f) - Vector3.right;
        okButton.transform.localScale = Vector3.one * 0.01f;
        okButton.transform.localRotation = Quaternion.Euler(0, 180, 0);
        TextMesh okButtonText = okButton.GetComponent<TextMesh>();
        okButtonText.text = "Ok";
        okButton.AddComponent<BoxCollider>();
        Clickable okClickable = okButton.AddComponent<Clickable>();
        okClickable.setMouseInAction(() => { okButtonText.color = Color.grey; });
        okClickable.setMouseOutAction(() => { okButtonText.color = Color.white; });
        okClickable.setClickAction(() =>
        {
            LoadCode(this.newCode);
        });


        GameObject cancelButton = GameObject.Instantiate(Resources.Load<GameObject>("Text/TextPrefab"));
        cancelButton.transform.parent = this.gameObject.transform;
        cancelButton.transform.localPosition = Vector3.up * (totalHeight * 0.5f + 0.7f) + Vector3.right;
        cancelButton.transform.localScale = Vector3.one * 0.01f;
        cancelButton.transform.localRotation = Quaternion.Euler(0, 180, 0);
        TextMesh cancelButtonText = cancelButton.GetComponent<TextMesh>();
        cancelButtonText.text = "Cancel";
        cancelButton.AddComponent<BoxCollider>();
        Clickable cancelClickable = cancelButton.AddComponent<Clickable>();
        cancelClickable.setMouseInAction(() => { cancelButtonText.color = Color.grey; });
        cancelClickable.setMouseOutAction(() =>
        {
            if (cancelButton != null)
            {
                cancelButtonText.color = Color.white;
            }
        });
        cancelClickable.setClickAction(() =>
        {
            Destroy(redButton);
            Destroy(greenButton);
            Destroy(blueButton);
            Destroy(whiteButton);
            Destroy(okButton);
            foreach (GameObject codeComp in newCodeComponents)
            {
                Destroy(codeComp.gameObject);
            }
            foreach (GameObject codeComp in codeComponents)
            {
                codeComp.SetActive(true);
            }
            enterButton.SetActive(true);
            this.newCode = "";
            Destroy(cancelButton);
        });


    }

    // Use this for initialization
    void Start()
    {


        this.curCode = "";
        this.newCode = "";
        this.codeComponents = new List<GameObject>();
        this.newCodeComponents = new List<GameObject>();
        this.disabledObjects = new List<GameObject>();
        GameObject camRig = GameObject.Find("[CameraRig]");
        this.originalCamParent = camRig.transform.parent;
        this.originalCamPos = camRig.transform.localPosition;
        this.originalCamRot = camRig.transform.localRotation;
        this.originalCamSca = camRig.transform.localScale;
        camRig.transform.parent = null;
        camRig.transform.position = Vector3.zero;
        camRig.transform.rotation = Quaternion.identity;
        camRig.transform.localScale = Vector3.one;
        foreach (GameObject go in Object.FindObjectsOfType<GameObject>())
        {
            if (go.tag.Equals("DisableOnChoice"))
            {
                this.disabledObjects.Add(go);
                go.SetActive(false);
            }
        }



        GameObject mainCamera = GameObject.Find("Camera (eye)");
        Vector3 forward = mainCamera.transform.forward;
        Vector3 up = mainCamera.transform.up;
        if (GameObject.Find("[CameraRig]").transform.position == Vector3.zero)
        {
            forward.y = 0.0f;
            up = Vector3.up;
        }

        this.transform.position = mainCamera.transform.position + forward.normalized - up.normalized * 0.2f;//+new Vector3(0, 1.5f, 0f);
        this.transform.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up) * Quaternion.Euler(0, 180, 0);

        GameObject lightObj = new GameObject("ChoiceLight");
        this.choiceLight = lightObj.AddComponent<Light>();
        this.choiceLight.type = LightType.Directional;
        lightObj.transform.position = Vector3.up;
        lightObj.transform.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
        this.choiceLight.intensity = 0.5f;

        Dictionary<int, Texture2D> textures = GetIconTextures();
        Dictionary<int, string> names = GetNames();

        icons = new List<GameObject>();
        progress = 0.0f;

        int numberOfScenes = SceneManager.sceneCountInBuildSettings;

        int curSceneNumber = SceneManager.GetActiveScene().buildIndex;

        float x = Mathf.Sqrt(((float)numberOfScenes));
        vIcons = 1;
        hIcons = numberOfScenes - 1;
        float hSize = maxWidth / (((float)hIcons) * paddingPercentage + ((float)hIcons) + paddingPercentage);
        float vSize = maxHeight / (((float)vIcons) * paddingPercentage + ((float)vIcons) + paddingPercentage);
        iconSize = Mathf.Min(hSize, vSize);


        float totalWidth = ((float)hIcons) * iconSize + (((float)hIcons) + 1.0f) * iconSize * paddingPercentage;
        float totalHeight = ((float)vIcons) * iconSize + (((float)vIcons) + 1.0f) * iconSize * paddingPercentage;

        this.back = GameObject.CreatePrimitive(PrimitiveType.Cube);
        this.back.transform.parent = this.gameObject.transform;
        this.back.transform.localPosition = Vector3.zero;
        this.back.transform.localScale = new Vector3(totalWidth, totalHeight, 0.1f);
        this.back.transform.localRotation = Quaternion.identity;

        GameObject textObject = GameObject.Instantiate(Resources.Load<GameObject>("Text/TextPrefab"));
        textObject.transform.parent = this.gameObject.transform;
        textObject.transform.localPosition = Vector3.up * (totalHeight * 0.5f + 0.3f);
        textObject.transform.localScale = Vector3.one * 0.01f;
        textObject.transform.localRotation = Quaternion.Euler(0, 180, 0);
        this.text = textObject.GetComponent<TextMesh>();
        this.text.text = "";



        for (int row = 0; row < vIcons; row++)
        {
            for (int col = 0; col < hIcons; col++)
            {
                int i = row * hIcons + col + 1;
                if (i <= numberOfScenes)
                {
                    GameObject nIcon = GameObject.CreatePrimitive(PrimitiveType.Cube);

                    nIcon.name = "Icon" + i;
                    nIcon.transform.parent = this.gameObject.transform;
                    Vector3 pos = new Vector3(-iconSize / 2.0f - paddingPercentage * iconSize - (iconSize * (1.0f + paddingPercentage)) * ((float)col) + totalWidth / 2.0f, -iconSize / 2.0f - paddingPercentage * iconSize - (iconSize * (1.0f + paddingPercentage)) * ((float)row) + totalHeight / 2.0f, 0.05f);
                    GameObject hideCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(hideCube.GetComponent<BoxCollider>());
                    hideCube.transform.parent = nIcon.transform;
                    nIcon.transform.localPosition = pos;
                    nIcon.transform.localScale = new Vector3(iconSize, iconSize, 0.1f);
                    hideCube.transform.localScale = new Vector3(1.01f, 1.01f, 0.99f);
                    nIcon.transform.localRotation = Quaternion.identity;
                    
                    Material iconMat = new Material(Shader.Find("Standard"));
                    Color iconColor = Color.grey;
                    if(i==curSceneNumber)
                    {
                        iconColor = Color.white;
                    }
                    else if(i<curSceneNumber)
                    {
                        iconColor = Color.HSVToRGB(0.3f,0.8f,0.8f);
                    }
                    else if(i==curSceneNumber+1)
                    {
                        iconColor = Color.HSVToRGB(0.6f, 0.8f, 1.0f);
                    }
                    iconMat.SetColor("_Color", iconColor);
                    iconMat.SetFloat("_Glossiness", 0.0f);
                    if (textures.ContainsKey(i))
                    {
                        iconMat.SetTexture("_MainTex", textures[i]);
                    }
                    nIcon.GetComponent<MeshRenderer>().material = iconMat;
                    if (i <= curSceneNumber + 1)
                    {
                        Clickable iconClick = nIcon.AddComponent<Clickable>();
                        iconClick.setMouseInAction(delegate ()
                        {
                            nIcon.transform.localPosition = pos - Vector3.forward * 0.025f;
                            string name = names.ContainsKey(i) ? names[i] : "Unnamed";
                            this.numberOfHover++;
                            this.text.transform.localPosition = Vector3.up * (totalHeight * 0.5f + 0.2f) + new Vector3(pos.x + iconSize / 2.0f, 0, 0);
                            this.text.text = name;
                        });
                        iconClick.setMouseOutAction(delegate ()
                        {
                            this.numberOfHover--;
                            if (nIcon != null)
                            {
                                nIcon.transform.localPosition = pos;

                                if (numberOfHover <= 0)
                                {
                                    this.text.text = "";
                                }
                            }
                        });
                        iconClick.setClickAction(delegate ()
                        {
                            nIcon.transform.localPosition = pos - Vector3.forward * 0.025f;
                            SceneManager.LoadScene(i);
                        });
                    }
                    this.icons.Add(nIcon);
                }
            }
        }
    }

    public void Reset()
    {
        GameObject camRig = GameObject.Find("[CameraRig]");
        camRig.transform.parent = this.originalCamParent;
        camRig.transform.localPosition = this.originalCamPos;
        camRig.transform.localScale = this.originalCamSca;
        camRig.transform.localRotation = this.originalCamRot;
        this.choiceLight.gameObject.SetActive(false);
        Destroy(this.choiceLight.gameObject);
        foreach (GameObject go in this.disabledObjects)
        {
            go.SetActive(true);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
