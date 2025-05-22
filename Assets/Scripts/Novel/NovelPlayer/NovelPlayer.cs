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


    public GameObject currentBackgroundObject;

    private List<CommandLine> _waitedCommandLines = new();
    //public List<NovelCharacterSO> currentCharacters = new();
    public Dictionary<NovelCharacterSO, GameObject> currentCharacterDict = new();


    [Header("프리팹")]
    public GameObject backgroundPrefab;
    public GameObject standingPrefab;

    public GameObject standingObject;

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
        foreach (var sprite in so.headDict.Values)
        {
            Debug.Log(sprite);
        }

        //GameObject body = standingObject.transform.GetChild(0).gameObject;
        //GameObject head = standingObject.transform.GetChild(1).gameObject;
        //Debug.Log(body.GetComponent<RectTransform>().anchoredPosition);
        //Debug.Log(head.GetComponent<RectTransform>().anchoredPosition);
        //head.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        //Debug.Log(body.GetComponent<RectTransform>().anchoredPosition);
        //Debug.Log(head.GetComponent<RectTransform>().anchoredPosition);
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
}