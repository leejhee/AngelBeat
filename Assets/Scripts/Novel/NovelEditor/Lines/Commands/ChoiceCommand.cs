using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace novel
{
    [System.Serializable]
    public class ChoiceCommand : CommandLine
    {
        public string argument;
        
        public ChoiceCommand(int index, string argument) : base(index, DialogoueType.CommandLine)
        {
            this.argument = argument;
        }

        public override void Execute()
        {
            // 배경 프리팹 불러오기
            GameObject choicePrefab = GameObject.Instantiate(NovelPlayer.Instance.choiceButtonPrefab);
            choicePrefab.transform.SetParent(NovelPlayer.Instance.choicePanel.transform);
            choicePrefab.GetComponentInChildren<TextMeshProUGUI>().text = argument;
            Button choiceButton = choicePrefab.GetComponent<Button>();
            choiceButton.onClick.AddListener(() => OnClickChoiceButton());

            NovelPlayer.Instance.currentChoices.Add(this, choicePrefab);
        }
        private void OnClickChoiceButton()
        {
            Debug.Log("선택지 클릭");

            foreach (Transform child in NovelPlayer.Instance.choicePanel.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            NovelPlayer.Instance.Resume();
            // 선택지 밑에 실행하는거 일단 못했음
            //foreach(var line in subLines)
            //{
            //    NovelPlayer.Instance.currentSubLines.Add(line);
            //}

            //foreach(var line in NovelPlayer.Instance.currentSubLines)
            //{
            //    Debug.Log(line);
            //}
        }
        public override bool? IsWait()
        {
            return null;
        }
    }

}

