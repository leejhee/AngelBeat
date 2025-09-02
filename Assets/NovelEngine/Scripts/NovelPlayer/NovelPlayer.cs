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


    // 이거 나중에 설정 가능하도록 바꾸기
    public float typingSpeed = 0.03f;

    private bool isTyping = false;
    private bool isSubLinePlaying = false;
    public bool isWait = false;
    private bool isCommandRunning = false;  // 얘는 wait와는 다르게 진짜 명령어가 실행중이면 cancel 막기 위해서 필요


    private CancellationTokenSource _typingCts;
    private CancellationToken _destroyToken;
    private CancellationTokenSource _commandCts;

    private void Awake()
    {
        _destroyToken = this.GetCancellationTokenOnDestroy();
    }
    void Start()
    {
        // 초기화 작업
        Init();

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
        if (isCommandRunning)
        {
            _commandCts?.Cancel();
            return;
        }

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

    public void StartWaitForseconds(float time)
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

    public void Resume()
    {
        isWait = false;
        OnNextLineClicked();
    }
    public void OnCommandStart()
    {
        isCommandRunning = true;
        _commandCts?.Cancel();
        _commandCts?.Dispose();
        _commandCts = new CancellationTokenSource();
    }
    public void OnCommandEnd()
    {
        isCommandRunning = false;
        _commandCts?.Cancel();
        _commandCts?.Dispose();
        _commandCts = null;
    }
    public CancellationToken CommandToken => _commandCts != null ? _commandCts.Token : CancellationToken.None;

    private void OnDestroy()
    {
        _typingCts?.Cancel();
        _typingCts?.Dispose();
    }
}