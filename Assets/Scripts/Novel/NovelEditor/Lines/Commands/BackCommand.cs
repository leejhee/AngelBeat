using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
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
        public bool? wait;

        
        public BackCommand(int index, string backName,  string transition, Vector2? pos, float? scale, float? time, bool? wait) : base(index, DialogoueType.CommandLine)
        {
            this.backName = backName;
            this.transition = transition;
            this.pos = pos;
            this.scale = scale;
            this.time = time;
            this.wait = wait;
        }

        public override void Execute()
        {


            // 기존에 있던 배경 오브젝트 제거
            if (NovelPlayer.Instance.currentBackgroundObject != null)
                GameObject.Destroy(NovelPlayer.Instance.currentBackgroundObject);

            // 배경 프리팹 불러오기
            GameObject backgroundPrefab = GameObject.Instantiate(NovelPlayer.Instance.backgroundPrefab);

            // 배경 이미지 불러오기
            Sprite sprite = ResourceManager.LoadImageFromResources("Novel/NovelResourceData/GraphicData/BackgroundData/" + backName);
            if (sprite == null)
            {
                Debug.LogError("배경 이미지 불러오기 실패" + backName);
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
                    NovelPlayer.Instance.BackgroundFadeOut(image, fadeTime, backgroundPrefab);
                }
                else
                {
                    Debug.Log("왜안대");
                }
            }



            backgroundPrefab.transform.SetParent(NovelPlayer.Instance.backgroundPanel.transform, false);
            NovelPlayer.Instance.currentBackgroundObject = backgroundPrefab;
        }
    }
}