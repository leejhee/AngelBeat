using novel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NovelPlayer : MonoBehaviour
{
    public static NovelPlayer Instance {  get; private set; }

    [Header("실행할 노벨 스크립트")]
    public TextAsset novelScript;
    [SerializeField]
    public NovelAct currentAct = new();
    public Dictionary<string, int> labelDict = new Dictionary<string, int>();
     
    private bool isFinished = false;

    [Header("노벨 플레이어 UI 패널")]
    [SerializeField]
    private GameObject _dialoguePanel;
    public GameObject backgroundPanel;
    public GameObject namePanel;
    public GameObject standingPanel;

    [Header("노벨 플레이어 UI 기타 오브젝트")]
    [SerializeField]
    private TextMeshProUGUI novelText;
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private Button nextButton;

    // 현재 배경화면
    public GameObject currentBackgroundObject;

    private List<CommandLine> _waitedCommandLines = new();

    // 현재 스탠딩 나와 있는 캐릭터들
    public Dictionary<NovelCharacterSO, GameObject> currentCharacterDict = new();


    [Header("프리팹")]
    public GameObject backgroundPrefab;
    public GameObject standingPrefab;

    //public GameObject standingObject;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Start()
    {
        var lines = novelScript.text.Split('\n');
        currentAct = NovelParser.Parse(lines);
        nextButton.onClick.AddListener(OnNextLineClicked);
        currentAct.ResetAct();


        NovelCharacterSO so = new NovelCharacterSO();
        NovelManager.Instance.characterSODict.TryGetValue("DonQuixote", out so);

    }
    private void OnNextLineClicked()
    {
        if (isFinished) return;

        while (true)
        {
            var line = currentAct.GetNextLine();

            if (line == null)
            {
                isFinished = true;
                _dialoguePanel.SetActive(false);
                Debug.Log("스크립트 끝까지 플레이");
                return;
            }

            if (line is LabelLine)
                continue;
            else if (line is IExecutable exec)
            {
                exec.Execute();
                continue;
            }

                PlayLine(line);
            return;
        }
    }


    private void PlayLine(NovelLine line)
    {
        switch (line)
        {
            case NormalLine normal:
                _dialoguePanel.SetActive(true);
                namePanel.SetActive(false);
                novelText.text = normal.line;
                break;
            case PersonLine person:
                _dialoguePanel.SetActive(true);
                namePanel.SetActive(true);
                nameText.text = person.actorName;
                novelText.text = person.actorLine;
                break;
        }
    }


    public void FadeOut(Image image, float duration, NovelCharacterSO charSO,bool isFadeOut = true)
    {
        if (image != null)
        {
            StartCoroutine(CharacterFadeOutCoroutine(image, duration, isFadeOut, charSO));
        }
    }
    private IEnumerator CharacterFadeOutCoroutine(Image image, float duration, bool isFadeOut, NovelCharacterSO charSO)
    {
        float counter = 0f;
        Color originalColor = image.color;

        if (isFadeOut)
        {
            while(counter < duration)
            {
                float alpha = Mathf.Lerp(1f, 0f, counter / duration);
                image.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                counter += Time.deltaTime;
                yield return null;
            }
            image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

            if (NovelPlayer.Instance.currentCharacterDict.ContainsKey(charSO))
            {
                GameObject destroyObject = null;
                NovelPlayer.Instance.currentCharacterDict.TryGetValue(charSO, out destroyObject);
                GameObject.Destroy(destroyObject);
                NovelPlayer.Instance.currentCharacterDict.Remove(charSO);
            }
        }
        else
        {
            //페이드 인 정의
            while (counter < duration)
            {
                float alpha = Mathf.Lerp(0f, 1f, counter/ duration);
                image.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                counter += Time.deltaTime;
                yield return null;
            }
            image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }

    }
    public void BackgroundFadeOut(Image image, float duration, GameObject backObject, bool isFadeOut = true)
    {
        if (image != null)
        {
            StartCoroutine(BackgroundFadeOutCoroutine(image, duration, backObject, isFadeOut));
        }
    }
    private IEnumerator BackgroundFadeOutCoroutine(Image image, float duration, GameObject backObject, bool isFadeOut)
    {
        float counter = 0f;
        Color originalColor = image.color;

        if (isFadeOut)
        {
            while (counter < duration)
            {
                float alpha = Mathf.Lerp(1f, 0f, counter / duration);
                image.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                counter += Time.deltaTime;
                yield return null;
            }
            image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

            if (NovelPlayer.Instance.currentBackgroundObject != null)
            {
                currentBackgroundObject = null;
                GameObject.Destroy(backObject);
            }
        }
        else
        {
            //페이드 인 정의
            while (counter < duration)
            {
                float alpha = Mathf.Lerp(0f, 1f, counter / duration);
                image.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                counter += Time.deltaTime;
                yield return null;
            }
            image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }

    }
}