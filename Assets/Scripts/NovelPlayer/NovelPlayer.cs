using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace novel
{
    public class NovelPlayer : MonoBehaviour
    {
        public NovelScript nowScript;
        public TMP_Text charNameText;
        public TMP_Text dialogText;
        public Image[] standingList;

        private int curLineIndex = 0;

        public NovelParser parser;

        public GameObject nameObject;

        private void Start()
        {
            if (nowScript == null || nowScript.dialogueLines.Count == 0)
            {
                Debug.LogError("스크립트가 없거나 비어있음");
                return;
            }
            ShowDialogue();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetMouseButtonDown(0))
            {
                NextDialogue();
            }
        }

        private void ShowDialogue()
        {
            if (curLineIndex >= nowScript.dialogueLines.Count)
            {
                Debug.Log("스크립트 끝");
                return;
            }
            DialogueLine line = nowScript.dialogueLines[curLineIndex];

            // 여기서 파싱해야함
            ParseObject parseObject = parser.Parse(line);
            switch (parseObject.command)
            {
                case CommandType.None:
                    if (parseObject.name == "")
                    {
                        nameObject.SetActive(false);
                        dialogText.text = parseObject.text;
                    }
                    else
                    {
                        nameObject.SetActive(true);
                        charNameText.text = parseObject.name;
                        dialogText.text = parseObject.text;
                    }
                    break;
                case CommandType.Background:
                    break;
                case CommandType.BGM:
                    break;
                case CommandType.SFX:
                    break;
                case CommandType.Effect:
                    break;
                case CommandType.ShowCharacter:
                    break;
                case CommandType.HideCharacter:
                    break;
                case CommandType.Clearall:
                    break;
                case CommandType.Choice:
                    break;
                case CommandType.Goto:
                    break;
            }

        }

        private void NextDialogue()
        {
            curLineIndex++;
            if (curLineIndex < nowScript.dialogueLines.Count)
            {
                ShowDialogue();
            }
            else
            {
                Debug.Log("스크립트 플레이 종료");
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 현재 실행 중인 줄의 인덱스를 반환
        /// </summary>
        public int GetCurrentLineIndex()
        {
            return curLineIndex;
        }
    }
}
