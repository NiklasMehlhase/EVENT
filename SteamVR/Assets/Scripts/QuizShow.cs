using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuizShow : MonoBehaviour
{
    public AudioPlaylist textAudioSource;
    public AudioSource effectAudioSource;
    public AudioClip rightEffect;
    public AudioClip wrongEffect;
    public Vector3 size;
    public float maxFontSize = 0.2f;
    public float minFontSize = 0.05f;
    private GameObject screen;
    private List<GameObject> oldScreens;
    private Quiz quiz;
    private List<GameObject> answerObjects;
    private GameObject explenationObject;
    public Material fontMaterial;
    public Font font;
    private GameObject progressBar;
    private Vector2 screenExtents;
    private float targetProgress;
    private float curProgress;
    private MeshRenderer progressBarRenderer;
    private TextMesh progressText;

    private static float speed = 50.0f;
    private static float flyDistance = 50.0f;
    private static float progressSpeed = 1.0f;
    private TimeKeeper timeKeeper;

    private float targetRotation;

    
    // Use this for initialization
    void Start()
    {
        this.timeKeeper = GameObject.FindObjectOfType<TimeKeeper>();
        this.oldScreens = new List<GameObject>();
        this.answerObjects = new List<GameObject>();
        this.quiz = this.GetComponent<Quiz>();
        this.targetRotation = this.transform.rotation.eulerAngles.y;

        MaterialUtils.SetToFade(ref this.fontMaterial);
        MaterialUtils.IncrementRenderQueue(ref this.fontMaterial);
        this.screenExtents = new Vector2(size.x, size.y);

		if (this.effectAudioSource != null) {
			this.effectAudioSource.volume = 0.5f;
		}

    }


    private void InitProgress()
    {
        this.progressBar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        this.progressBar.transform.parent = this.gameObject.transform;
        this.progressBar.transform.localPosition = new Vector3(0.0f, screenExtents.y / 1.5f, 0.0f);
        this.progressBar.transform.localScale = Vector3.one * screenExtents.y * 0.15f;
        this.progressBar.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 90.0f);
        this.progressBarRenderer = this.progressBar.GetComponent<MeshRenderer>();
        GameObject textObject = new GameObject("ProgressText");
        textObject.transform.parent = this.gameObject.transform;
        textObject.transform.localPosition = new Vector3(0.0f, screenExtents.y, 0.0f);
        textObject.transform.localScale = Vector3.one * screenExtents.y * 0.015f;
        textObject.transform.localRotation = Quaternion.identity;
        this.progressText = textObject.AddComponent<TextMesh>();
        this.progressText.font = this.font;
        this.progressText.color = Color.black;
        textObject.GetComponent<MeshRenderer>().material = this.fontMaterial;
        this.progressText.text = "0 / " + this.quiz.GetNumberOfQuestions();

        this.curProgress = 0.0f;
        SetProgressBar(0.0f);
    }

    public void StartQuiz()
    {		
		InitProgress();
        this.quiz.startQuiz();
        this.showQuestion(quiz.getCurrentQuestion());

    }

    public void Stop()
    {
        if (this.screen != null)
        {
            this.oldScreens.Add(this.screen);
        }
        if (this.textAudioSource != null)
        {
            this.textAudioSource.Stop();
        }
        this.targetProgress = 0.0f;
        this.progressText.text = "";

    }

    private void SetProgressBar(float progress)
    {
        progress = Mathf.Clamp(progress, 0.0f, 1.0f);
        float maxScale = screenExtents.x / 2.0f;
        this.progressBar.transform.localScale = new Vector3(this.progressBar.transform.localScale.x, maxScale * progress, this.progressBar.transform.localScale.z);
        this.progressBar.transform.localPosition = new Vector3(-maxScale * (1.0f - progress), this.progressBar.transform.localPosition.y, this.progressBar.transform.localPosition.z);
        if (progress > 0.0f)
        {
            this.progressBarRenderer.enabled = true;
        }
        else
        {
            this.progressBarRenderer.enabled = false;
        }
    }

    void showCurrentQuestion()
    {
        this.targetRotation = this.transform.rotation.eulerAngles.y;
    }

    void showQuestion(Question question)
    {
        float progress = ((float)this.quiz.GetCurQuestionNumber()) / ((float)this.quiz.GetNumberOfQuestions());
        if ((this.quiz.GetCurQuestionNumber() <= this.quiz.GetNumberOfQuestions() && this.quiz.GetNumberOfQuestions()>1) || (this.quiz.GetCurQuestionNumber() < this.quiz.GetNumberOfQuestions()))
        {
            this.targetProgress = progress;
            this.progressText.text = this.quiz.GetCurQuestionNumber() + " / " + this.quiz.GetNumberOfQuestions();
        }
        else
        {
            this.targetProgress = 0.0f;
            this.progressText.text = "";
        }
        this.oldScreens.Add(this.screen);
        this.screen = null;
        if (this.textAudioSource != null)
        {
            this.textAudioSource.Stop();
        }
        if (question != null)
        {
            List<Pair<int, Pair<string, string>>> answers = question.getRandomAnswers();
            if (this.textAudioSource != null && question.questionAudio != null)
            {
                this.textAudioSource.Clear();
                this.textAudioSource.AddClip(question.questionAudio, 1.0f);
                foreach(Pair<int, Pair<string, string>> answer in answers)
                {
					if(answer.First==0 && question.rightAnswerAudio!=null)
                    {
                        this.textAudioSource.AddClip(question.rightAnswerAudio,1.0f);
                    }
					else if(question.answersAudio!=null && answer.First>0 && answer.First-1<question.answersAudio.Count && question.answersAudio[answer.First-1]!=null)
                    {
                        this.textAudioSource.AddClip(question.answersAudio[answer.First-1], 1.0f);
                    }
                }                
            }
            this.targetRotation = this.transform.rotation.eulerAngles.y;
            this.screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            this.screen.transform.localScale = this.size;
            Collider collider = this.screen.GetComponent<Collider>();
            if (collider != null)
            {
                BoxCollider boxCollider = (BoxCollider)collider;
                Vector3 size = boxCollider.size;
                size.z = 0.2f;
                boxCollider.size = size;

            }
            MeshRenderer renderer = this.screen.GetComponent<MeshRenderer>();
            Material nMaterial = new Material(renderer.material);
            MaterialUtils.SetToFade(ref nMaterial);
            renderer.material = nMaterial;
            Vector3 globalPos = this.transform.position;
            this.screen.transform.localPosition = globalPos + this.transform.right * flyDistance;
            for (int i = 0; i < this.screen.transform.childCount; i++)
            {
                GameObject child = this.screen.transform.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }

            addText(question.questionText, screenExtents.x * 0.9f, screenExtents.y * 0.31f, -0.45f, 0.45f);
            



            float heightPerAnswer = (screenExtents.y * 0.57f) / ((float)answers.Count);
            float yPerAnswer = 0.5f / ((float)answers.Count);
            float minSize = float.MaxValue;
            for (int i = 0; i < answers.Count; i++)
            {
                string text = "- " + answers[i].Second.First;
                GameObject gameObject = new GameObject("TestText");
                gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                gameObject.transform.parent = this.screen.transform;
                gameObject.transform.localPosition = new Vector3(0, 0, -0.55f);
                MeshRenderer testRenderer = gameObject.AddComponent<MeshRenderer>();
                TextMesh textComp = gameObject.AddComponent<TextMesh>();
                textComp.font = this.font;
                textComp.color = Color.black;
                testRenderer.material = this.fontMaterial;

                adjustTextSize(ref textComp, text, screenExtents.x * 0.9f, heightPerAnswer, minFontSize, maxFontSize);
                minSize = Math.Min(minSize, textComp.characterSize);
                Destroy(gameObject);
            }


            for (int i = 0; i < answers.Count; i++)
            {
                float height = 0.1f - ((float)i) * yPerAnswer;
                addTextAnswer(answers[i], screenExtents.x * 0.9f, heightPerAnswer, -0.45f, height,minSize);
            }
            this.screen.transform.rotation = this.transform.rotation;
        }
    }

    void showExplenation(string text,bool rightAnswer=false)
    {
        this.targetRotation = this.screen.transform.rotation.eulerAngles.y + 180.0f;
        addExplenation(text+"\n\nHier klicken um weiter zu kommen", screenExtents.x * 0.9f, screenExtents.y * 0.85f, 0.45f, 0.45f,rightAnswer);
    }

    void answer(int id)
    {
        Question curQuestion = quiz.getCurrentQuestion();
        if (id == 0)
        {
            if (this.effectAudioSource != null)
            {
                this.effectAudioSource.Stop();
                this.effectAudioSource.clip = rightEffect;
                this.effectAudioSource.Play();
            }
            if (curQuestion.rightExplenation.Trim().Length > 0)
            {
                showExplenation(curQuestion.rightExplenation, true);
                if (this.textAudioSource != null && curQuestion.rightExplenationAudio != null)
                {
                    this.textAudioSource.Stop();
                    this.textAudioSource.SetClip(curQuestion.rightExplenationAudio);
                    this.textAudioSource.PlayDelayed(1.0f);
                }
            }
            else
            {
                showQuestion(this.quiz.nextQuestion());
            }
            

        }
        else
        {
            string answerId = curQuestion.wrongAnswers[id - 1];
            if(answerId.Length>20)
            {
                answerId = answerId.Substring(0, 20);
            }

            if(this.timeKeeper!=null) { 
                this.timeKeeper.incWrongAnswers(this.name,this.quiz.GetCurQuestionNumber().ToString(),answerId);
            }

            if (curQuestion.explenations[id - 1].Trim().Length > 0)
            {
                showExplenation(curQuestion.explenations[id - 1]);
                if (this.textAudioSource != null && curQuestion.explenationsAudio[id - 1] != null)
                {
                    this.textAudioSource.Stop();
                    this.textAudioSource.SetClip(curQuestion.explenationsAudio[id - 1]);
                    this.textAudioSource.PlayDelayed(1.0f);
                }
            }
        }

    }

    private static void adjustTextSize(ref TextMesh textComp, string text, float width, float height, float minFontSize, float maxFontSize)
    {
		Quaternion oRotation = textComp.transform.localRotation;
		Quaternion oParentRotation = textComp.transform.parent.localRotation;
		textComp.transform.parent.rotation = Quaternion.identity;
		textComp.transform.rotation = Quaternion.identity;

        MeshRenderer renderer = textComp.gameObject.GetComponent<MeshRenderer>();
        textComp.characterSize = maxFontSize;
        textComp.text = text;
        string[] words = text.Split(' ');
        List<string> lines = new List<string>();
        int lineIndex = 0;
        do
        {

            lines.Clear();
            lineIndex = 0;
            lines.Add("");
            for (int i = 0; i < words.Length; i++)
            {
                textComp.text = lines[lineIndex] + words[i];
                if (renderer.bounds.extents.x * 2.0f > width)
                {
                    lineIndex++;
                    lines.Add(words[i] + " ");
                }
                else
                {
                    lines[lineIndex] += words[i] + " ";
                }
            }

            textComp.text = "";
            for (int i = 0; i < lines.Count; i++)
            {
                textComp.text += lines[i];
                if (i < lines.Count - 1)
                {
                    textComp.text += "\n";
                }
            }

            if (renderer.bounds.extents.y * 2.0f > height || renderer.bounds.extents.x * 2.0f > width)
            {
                textComp.characterSize = textComp.characterSize - 0.01f;
            }
        } while (textComp.characterSize > 0.0f && (renderer.bounds.extents.y * 2.0f > height || renderer.bounds.extents.x * 2.0f > width));
        textComp.characterSize = Mathf.Max(textComp.characterSize, minFontSize);

        lines.Clear();
        lineIndex = 0;
        lines.Add("");
        for (int i = 0; i < words.Length; i++)
        {
            textComp.text = lines[lineIndex] + words[i];
            if (renderer.bounds.extents.x * 2.0f > width)
            {
                lineIndex++;
                lines.Add(words[i] + " ");
            }
            else
            {
                lines[lineIndex] += words[i] + " ";
            }
        }

        textComp.text = "";
        for (int i = 0; i < lines.Count; i++)
        {
            textComp.text += lines[i];
            if (i < lines.Count - 1)
            {
                textComp.text += "\n";
            }
        }

		textComp.transform.localRotation = oRotation;
		textComp.transform.parent.localRotation = oParentRotation;

    }


    void addText(string answer, float width, float height, float x, float y)
    {
        GameObject gameObject = new GameObject("Text");
        gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        gameObject.transform.parent = this.screen.transform;
        gameObject.transform.localPosition = new Vector3(x, y, -0.55f);
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        TextMesh textComp = gameObject.AddComponent<TextMesh>();
        textComp.font = this.font;

        textComp.color = Color.black;
        renderer.material = this.fontMaterial;
        adjustTextSize(ref textComp, answer, width, height, minFontSize, maxFontSize);


        this.answerObjects.Add(gameObject);
    }

    void addExplenation(string text, float width, float height, float x, float y, bool rightAnswer = false)
    {
		GameObject textGameObject = new GameObject("Text");
        textGameObject.transform.parent = this.screen.transform;
        textGameObject.transform.localRotation = Quaternion.identity;
        textGameObject.transform.localScale = new Vector3(-0.005f, 0.005f, 0.005f);
        textGameObject.transform.localPosition = new Vector3(x, y, 0.55f);
        MeshRenderer renderer = textGameObject.AddComponent<MeshRenderer>();
        TextMesh textComp = textGameObject.AddComponent<TextMesh>();
        textComp.text = text;
        textComp.font = this.font;
        textComp.color = Color.black;
        renderer.material = this.fontMaterial;
		adjustTextSize(ref textComp, text, width, height, minFontSize, maxFontSize * 10.0f);

        this.explenationObject = textGameObject;

        BoxCollider collider = textGameObject.AddComponent<BoxCollider>();
		collider.size = new Vector3(1.0f/textGameObject.transform.localScale.x, 1.0f/textGameObject.transform.localScale.y, 1.0f);
		collider.center = new Vector3(textGameObject.transform.localPosition.y/textGameObject.transform.localScale.y,textGameObject.transform.localPosition.x/textGameObject.transform.localScale.x,0);
        Clickable clickable = textGameObject.AddComponent<Clickable>();
        if (rightAnswer)
        {
            clickable.setClickAction(delegate ()
            {
                this.explenationObject.SetActive(false);
                Destroy(this.explenationObject);
                if (this.textAudioSource != null)
                {
                    this.textAudioSource.Stop();
                }
                showQuestion(this.quiz.nextQuestion());
            });
        }
        else
        {
            clickable.setClickAction(delegate ()
            {
                this.explenationObject.SetActive(false);
                Destroy(this.explenationObject);
                if (this.textAudioSource != null)
                {
                    this.textAudioSource.Stop();
                }
                this.showCurrentQuestion();
            });
        }
        clickable.setMouseInAction(delegate ()
        {
				textComp.color = Color.blue;
        });
        clickable.setMouseOutAction(delegate ()
        {
            if (textComp != null)
            {
                textComp.color = Color.black;
            }
        });
    }

    void addTextAnswer(Pair<int, Pair<string, string>> answer, float width, float height, float x, float y,float size=0.0f)
    {
        GameObject gameObject = new GameObject("Text");
        gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        gameObject.transform.parent = this.screen.transform;
        gameObject.transform.localPosition = new Vector3(x, y, -0.55f);
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        TextMesh textComp = gameObject.AddComponent<TextMesh>();
        textComp.text = answer.Second.First;
        textComp.font = this.font;
        textComp.color = Color.black;
        renderer.material = this.fontMaterial;
        if(size==0.0f) { 
            adjustTextSize(ref textComp, "- " + answer.Second.First, width, height, minFontSize, maxFontSize);
        }
        else
        {
            adjustTextSize(ref textComp, "- " + answer.Second.First, width, height, size, size);
        }
        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        Clickable clickable = gameObject.AddComponent<Clickable>();
        clickable.setClickAction(delegate ()
        {
            this.answer(answer.First);
        });
        clickable.setMouseInAction(delegate ()
        {
				textComp.color = Color.blue;
        });
        clickable.setMouseOutAction(delegate ()
        {
            textComp.color = Color.black;
        });
        this.answerObjects.Add(gameObject);
    }

    private static void SetAlpha(GameObject screen, float nAlpha)
    {
        MeshRenderer renderer = screen.GetComponent<MeshRenderer>();
        Color col = renderer.material.color;
        col.a = nAlpha;
        renderer.material.color = col;

        foreach (Text text in screen.GetComponentsInChildren<Text>())
        {
            Color txtCol = text.color;
            txtCol.a = nAlpha;
            text.color = txtCol;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (this.curProgress != this.targetProgress)
        {
            if (Mathf.Abs(this.curProgress - this.targetProgress) <= Time.deltaTime * progressSpeed)
            {
                this.curProgress = this.targetProgress;
            }
            else
            {
                this.curProgress += Mathf.Sign(this.targetProgress - this.curProgress) * Time.deltaTime * progressSpeed;
            }
            SetProgressBar(this.curProgress);
        }


        if (this.screen != null)
        {
            this.screen.transform.rotation = Quaternion.Lerp(this.screen.transform.rotation, Quaternion.Euler(this.screen.transform.rotation.x, this.targetRotation, this.screen.transform.rotation.z), 5.0f * Time.deltaTime);
        }
        List<GameObject> removables = new List<GameObject>();
        foreach (GameObject oldScreen in oldScreens)
        {
            if (oldScreen != null)
            {
                Vector3 oldPos = oldScreen.transform.localPosition;
                Vector3 targetPos = this.transform.position - flyDistance * this.transform.right;

                float distancePercentage = Vector3.Distance(oldPos, targetPos) / flyDistance;
                SetAlpha(oldScreen, distancePercentage);

                if (Vector3.Distance(oldPos, targetPos) < Time.deltaTime * speed)
                {
                    oldPos = targetPos;
                }
                else
                {
                    oldPos -= Time.deltaTime * speed * this.transform.right;
                }
                oldScreen.transform.localPosition = oldPos;
                if (Vector3.Distance(oldPos, targetPos) < 0.01f)
                {
                    oldScreen.SetActive(false);
                    removables.Add(oldScreen);
                    Destroy(oldScreen.gameObject);
                }
            }
            else
            {
                removables.Add(oldScreen);
            }
        }

        foreach (GameObject removed in removables)
        {
            this.oldScreens.Remove(removed);
        }

        if (this.screen != null)
        {
            Vector3 oldPos2 = this.screen.transform.localPosition;
            Vector3 targetPos = this.transform.position;
            float distancePercentage = 1.0f - Vector3.Distance(oldPos2, targetPos) / flyDistance;
            SetAlpha(this.screen, distancePercentage);

            if (Vector3.Distance(oldPos2, targetPos) < Time.deltaTime * speed)
            {
                oldPos2 = targetPos;
            }
            else
            {
                oldPos2 -= Time.deltaTime * speed * this.transform.right;
            }
            this.screen.transform.localPosition = oldPos2;
        }
    }
}
