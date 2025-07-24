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
            // 선택지 프리팹 불러오기
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

            // 선택지 오브젝트 제거
            foreach (Transform child in NovelPlayer.Instance.choicePanel.transform)
            {
                GameObject.Destroy(child.gameObject);
            }


            


            // 선택한 선택지의 서브라인들을 현재 서브라인 리스트에 넣어줌
            foreach (var line in subLines)
            {
                NovelPlayer.Instance.currentSubLines.Add(line);
            }


            NovelPlayer.Instance.SetSublinePlaying();
            NovelPlayer.Instance.Resume();
        }
        public override bool? IsWait()
        {
            return null;
        }
    }

}

