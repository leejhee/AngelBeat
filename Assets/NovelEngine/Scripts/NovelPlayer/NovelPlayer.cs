using Cysharp.Threading.Tasks;
using novel;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;

public class NovelPlayer : MonoBehaviour
{
    [Header("실행할 노벨 스크립트")]
    public TextAsset novelScript;
    public NovelAct currentAct = new();
    public NovelEngine.Scripts.SerializableDict<string, int> labelDict = new NovelEngine.Scripts.SerializableDict<string, int>();
    // 현재 실행중인 서브라인
    public NovelLine currentSubline;


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
    public NovelEngine.Scripts.SerializableDict<ChoiceCommand, GameObject> currentChoices = new();



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



    private CancellationTokenSource _typingCts;
    private CancellationToken _destroyToken;

    private void Awake()
    {
        _destroyToken = this.GetCancellationTokenOnDestroy();
    }
    void Start()
    {
        // 초기화 작업
        Init();

        //var testXiaoModel = new CharacterModel(88888888);
        //Party playerParty = new Party(new List<CharacterModel> { testXiaoModel });
        //Debug.Log($"{playerParty.SearchCharacter("샤오").Name}");
    }
    private void Init()
    {
        novelScript = null;
    }
    public void Play()
    {
        // 플레이 해주기 전에 미리 novelScript 세팅해줘야함

        if (novelScript == null)
        {
            Debug.LogError("노벨 스크립트가 설정되지 않음");
            return;
        }

        var lines = novelScript.text.Split('\n');
        currentAct = NovelParser.Parse(lines);

        nextButton.onClick.AddListener(OnNextLineClicked);

        currentAct.ResetAct();
        _dialoguePanel.SetActive(false);
        // 이거 시작 시점 언젠지 상의 필요
        OnNextLineClicked();
    }
    private enum LineResult { Continue, Stop, Finished }
    private async UniTask NextLine()
    {
        var result = await ProcessLine();
        if (result == LineResult.Continue)
        {
            await NextLine();
        }
    }
    private async UniTask<LineResult> ProcessLine()
    {
        if (isWait) return LineResult.Stop;

        if (isSubLinePlaying && currentSubline is CommandLine subCommand)
        {
            await subCommand.Execute();
            isSubLinePlaying = false;
            return LineResult.Continue;
        }

        var line = currentAct.GetNextLine();

        if (line == null)
        {
            isFinished = true;
            _dialoguePanel.SetActive(false);
            Debug.Log("스크립트 끝까지 플레이");
            return LineResult.Finished;
        }

        switch (line)
        {
            case LabelLine label:
                return LineResult.Continue;
            case CommandLine command:
                await command.Execute(); // 반드시 UniTask여야 함
                return LineResult.Continue;
        }
        PlayLine(line);
        return LineResult.Stop;
    }
    private void OnNextLineClicked()
    {
        
        // Act가 끝났으면 리턴
        if (isFinished) return;
        // wait 실행중
        if(isWait) return;

        // 텍스트 타이핑 중이면 바로 전체 출력
        if (isTyping)
        {
            _typingCts?.Cancel();
            return;
        }
        NextLine().Forget();
    }

    private void PlayLine(NovelLine line)
    {
        _dialoguePanel.SetActive(true);

        _typingCts?.Cancel();
        _typingCts?.Dispose();
        _typingCts = CancellationTokenSource.CreateLinkedTokenSource(_destroyToken);

        switch (line)
        {
            case NormalLine normal:
                namePanel.SetActive(false);
                _ = TypeTextAsync(normal.line, _typingCts.Token);
                Debug.Log($"Play Normal Line :  {normal.line} \nIndex : {normal.index}");
                break;
            case PersonLine person:
                namePanel.SetActive(true);
                nameText.text = person.actorName;
                _ = TypeTextAsync(person.actorLine, _typingCts.Token);
                Debug.Log($"Play Person Line :  {person.actorLine} \nIndex : {person.index}");
                break;
        }
    }
    private async UniTask TypeTextAsync(string fullText, CancellationToken token)
    {
        isTyping = true;
        novelText.text = "";
        if (typingSpeed <= 0f)
        {
            novelText.text = fullText;
            isTyping = false;
            return;
        }
        var stringBuilder = new StringBuilder(fullText.Length);

        for (int i = 0; i < fullText.Length; i++)
        {
            if (token.IsCancellationRequested) break;

            stringBuilder.Append(fullText[i]);
            novelText.text = stringBuilder.ToString();

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(typingSpeed), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        // 스킵/완료 모두 최종 전체 출력 보장
        novelText.text = fullText;
        isTyping = false;
    }

    public void SetSublinePlaying(NovelLine line)
    {
        isSubLinePlaying = true;
        currentSubline = line;
    }


    // 연출관련 함수들은 나중에 모듈로 뺄거임
    #region 연출 관련
    public void FadeOut(Image image, float duration, NovelCharacterSO charSO, bool isFadeOut = true )
    {
        if (image != null)
        {
            NovelManager.Player.currentCommandCoroutine =  StartCoroutine(CharacterFadeOutCoroutine(image, duration, isFadeOut, charSO));
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

            if (NovelManager.Player.currentCharacterDict.ContainsKey(charSO))
            {
                GameObject destroyObject = null;
                NovelManager.Player.currentCharacterDict.TryGetValue(charSO, out destroyObject);
                GameObject.Destroy(destroyObject);
                NovelManager.Player.currentCharacterDict.Remove(charSO);
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

            if (NovelManager.Player.currentBackgroundObject != null)
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
    private void OnDestroy()
    {
        _typingCts?.Cancel();
        _typingCts?.Dispose();
    }
}