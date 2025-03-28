using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
//임시 나중에 제거해도 될듯
using UnityEditor;

namespace novel
{
    public class NovelPlayer : MonoBehaviour
    {
        public NovelScript nowScript;
        public TMP_Text charNameText;
        public TMP_Text dialogText;
        public List<Image> standingList;

        private int curLineIndex = 0;

        public NovelParser parser;

        public GameObject nameObject;

        //테스트를 위한 임시 변수
        public NovelName novelName;

        public GameObject backgroundPanel;
        
        public GameObject standingPanel;


        // paths
        private string novelDataPath = "Assets/NovelData/NovelResourceData/";
        private void Start()
        {
            // 테스트용
            if (novelName != NovelName.MaxCount)
            {
                nowScript = AssetDatabase.LoadAssetAtPath<NovelScript>($"Assets/NovelData/NovelScriptData/{novelName.ToString()}.asset");

            }
            //여기까지 테스트

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
        public void SetDialogue(NovelScript script)
        {
            nowScript = script;
            charNameText.text = "";
            dialogText.text = "";
            standingList.Clear();

            curLineIndex = 0;
            ShowDialogue();
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
                    GameObject backgroundObject = AssetDatabase.LoadAssetAtPath<GameObject>($"{novelDataPath}GraphicData/BackgroundData/BackgroundBase.prefab");

                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{novelDataPath}GraphicData/BackgroundData/{parseObject.text}.jpg");
                    if (texture != null)
                    {
                        Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        
                        if (backgroundObject != null)
                        {
                            backgroundObject.GetComponent<Image>().sprite = newSprite;
                            Debug.Log($"{parseObject.text} 배경화면 띄움");
                        }
                        else
                        {
                            Debug.Log("프리팹 인식 안됨");
                            return;
                        }
                    }
                    else
                    {
                        Debug.Log($"{parseObject.text} 배경화면이 존재하지 않음");
                        return;
                    }

                    Instantiate(backgroundObject, backgroundPanel.transform);

                    // 대사창은 꺼야할듯?
                    break;
                case CommandType.BGM:
                    break;
                case CommandType.SFX:
                    break;
                case CommandType.Effect:
                    break;
                case CommandType.ShowCharacter:
                    // parseObject.text 에서 캐릭터 등장 시켜줌
                    NovelCharacter character = AssetDatabase.LoadAssetAtPath<NovelCharacter>($"Assets/NovelData/NovelCharacterData/{parseObject.name}");

                    Debug.Log(parseObject.text);
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

        private void ShowStanding()
        {

        }
    }
}
