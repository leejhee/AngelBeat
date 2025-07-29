using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.U2D;

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
            // 선택지 프리팹 불러오기
            GameObject choicePrefab = GameObject.Instantiate(NovelPlayer.Instance.choiceButtonPrefab);
            choicePrefab.transform.SetParent(NovelPlayer.Instance.choicePanel.transform);
            choicePrefab.GetComponentInChildren<TextMeshProUGUI>().text = argument;
            Button choiceButton = choicePrefab.GetComponent<Button>();
            choiceButton.onClick.AddListener(() => OnClickChoiceButton());

            NovelPlayer.Instance.currentChoices.Add(this, choicePrefab);

            Debug.Log($"선택지 프리팹 생성 : {argument}");
        }
        private void OnClickChoiceButton()
        {
            Debug.Log($"선택지 {argument} 클릭");

            // 선택지 오브젝트 제거
            foreach (Transform child in NovelPlayer.Instance.choicePanel.transform)
            {
                GameObject.Destroy(child.gameObject);
            
            }

            if (subLine is CommandLine command)
            {
                command.Execute();
            }
            NovelPlayer.Instance.currentChoices = new();
            NovelPlayer.Instance.Resume();
        }
        public override bool? IsWait()
        {
            return null;
        }
    }

}

