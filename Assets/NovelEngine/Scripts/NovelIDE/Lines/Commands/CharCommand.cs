using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

namespace novel
{
    [System.Serializable]
    public class CharCommand : CommandLine
    {
        string charName;
        string appearance;
        string transition;
        Vector2? pos;
        float? scale;
        float? time;
        bool? wait;

        CharCommandType charCommandType;
        public CharCommand(int index, string name, string appearance, string transition, Vector2? pos, float? scale,
                        float? time, bool? wait, CharCommandType charCommandType = CharCommandType.Show) : base(index, DialogoueType.CommandLine)
        {
            this.charName = name;
            this.appearance = appearance;
            this.transition = transition;
            this.pos = pos;
            this.scale = scale;
            this.time = time;
            this.wait = wait;
            this.charCommandType = charCommandType;   
        }
        public override async UniTask Execute()
        {
            try
            {
                if (charCommandType == CharCommandType.HideAll)
                {
                    Debug.Log("Hide All 커맨드 실행");

                    foreach (var charObject in NovelManager.Player.currentCharacterDict.Values)
                    {
                        GameObject.Destroy(charObject);
                    }
                    NovelManager.Player.currentCharacterDict.Clear();
                    return;
                }

                NovelCharacterSO charSO = NovelManager.Data.character.GetCharacterByName(charName);
                // SO가 널이면 캐릭터 불러오기 실패
                if (!charSO)
                {
                    Debug.LogError($"{charName} 캐릭터 불러오기 실패");
                    return;
                }
                            switch (charCommandType)
            {
                case CharCommandType.Show:
                    if (NovelManager.Player.currentCharacterDict.ContainsKey(charSO))   // 현재 캐릭터가 이미 띄워져 있는 경우
                    {

                    }
                    else  // 현재 캐릭터가 띄워져 있지 않은 경우
                    {
                        GameObject standingObject = null;
                        Addressables.InstantiateAsync("CharacterStandingBase").Completed += handle =>
                        {
                            if (handle.Status == AsyncOperationStatus.Succeeded)
                            {
                                standingObject = handle.Result;
                                Debug.Log("Prefab Loaded: " + standingObject.name);

                                standingObject.name = charSO.name;


                                standingObject.transform.SetParent(NovelManager.Player.standingPanel.transform, false);
                                GameObject bodyObject = standingObject.transform.GetChild(0).gameObject;
                                GameObject headObject = standingObject.transform.GetChild(1).gameObject;
                            }
                            else
                            {
                                Debug.LogError("Failed to load prefab.");
                            }
                        };
                    }
                    break;
                case CharCommandType.Hide:
                    Debug.Log($"Hide Character : {charName}");
                    if (NovelManager.Player.currentCharacterDict.ContainsKey(charSO))   // 현재 캐릭터가 이미 띄워져 있는 경우
                    {

                    }
                    break;
                case CharCommandType.Fade:
                    if (NovelManager.Player.currentCharacterDict.ContainsKey(charSO))   // 현재 캐릭터가 이미 띄워져 있는 경우
                    {

                    }
                    break;
                case CharCommandType.Effect:
                    // 아직 고려 x
                    break;
                default:
                    Debug.LogError("할당되지 않은 Character 커맨드");
                    break;
            }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }




            





            // 목표료 하는 스탠딩의 게임오브젝트
            //GameObject standingObject = null;




            //bool isWait = wait ?? false;
            //if (NovelPlayer.Instance.currentCharacterDict.ContainsKey(charSO))
            //{
            //    // 지금 언급한 스탠딩이 이미 띄워져 있을 경우


            //    NovelPlayer.Instance.currentCharacterDict.TryGetValue(charSO, out standingObject);

            //    // 이미 떠 있는 스탠딩에 대한 페이드 아웃
            //    if (transition == "fadeout")
            //    {
            //        Image[] images = standingObject.GetComponentsInChildren<Image>();
            //        float fadeTime = time ?? 0;
            //        foreach (var image in images)
            //        {
            //            NovelPlayer.Instance.FadeOut(image, fadeTime, charSO, true);
            //        }



            //    }
            //    else if (time != null)
            //    {
            //        // 이미 떠 있는데 페이드인 설정이 들어간 경우

            //        Image[] images = standingObject.GetComponentsInChildren<Image>(true);

            //        // 일단 투명하게 만들기
            //        foreach(var image in images)
            //        {
            //            Color curColor = image.color;
            //            image.color = new Color(curColor.r, curColor.g, curColor.b, 0f);
            //        }


            //        // 그 후 페이드 인
            //        float fadeTime = time ?? 0;
            //        foreach (var image in images)
            //        {
            //            NovelPlayer.Instance.FadeOut(image, fadeTime, charSO, false);
            //        }
            //    }

            //    // 표정 변화

            //    // appearance가 널이면 기본 표정으로 - 머리오브젝트 끄기
            //    if (appearance == "" || appearance == null)
            //    {
            //        standingObject.transform.GetChild(1).gameObject.SetActive(false);
            //    }
            //    else
            //    {
            //        // 머리 스프라이트 조정
            //        GameObject headObject = standingObject.transform.GetChild(1).gameObject;
            //        headObject.SetActive(true);
            //        Sprite headSprite = charSO.GetHead(appearance);
            //        if (headSprite == null)
            //        {
            //            Debug.LogError($"{charName}의 {appearance} 표정 존재하지 않음");
            //            return;
            //        }
            //        headObject.GetComponent<Image>().sprite = headSprite;
            //        RectTransform headTransform = headObject.GetComponent<RectTransform>();
            //        headTransform.localPosition = charSO.headOffset;
            //        headTransform.sizeDelta = new Vector2(headSprite.rect.width, headSprite.rect.height);

            //    }

            //    return;
            //}
            //else
            //{
            //    // 스탠딩을 새로 띄워야 하는 경우

            //    // 프리팹 인스턴스화
            //    standingObject = GameObject.Instantiate(NovelPlayer.Instance.standingPrefab);
            //    standingObject.name = charSO.name;


            //    GameObject bodyObject = standingObject.transform.GetChild(0).gameObject;
            //    GameObject headObject = standingObject.transform.GetChild(1).gameObject;


            //    // 몸통 스프라이트 조정
            //    Sprite bodySprite = charSO.body;
            //    bodyObject.GetComponent<Image>().sprite = bodySprite;
            //    RectTransform bodyTransform = bodyObject.GetComponent<RectTransform>();
            //    bodyTransform.sizeDelta = new Vector2(bodySprite.rect.width, bodySprite.rect.height);



            //    // 표정이 없으면 몸에 달려있는 기본 표정으로
            //    if (appearance == "" || appearance == null)
            //    {
            //        standingObject.transform.GetChild(1).gameObject.SetActive(false);
            //    }
            //    else
            //    {
            //        // 머리 스프라이트 조정

            //        headObject.SetActive(true);
            //        Sprite headSprite = charSO.GetHead(appearance);
            //        if (headSprite == null)
            //        {
            //            Debug.LogError($"{charName}의 {appearance} 표정 존재하지 않음");
            //            return;
            //        }
            //        headObject.GetComponent<Image>().sprite = headSprite;
            //        RectTransform headTransform = headObject.GetComponent<RectTransform>();
            //        headTransform.localPosition = charSO.headOffset;
            //        headTransform.sizeDelta = new Vector2(headSprite.rect.width, headSprite.rect.height);

            //    }
            //    // 스탠딩 위치조절
            //    standingObject.GetComponent<RectTransform>().position = Vector2.zero;
            //    Vector2 percentPos = pos ?? new Vector2(50, 50);
            //    Vector2 normalizedAnchor = percentPos / 100f;

            //    RectTransform rectTransform = standingObject.GetComponent <RectTransform>();

            //    rectTransform.anchorMin = normalizedAnchor;
            //    rectTransform.anchorMax = normalizedAnchor;
            //    rectTransform.pivot = new Vector2(0.5f, 0.5f);

            //    rectTransform.anchoredPosition = Vector2.zero;

            //    // 처음 띄울때는 투명하게
            //    Image[] images = standingObject.GetComponentsInChildren<Image>(true);
            //    Color startColor = new Color(255f, 255f, 255f, 0f);
            //    foreach(Image image in images)
            //    {
            //        image.color = startColor;
            //    }

            //    // 만약 캐릭터 색깔만 바꾸는 경우가 있다면 수정할것
            //    //Debug.Log(isWait);
            //    float fadeTime = time ?? 0;
            //    foreach (var image in images)
            //    {
            //        NovelPlayer.Instance.FadeOut(image, fadeTime, charSO, false);
            //    }

            //    // 스탠딩 패널에 넣기
            //    standingObject.transform.SetParent(NovelPlayer.Instance.standingPanel.transform, false);

            //    // 현재 나와있는 스탠딩리스트에 추가
            //    NovelPlayer.Instance.currentCharacterDict.Add(charSO, standingObject);
            //}


        }
        public override bool? IsWait()
        {
            return this.wait;
        }
    }
}

