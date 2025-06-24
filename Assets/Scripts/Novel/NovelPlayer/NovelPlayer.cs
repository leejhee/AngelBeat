using novel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class NovelPlayer : MonoBehaviour
{
    public static NovelPlayer Instance {  get; private set; }

    [Header("실행할 노벨 스크립트")]
    public TextAsset novelScript;
    public NovelAct currentAct = new();
    [SerializeField]
    public List<NovelLine> currentSubLines = new();
    public SerializableDict<string, int> labelDict = new SerializableDict<string, int>();
    
     
    private bool isFinished = false;

    [Header("노벨 플레이어 UI 패널")]
    [SerializeField]
    private GameObject _dialoguePanel;
    public GameObject backgroundPanel;
    public GameObject namePanel;
    public GameObject standingPanel;
    public GameObject choicePanel;

    [Header("노벨 플레이어 UI 기타 오브젝트")]
    [SerializeField]
    private TextMeshProUGUI novelText;
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private Button nextButton;

    // 현재 배경화면
    public GameObject currentBackgroundObject;


    // 현재 스탠딩 나와 있는 캐릭터들
    public Dictionary<NovelCharacterSO, GameObject> currentCharacterDict = new();
    //  현재 선택지
    [NonSerialized]
    public SerializableDict<ChoiceCommand, GameObject> currentChoices = new();


    [Header("프리팹")]
    public GameObject backgroundPrefab;
    public GameObject standingPrefab;
    public GameObject choiceButtonPrefab;


    public float typingSpeed = 0.03f;
    private Coroutine typingCoroutine;
    public Coroutine currentCommandCoroutine;
    private bool isTyping = false;
    
    public bool isWait = false;
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
        _dialoguePanel.SetActive(false);

        OnNextLineClicked();
    }
    private IEnumerator NextLine()
    {
        while (true)
        {
            // wait 실행중
            if (isWait)
            {
                yield break;
            }

            var line = currentAct.GetNextLine();

            if (line == null)
            {
                isFinished = true;
                _dialoguePanel.SetActive(false);
                Debug.Log("스크립트 끝까지 플레이");
                yield break;
            }
            // 라벨일 경우
            if (line is LabelLine)
                continue;

            if (line is IExecutable exec)
            {
                // 커맨드일 경우
                exec.Execute();
                //yield return StartCoroutine(ExecuteWithWait(exec));
                continue;
            }
            // 대사나 나래이션
            PlayLine(line);
            yield break;
        }
    }
    //private IEnumerator ExecuteWithWait(IExecutable exec)
    //{
    //    exec.Execute();

    //    while (waitFlag)
    //    {
    //        yield return null;
    //    }
    //    currentCommandCoroutine = null;
    //}

    private void OnNextLineClicked()
    {
        
        // Act가 끝났으면 리턴
        if (isFinished) return;

        // wait 실행중
        if(isWait)
        {
            return;
        }

        // 텍스트 타이핑 중이면 바로 전체 출력
        if (isTyping)
        {
            isTyping = false;
            return;
        }

        // 커맨드 도중이면 중단만 하고 다음 줄 실행은 막기
        //if (waitFlag)
        //{
        //    waitFlag = false;
        //    return;
        //}

        StartCoroutine(NextLine());
    }

    private void PlayLine(NovelLine line)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        _dialoguePanel.SetActive(true);

        switch (line)
        {
            case NormalLine normal:
                namePanel.SetActive(false);
                typingCoroutine = StartCoroutine(TypeText(normal.line));
                break;
            case PersonLine person:
                namePanel.SetActive(true);
                nameText.text = person.actorName;
                typingCoroutine = StartCoroutine(TypeText(person.actorLine));
                break;
        }
    }
    private IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        novelText.text = "";

        foreach (char c in fullText)
        {
            novelText.text += c;
            yield return new WaitForSeconds(typingSpeed);

            if (!isTyping)
            {
                novelText.text = fullText;
                yield break;
            }
        }

        isTyping = false;
    }

    public void FadeOut(Image image, float duration, NovelCharacterSO charSO, bool isFadeOut = true )
    {
        if (image != null)
        {
            NovelPlayer.Instance.currentCommandCoroutine =  StartCoroutine(CharacterFadeOutCoroutine(image, duration, isFadeOut, charSO));
        }
    }
    private IEnumerator CharacterFadeOutCoroutine(Image image, float duration, bool isFadeOut, NovelCharacterSO charSO )
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

                //if (!waitFlag)
                //{
                //    image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
                //    yield break;
                //}
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
                //if (!waitFlag)
                //{
                //    image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
                //    yield break;
                //}
            }
            image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }
    }
    public void BackgroundFadeOut(Image image, float duration, GameObject backObject, bool isFadeOut = true, bool isWait = false)
    {
        if (image != null)
        {
            StartCoroutine(BackgroundFadeOutCoroutine(image, duration, backObject, isFadeOut, isWait));
        }
    }
    private IEnumerator BackgroundFadeOutCoroutine(Image image, float duration, GameObject backObject, bool isFadeOut, bool isWait)
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

    public void Resume()
    {
        isWait = false;
        OnNextLineClicked();
    }

    //[ContextMenu("캐릭터 SO 제작")]
    //public void CreateCharacterSO()
    //{
    //    NovelManager.Instance.CreateCharacterSOAssets();
    //}
}