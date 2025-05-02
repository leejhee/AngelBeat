using novel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NovelPlayer : MonoBehaviour
{
    public TextAsset novelScript;

    public NovelAct currentAct;

    private bool isFinished = false;

    [SerializeField]
    private GameObject dialoguePanel;
    [SerializeField]
    private TextMeshProUGUI novelText;
    [SerializeField]
    private GameObject namePanel;
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private Button nextButton;

    // Start is called before the first frame update
    void Start()
    {
        var lines = novelScript.text.Split('\n');
        currentAct = NovelParser.Parse(lines);
        nextButton.onClick.AddListener(OnNextLineClicked);
        currentAct.ResetAct();
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
                dialoguePanel.SetActive(false);
                Debug.Log("스크립트 끝까지 플레이");
                return;
            }

            if (line is LabelLine)
                continue;

            PlayLine(line);
            return;
        }
    }


    private void PlayLine(NovelLine line)
    {
        switch (line)
        {
            case NormalLine normal:
                dialoguePanel.SetActive(true);
                namePanel.SetActive(false);
                novelText.text = normal.line;
                break;
            case PersonLine person:
                dialoguePanel.SetActive(true);
                namePanel.SetActive(true);
                nameText.text = person.actorName;
                novelText.text = person.actorLine;
                break;
            case CommandLine command:
                break;
        }
    }

}