using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class Init : MonoBehaviour
{

    public InputField input;
    public TimeKeeper timeKeeper;
    public Text errorText;
    public Text fileErrorText;

    private int startStage;
    private int startRoom;

    // Use this for initialization
    void Start()
    {
        try
        {
            DateTime now = DateTime.Now;
            string testPath = Application.persistentDataPath + "/test_" + now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv";
            File.WriteAllText(testPath, "TEST;TEST;TEST" + Environment.NewLine + "TEST2;TEST2;TEST2");
            if (File.Exists(testPath))
            {
                string content = File.ReadAllText(testPath);
                if (content.Length <= 0)
                {
                    this.fileErrorText.text = "Kann nicht in Datei schreiben";
                }
                else
                {
                    this.fileErrorText.enabled = false;
                    File.Delete(testPath);
                }
            }
            else
            {
                this.fileErrorText.text = "Kann Datei nicht erstellen!";
            }
        }
        catch (Exception e)
        {
            this.fileErrorText.text = "Fehler beim Schreiben: " + e.Message;
        }

        string confText = Resources.Load<TextAsset>("Conf/startat").text;
        string[] splits = confText.Split('|');
        this.startStage = int.Parse(splits[0]);
        this.startRoom = int.Parse(splits[1]);
        if(this.startStage!=1||this.startRoom!=1)
        {
            GameObject modifier = new GameObject("modifier");
            modifier.AddComponent<StartModifier>().startRoom = this.startRoom;
            DontDestroyOnLoad(modifier);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void confirmUserId()
    {
        string idStr = input.text.Trim();
        if (idStr.Length == 0)
        {
            errorText.text = "Ungültige Teilnehmernummer: Leeres Eingabefeld";
        }
        else
        {
			Regex rgx = new Regex("[^a-zA-Z0-9\\-_]");
			idStr = rgx.Replace(idStr, "");
			timeKeeper.setUserId(idStr);
			SceneManager.LoadScene(this.startStage);
        }
    }

}
