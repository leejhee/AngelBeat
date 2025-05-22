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
        public CharCommand(int index, string name, string appearance, string transition, Vector2? pos, float? scale, float? time, bool? wait) : base(index, DialogoueType.CommandLine)
        {
            this.charName = name;
            this.appearance = appearance;
            this.transition = transition;
            this.pos = pos;
            this.scale = scale;
            this.time = time;
            this.wait = wait;
        }

        public override void Execute()
        {
            // 해당하는 이름이 존재하지 않으면 리턴
            if (!NovelManager.Instance.characterSODict.ContainsKey(charName))
            {
                Debug.LogError($"{charName} 캐릭터 존재하지 않음");
                return;
            }


            //노벨 매니저에 캐싱해둔 캐릭터 SO 불러오기
            NovelCharacterSO charSO;
            NovelManager.Instance.characterSODict.TryGetValue(charName, out charSO);
            if (charSO == null)
            {
                Debug.LogError($"{charName} 캐릭터 SO 불러오기 실패");
                return;
            }

            // 지금 언급한 스탠딩이 이미 띄워져 있을 경우
            if (NovelPlayer.Instance.currentCharacterDict.ContainsKey(charSO))
            {
                GameObject standingObject;
                NovelPlayer.Instance.currentCharacterDict.TryGetValue(charSO, out standingObject);

                // 표정이 없으면 몸에 달려있는 기본 표정으로
                if (appearance == "" || appearance == null)
                {
                    standingObject.transform.GetChild(1).gameObject.SetActive(false);
                }
                else
                {
                    // 머리 스프라이트 조정
                    GameObject headObject = standingObject.transform.GetChild(1).gameObject;
                    headObject.SetActive(true);
                    Sprite headSprite = charSO.GetHead(appearance);
                    if (headSprite == null)
                    {
                        Debug.LogError($"{charName}의 {appearance} 표정 존재하지 않음");
                        return;
                    }
                    headObject.GetComponent<Image>().sprite = headSprite;
                    RectTransform headTransform = headObject.GetComponent<RectTransform>();
                    headTransform.localPosition = charSO.headOffset;
                    headTransform.sizeDelta = new Vector2(headSprite.rect.width, headSprite.rect.height);

                }

                return;
            }
            else
            {
                // 프리팹 인스턴스화
                GameObject standingObject = GameObject.Instantiate(NovelPlayer.Instance.standingPrefab);
                standingObject.name = charSO.name;

                // 몸통 스프라이트 조정
                Sprite bodySprite = charSO.body;

                GameObject bodyObject = standingObject.transform.GetChild(0).gameObject;
                bodyObject.GetComponent<Image>().sprite = bodySprite;
                RectTransform bodyTransform = bodyObject.GetComponent<RectTransform>();
                bodyTransform.sizeDelta = new Vector2(bodySprite.rect.width, bodySprite.rect.height);



                // 표정이 없으면 몸에 달려있는 기본 표정으로
                if (appearance == "" || appearance == null)
                {
                    standingObject.transform.GetChild(1).gameObject.SetActive(false);
                }
                else
                {
                    // 머리 스프라이트 조정
                    GameObject headObject = standingObject.transform.GetChild(1).gameObject;
                    headObject.SetActive(true);
                    Sprite headSprite = charSO.GetHead(appearance);
                    if (headSprite == null)
                    {
                        Debug.LogError($"{charName}의 {appearance} 표정 존재하지 않음");
                        return;
                    }
                    headObject.GetComponent<Image>().sprite = headSprite;
                    RectTransform headTransform = headObject.GetComponent<RectTransform>();
                    headTransform.localPosition = charSO.headOffset;
                    headTransform.sizeDelta = new Vector2(headSprite.rect.width, headSprite.rect.height);

                }
                // 스탠딩 위치조절
                standingObject.GetComponent<RectTransform>().position = Vector2.zero;
                Vector2 percentPos = pos ?? new Vector2(50, 50);
                Vector2 normalizedAnchor = percentPos / 100f;

                RectTransform rectTransform = standingObject.GetComponent <RectTransform>();

                rectTransform.anchorMin = normalizedAnchor;
                rectTransform.anchorMax = normalizedAnchor;
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                rectTransform.anchoredPosition = Vector2.zero;

                // 스탠딩 패널에 넣기
                standingObject.transform.SetParent(NovelPlayer.Instance.standingPanel.transform, false);
                // 현재 나와있는 스탠딩리스트에 추가
                NovelPlayer.Instance.currentCharacterDict.Add(charSO, standingObject);
            }


        }
    }
}

