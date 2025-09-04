using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

namespace novel
{
    [System.Serializable]
    public class CharCommand : CommandLine
    {
        private const int BODY_CHILD_INDEX = 0;
        private const int HEAD_CHILD_INDEX = 1;

        string charName;
        string appearance;
        string transition;
        Vector2? pos;
        float? scale;
        float? time;

        CharCommandType charCommandType;
        public CharCommand(
            int index,
            string name,
            string appearance,

            // 이거 나중에 이넘으로 바꿀까
            string transition,
            Vector2? pos,
            float? scale,
            float? time,
            CharCommandType charCommandType = CharCommandType.Show
        ) : base(index, DialogoueType.CommandLine)
        {
            this.charName = name;
            this.appearance = appearance;
            this.transition = transition;
            this.pos = pos;
            this.scale = scale;
            this.time = time;
            this.charCommandType = charCommandType;   
        }
        public override async UniTask Execute()
        {
            var player = NovelManager.Player;
            var dict = player.currentCharacterDict;
            var token = player.CommandToken;

            try
            {
                if (charCommandType == CharCommandType.HideAll)
                {
                    Debug.Log("Hide All 커맨드 실행");
                    var toRelease = new List<GameObject>(dict.Values).ToArray();
                    foreach (var charObject in toRelease)
                    {
                        if (charObject == null) continue;
                        charObject.SetActive(false);
                        Addressables.ReleaseInstance(charObject);
                    }
                    dict.Clear();
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
                        if (dict.TryGetValue(charSO, out GameObject standingObject) && standingObject != null)
                        {
                            var headObject = standingObject.transform.GetChild(HEAD_CHILD_INDEX).gameObject;

                            if (string.IsNullOrEmpty(appearance))
                            {
                                headObject.SetActive(false);
                            }
                            else
                            {
                                var headSprite = charSO.GetHead(appearance);
                                if (headSprite == null)
                                {
                                    Debug.LogError($"{charName}의 {appearance} 표정 존재하지 않음");
                                    headObject.SetActive(false);
                                    break;
                                }

                                headObject.SetActive(true);

                                if (!headObject.TryGetComponent<Image>(out var headImage))
                                    headImage = headObject.AddComponent<Image>();


                                headImage.sprite = headSprite;

                                var headTransform = headObject.GetComponent<RectTransform>();
                                headTransform.localPosition = charSO.headOffset;
                                headTransform.sizeDelta = new Vector2(headSprite.rect.width, headSprite.rect.height);
                            }
                            break;
                        }
                        else  // 현재 캐릭터가 띄워져 있지 않은 경우
                        {
                            GameObject newStanding = null;
                            try
                            {
                                var handle = Addressables.InstantiateAsync("CharacterStandingBase",
                                            player.standingPanel.transform);
                                newStanding = await handle.Task;
                                if (newStanding == null)
                                {
                                    Debug.LogError("프리팹 인스턴스화 실패");
                                    break;
                                }
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogError(e);
                                break;
                            }
                            // 이부분 필요한지 고민
                            //// 클릭으로 이미 취소되었으면 즉시 엔드 스냅
                            //if (token.IsCancellationRequested)
                            //{
                            //    // 즉시 정리(엔드 상태 스냅): 보여줄 필요 없으면 바로 제거
                            //    newStanding.SetActive(false);
                            //    Addressables.ReleaseInstance(newStanding);
                            //    break;
                            //}
                            var bodyObject = newStanding.transform.GetChild(BODY_CHILD_INDEX).gameObject;
                            var headObject = newStanding.transform.GetChild(HEAD_CHILD_INDEX).gameObject;

                            var bodySprite = charSO.body;
                            if (!bodyObject.TryGetComponent<Image>(out var bodyImage))
                                bodyImage = bodyObject.AddComponent<Image>();
                            bodyImage.sprite = bodySprite;

                            var bodyTransform = bodyObject.GetComponent<RectTransform>();
                            bodyTransform.sizeDelta = new Vector2(bodySprite.rect.width, bodySprite.rect.height);

                            if (string.IsNullOrEmpty(appearance))
                            {
                                headObject.SetActive(false);
                            }
                            else
                            {
                                var headSprite = charSO.GetHead(appearance);
                                if (headSprite == null)
                                {
                                    Debug.LogError($"{charName}의 {appearance} 표정 존재하지 않음");
                                    headObject.SetActive(false);
                                }
                                else
                                {
                                    headObject.SetActive(true);
                                    if (!headObject.TryGetComponent<Image>(out var headImage))
                                        headImage = headObject.AddComponent<Image>();
                                    headImage.sprite = headSprite;

                                    var headTransform = headObject.GetComponent<RectTransform>();
                                    headTransform.localPosition = charSO.headOffset;
                                    headTransform.sizeDelta = new Vector2(headSprite.rect.width, headSprite.rect.height);
                                }
                            }

                            // 위치/스케일
                            {
                                var rt = newStanding.GetComponent<RectTransform>();
                                Vector2 percentPos = pos ?? new Vector2(50, 50);
                                Vector2 anchor = percentPos / 100f;

                                rt.anchorMin = anchor;
                                rt.anchorMax = anchor;
                                rt.pivot = new Vector2(0.5f, 0.5f);
                                rt.anchoredPosition = Vector2.zero;

                                if (scale.HasValue)
                                {
                                    rt.localScale = Vector3.one * scale.Value;
                                }
                            }


                            bool doFadeIn = string.Equals(transition, "fadein");


                            newStanding.SetActive(true);

                            // 페이드 인 연출
                            if (doFadeIn)
                            {
                                if (!newStanding.TryGetComponent<CanvasGroup>(out var cg))
                                    cg = newStanding.AddComponent<CanvasGroup>();

                                cg.alpha = doFadeIn ? 0f : 1f;  // fadein일 경우 0으로 시작

                                float time = this.time ?? 0f;
                                if (time > 0f)
                                    await NovelUtils.Fade(newStanding, time, true, token);
                                else
                                    cg.alpha = 1f; // 즉시 엔드
                            }

                            dict[charSO] = newStanding;
                            break;
                            //NovelManager.Player.currentCharacterDict.Add(charSO, standingObject);
                        }

                    case CharCommandType.Hide:
                        Debug.Log($"Hide Character : {charName}");
                        if (!dict.TryGetValue(charSO, out GameObject hideObject) || hideObject == null)
                        {
                            // 이미 없으면 조용히 정리
                            dict.Remove(charSO);
                            break;
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(transition))
                            {
                                float dur = this.time ?? 0f;
                                if (dur > 0f)
                                    await NovelUtils.Fade(hideObject, dur, false, token);
                                else
                                {
                                    // 즉시 엔드
                                    if (!hideObject.TryGetComponent<CanvasGroup>(out var cg))
                                        cg = hideObject.AddComponent<CanvasGroup>();
                                    cg.alpha = 0f;
                                }
                            }
                        }
                        finally
                        {
                            hideObject.SetActive(false);
                            Addressables.ReleaseInstance(hideObject);
                            dict.Remove(charSO);
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

        }
    }
}

