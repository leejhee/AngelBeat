using Character;
using Core.Foundation.Utils;
using GamePlay.Character;
using novel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class NovelPlayer : MonoBehaviour
{

    //public static NovelPlayer Instance {  get; private set; }

    [Header("실행할 노벨 스크립트")]
    public TextAsset novelScript;
    public NovelAct currentAct = new();
    public SerializableDict<string, int> labelDict = new SerializableDict<string, int>();
    // 현재 실행중인 서브라인
    public List<NovelLine> currentSublines = new();


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
    public SerializableDict<ChoiceCommand, GameObject> currentChoices = new();



    [Header("프리팹")]
    public GameObject backgroundPrefab;
    public GameObject standingPrefab;
    public GameObject choiceButtonPrefab;


    public float typingSpeed = 0.03f;
    private Coroutine typingCoroutine;
    public Coroutine currentCommandCoroutine;

    private bool isTyping = false;
    private bool isSubLinePlaying = false;
    public bool isWait = false;

    private void Awake()
    {
        //if (Instance != null && Instance != this)
        //{
        //    Destroy(gameObject);
        //    return;
        //}
        //Instance = this;
    }
    void Start()
    {
        var lines = novelScript.text.Split('\n');
        currentAct = NovelParser.Parse(lines);
        nextButton.onClick.AddListener(OnNextLineClicked);
        currentAct.ResetAct();
        _dialoguePanel.SetActive(false);

        OnNextLineClicked();


        var testXiaoModel = new CharacterModel(88888888);
        Party playerParty = new Party(new List<CharacterModel> { testXiaoModel });
        Debug.Log($"{playerParty.SearchCharacter("샤오").Name}");



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

            if (isSubLinePlaying)
            {
                if (currentSublines.Count > 0)
                {
                    NovelLine subline = currentSublines[0];

                    if (subline is IExecutable execSub)
                    {
                        // 커맨드일 경우
                        execSub.Execute();
                        currentSublines.RemoveAt(0);
                        continue;
                    }

                    // 대사나 나래이션
                    PlayLine(subline);

                    currentSublines.RemoveAt(0);
                    yield break;
                }
                else
                {
                    isSubLinePlaying = false;
                    continue;
                }
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
                continue;
            }
            // 대사나 나래이션
            PlayLine(line);
            yield break;
        }
    }

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
                Debug.Log($"Play Normal Line :  {normal.line} \nIndex : {normal.index}");
                break;
            case PersonLine person:
                namePanel.SetActive(true);
                nameText.text = person.actorName;
                typingCoroutine = StartCoroutine(TypeText(person.actorLine));
                Debug.Log($"Play Person Line :  {person.actorLine} \nIndex : {person.index}");
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

    public void SetSublinePlaying(List<NovelLine> lines)
    {
        isSubLinePlaying = true;
        currentSublines = lines;
    }


    // 연출관련 함수들은 나중에 모듈로 뺄거임
    #region 연출 관련
    public void FadeOut(Image image, float duration, NovelCharacterSO charSO, bool isFadeOut = true )
    {
        if (image != null)
        {
            NovelManager.novelPlayer.currentCommandCoroutine =  StartCoroutine(CharacterFadeOutCoroutine(image, duration, isFadeOut, charSO));
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

            }
            image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

            if (NovelManager.novelPlayer.currentCharacterDict.ContainsKey(charSO))
            {
                GameObject destroyObject = null;
                NovelManager.novelPlayer.currentCharacterDict.TryGetValue(charSO, out destroyObject);
                GameObject.Destroy(destroyObject);
                NovelManager.novelPlayer.currentCharacterDict.Remove(charSO);
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

            if (NovelManager.novelPlayer.currentBackgroundObject != null)
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
    public void StartWait(float time)
    {
        StartCoroutine(WaitCoroutine(time));
    }
    private IEnumerator WaitCoroutine(float time)
    {
        isWait = true;

        float counter = 0f;
        while (counter < time)
        {
            Debug.Log($"{counter}초째 기다리는중");
            counter += Time.deltaTime;
            yield return null;
        }

        isWait = false;
        OnNextLineClicked();
    }
    #endregion


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