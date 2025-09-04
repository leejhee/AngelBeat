using Cysharp.Threading.Tasks;
using GamePlay.Features.Scripts.Battle.Unit;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;


namespace novel
{
    [System.Serializable]
    public class BackCommand : CommandLine
    {
        public string backName;
        public string transition;
        public Vector2? pos;
        public float? scale;
        public float? time;
        
        public BackCommand(int index, string backName,  string transition,
                            Vector2? pos, float? scale, float? time ) : base(index, DialogoueType.CommandLine)
        {
            this.backName = backName;
            this.transition = transition;
            this.pos = pos;
            this.scale = scale;
            this.time = time;
        }

        public override async UniTask Execute()
        {
            var player = NovelManager.Player;

            // 기존에 있던 배경 오브젝트 제거
            if (player.currentBackgroundObject != null)
            {
                var backgroundObject = player.currentBackgroundObject;
                backgroundObject.SetActive(false);
                Addressables.ReleaseInstance(backgroundObject);
            }

            // 배경 프리팹 불러오기
            GameObject backgroundPrefab = null;
            try
            {
                var handle = Addressables.InstantiateAsync("BackgroundBase",
                            player.backgroundPanel.transform);
                backgroundPrefab = await handle.Task;
                if (backgroundPrefab == null)
                {
                    Debug.LogError("배경화면 프리팹 인스턴스화 실패");
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }

            // 데이터에서 배경화면 불러오기
            Texture2D backgroundTexture = NovelManager.Data.background.GetTexture2DByName(backName);
            if (backgroundTexture == null)
            {
                Debug.LogError($"{backName} 배경화면 불러오기 실패");
                return;
            }
            Sprite sprite = Sprite.Create(backgroundTexture,
                                          new Rect(0, 0, backgroundTexture.width, backgroundTexture.height),
                                          new Vector2(0.5f, 0.5f));


            if (sprite == null)
            {
                Debug.LogError("스프라이트 Create 실패 " + backName);
                return;
            }

            Image image = backgroundPrefab.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = sprite;

                // 위치 조정
                Vector2 percentPos = pos ?? new Vector2(50, 50);
                Vector2 normalizedAnchor = percentPos / 100f;

                RectTransform rectTransform = backgroundPrefab.GetComponent<RectTransform>();

                rectTransform.anchorMin = normalizedAnchor;
                rectTransform.anchorMax = normalizedAnchor;
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                rectTransform.anchoredPosition = Vector2.zero;

                // 크기 조정
                
                //나중에 화면 크기 필요할수도 있을거 같아서 일단 가져옴
                //Vector2 canvasSize = new Vector2(Screen.width, Screen.height);

                if (scale.HasValue)
                    backgroundPrefab.transform.localScale = Vector3.one * scale.Value;

            }

            // 전환효과가 있을 경우
            if (transition != null && transition != "")
            {
                if (transition.ToLower() == "fadeout")
                {
                    Debug.Log("페이드아웃");
                    float fadeTime = time ?? 0f;
                    //NovelManager.Player.BackgroundFadeOut(image, fadeTime, backgroundPrefab, true, isWait);
                }
            }



            //backgroundPrefab.transform.SetParent(NovelManager.Player.backgroundPanel.transform, false);
            NovelManager.Player.SetBackground(backgroundPrefab);
        }
    }
}