using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

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

        public override async UniTask Execute()
        {
            var player = NovelManager.Player;
            var token = player.CommandToken;

            // 선택지 프리팹 불러오기
            GameObject choicePrefab = null;
            try
            {
                var handle = Addressables.InstantiateAsync("ChoiceButtonBase",
                            player.choicePanel.transform);
                choicePrefab = await handle.Task;
                if (choicePrefab == null)
                {
                    Debug.LogError("선택지 프리팹 인스턴스화 실패");
                    return;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return;
            }

            choicePrefab.GetComponentInChildren<TextMeshProUGUI>().text = argument;
            Button choiceButton = choicePrefab.GetComponent<Button>();
            choiceButton.onClick.AddListener(() => OnClickChoiceButton());

            player.currentChoices.Add(this, choicePrefab);

            Debug.Log($"선택지 프리팹 생성 : {argument}");
        }
        private void OnClickChoiceButton()
        {
            Debug.Log($"선택지 {argument} 클릭");

            NovelManager.Player.SetSublinePlaying(subLine);


            // 선택지 오브젝트 제거
            foreach (Transform child in NovelManager.Player.choicePanel.transform)
            {
                child.gameObject.SetActive(false);
                Addressables.ReleaseInstance(child.gameObject);
            }


            NovelManager.Player.currentChoices = new();
            NovelManager.Player.Resume();

        }
    }

}

