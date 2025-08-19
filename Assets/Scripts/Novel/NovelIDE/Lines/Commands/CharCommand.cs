using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
        public CharCommand(int index, string name, string appearance, string transition, Vector2? pos, float? scale, float? time, bool? wait, int depth = 0, CharCommandType charCommandType = CharCommandType.Show) : base(index, DialogoueType.CommandLine, depth)
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
        public override void Execute()
        {
            // 노벨매니저 모노비헤이비어 넣으면서 많이 바뀜 수정 필요함

            
            //if (charCommandType == CharCommandType.HideAll)
            //{
            //    Debug.Log("Hide All 커맨드 실행");

            //    foreach (var charObject in NovelPlayer.Instance.currentCharacterDict.Values)
            //    {
            //        GameObject.Destroy(charObject.gameObject);
            //    }
            //    NovelPlayer.Instance.currentCharacterDict.Clear();
            //    return;
            //}



            //// 해당하는 이름이 존재하지 않으면 리턴
            //if (!NovelManager.Instance.characterSODict.ContainsKey(charName))
            //{
            //    //string name = charName.ToLower();
            //    Debug.LogError($"{charName} 캐릭터 존재하지 않음");
            //    return;
            //}


            ////노벨 매니저에 캐싱해둔 캐릭터 SO 불러오기
            //NovelCharacterSO charSO = NovelManager.Instance.GetCharacterSO(charName);
            ////NovelManager.Instance.characterSODict.TryGetValue(charName, out charSO);
            //if (charSO == null)
            //{
            //    Debug.LogError($"{charName} 캐릭터 SO 불러오기 실패");
            //    return;
            //}
            //// 목표료 하는 스탠딩의 게임오브젝트
            //GameObject standingObject = null;


            //// 캐릭터 숨기기
            //if (charCommandType == CharCommandType.Hide)
            //{
            //    Debug.Log($"Hide Character : {charName}");
            //    return;
            //}

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

