using Cysharp.Threading.Tasks;
using novel;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;

public class NovelPlayer : MonoBehaviour
{
    private TextAsset novelScript;
    private NovelAct currentAct = new();
    public NovelEngine.Scripts.SerializableDict<string, int> labelDict { get; private set; }
    private bool isFinished = false;

    // 자식 오브젝트들
    public GameObject dialoguePanel { get; private set; }
    public GameObject backgroundPanel { get; private set; }
    public GameObject namePanel { get; private set; }
    public GameObject standingPanel { get; private set; }
    public GameObject choicePanel { get; private set; }
    public GameObject blurPanel { get; private set; }
    public TextMeshProUGUI novelText { get; private set; }
    public TextMeshProUGUI nameText { get; private set; }
    public Button nextButton { get; private set; }




    // 나중에 private로 돌릴것들

    // 현재 배경화면
    public GameObject currentBackgroundObject { get; private set; }

    // 현재 스탠딩 나와 있는 캐릭터들
    public Dictionary<NovelCharacterSO, GameObject> currentCharacterDict = new();
    //  현재 선택지
    public NovelEngine.Scripts.SerializableDict<ChoiceCommand, GameObject> currentChoices = new();



    // 이거 나중에 설정 가능하도록 바꾸기
    public float typingSpeed = 0.03f;
    private bool isTyping = false;

    // 실행시킬 서브라인
    public NovelLine currentSubline;
    private bool isSubLinePlaying = false;

    // 일정 시간동안 멈추는 기능
    private bool _isWaitForTime = false;
    // @wait 커맨드로 완전히 멈추기
    private bool _isHardWait = false;


    private bool isCommandRunning = false;  // 얘는 wait와는 다르게 진짜 명령어가 실행중이면 cancel 막기 위해서 필요


    private CancellationTokenSource _typingCts;
    private CancellationToken _destroyToken;

    private CancellationTokenSource _commandCts;
    private int _runningCommandCount = 0; // 동시 커맨드 수


    private bool _isPumping;

    private void Awake()
    {
        _destroyToken = this.GetCancellationTokenOnDestroy();
        Init();
    }
    private void Init()
    {
        novelScript = null;
        FindObjectsByType();
        labelDict = new();
    }
    private void FindObjectsByType()
    {
        var NovelObjects = GetComponentsInChildren<NovelObjects>(true);
        foreach (var obj in NovelObjects)
        {
            switch (obj.GetNovelObjectType())
            {
                case NovelObjectType.Blur:
                    blurPanel = obj.gameObject;
                    break;
                case NovelObjectType.BackgroundPanel:
                    backgroundPanel = obj.gameObject;
                    break;
                case NovelObjectType.StandingPanel:
                    standingPanel = obj.gameObject;
                    break;
                case NovelObjectType.DialogPanel:
                    dialoguePanel = obj.gameObject;
                    break;
                case NovelObjectType.NovelText:
                    novelText = obj.GetComponent<TextMeshProUGUI>();
                    break;
                case NovelObjectType.NamePanel:
                    namePanel = obj.gameObject;
                    break;
                case NovelObjectType.NameText:
                    nameText = obj.GetComponent<TextMeshProUGUI>();
                    break;
                case NovelObjectType.NextButton:
                    nextButton = obj.GetComponent<Button>();
                    break;
                case NovelObjectType.ChoicePanel:
                    choicePanel = obj.gameObject;
                    break;
            }
        }
    }
    public void SetScript(TextAsset text)
    {
        novelScript = text;
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

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextLineClicked);
        }


        currentAct.ResetAct();
        dialoguePanel.SetActive(false);
        // 이거 시작 시점 언젠지 상의 필요
        OnNextLineClicked();
    }
    private void OnNextLineClicked()
    {
        // Act가 끝났으면 리턴
        if (isFinished) return;

        // @wait 커맨드로 멈춤
        if (_isHardWait) return;

        if (isCommandRunning)
        {
            _commandCts?.Cancel();
        }

        // 시간이 정해진 @wait 커맨드
        if (_isWaitForTime) return;

        // 텍스트 타이핑 중이면 바로 전체 출력
        if (isTyping)
        {
            _typingCts?.Cancel();
            return;
        }

        if (!_isPumping)
        {
            _isPumping = true;
            NextLine().Forget();
        }
    }

    private async UniTask NextLine()
    {
        bool keepPumping = false;
        try
        {
            var result = await ProcessLine();
            if (result == LineResult.Continue)
            {
                keepPumping = true;
                NextLine().Forget();
                return;
            }
        }
        finally
        {
            if (!keepPumping)
                _isPumping = false;
        }
    }
    private enum LineResult { Continue, Stop, Finished }

    private async UniTask<LineResult> ProcessLine()
    {
        if (_isHardWait) return LineResult.Stop;

        if (isSubLinePlaying && currentSubline is CommandLine subCommand)
        {
            RunCommandLine(subCommand);
            isSubLinePlaying = false;
            return LineResult.Continue;
        }

        var line = currentAct.GetNextLine();

        if (line == null)
        {
            isFinished = true;
            dialoguePanel.SetActive(false);
            Debug.Log("스크립트 끝까지 플레이");
            return LineResult.Finished;
        }

        switch (line)
        {
            case LabelLine label:
                return LineResult.Continue;
            case CommandLine command:
                if (command is WaitCommand wait)
                {
                    RunCommandLine(command);
                    return LineResult.Stop;
                }
                RunCommandLine(command);
                return LineResult.Continue;
        }
        PlayLine(line);
        return LineResult.Stop;
    }
    private void RunCommandLine(CommandLine command)
    {
        OnCommandStart();
        RunCommandLineAsync(command).Forget();
    }
    private async UniTaskVoid RunCommandLineAsync(CommandLine command)
    {
        try
        {
            await command.Execute().AttachExternalCancellation(CommandToken);   // 현재 실행되고 있는 모든 커맨드에 대한 공유 토큰
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Command Cancelled");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Command Execution Error: {ex.Message}");
        }
        finally
        {
            OnCommandEnd();
        }
    }

    // 텍스트 패널에 있는 텍스트들 플레이 해주는 함수
    private void PlayLine(NovelLine line)
    {
        dialoguePanel.SetActive(true);

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
    // 타이핑 하는 연출
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
    public void SetHardWait (bool value)
    {
        _isHardWait = value;
    }
    public void SetWaitForTime(bool value)
    {
        _isWaitForTime = value;
    }
    public void ContinueFromWait() => OnNextLineClicked();
    public void Resume()
    {
        _isWaitForTime = false;
        _isHardWait = false;
        OnNextLineClicked();
    }
    public void OnCommandStart()
    {
        _runningCommandCount++;
        
        // 가장 처음 실행되는 커맨드에 대해서만 취소 토큰 생성
        if (_runningCommandCount == 1)
        {
            isCommandRunning = true;
            _commandCts?.Cancel();
            _commandCts?.Dispose();
            _commandCts = new CancellationTokenSource();
        }
    }
    public void OnCommandEnd()
    {
        _runningCommandCount = Math.Max(0, _runningCommandCount - 1);
        if (_runningCommandCount == 0)
        {
            isCommandRunning = false;
            _commandCts?.Cancel();   // 모두 끝났을 때 정리 겸 cancel
            _commandCts?.Dispose();
            _commandCts = null;
        }
    }

    public void SetBackground(GameObject background)
    {
        currentBackgroundObject = background;
    }
    public CancellationToken CommandToken => _commandCts != null ? _commandCts.Token : CancellationToken.None;

    private void OnDestroy()
    {
        _typingCts?.Cancel();
        _typingCts?.Dispose();
    }
}