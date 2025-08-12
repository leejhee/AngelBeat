using AngelBeat;
using Core.Data;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DataManager = Core.Managers.DataManager;

namespace UIs.UIObjects
{
    public class RewardObject : MonoBehaviour
    {
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private Image rewardImage;
        [SerializeField] private Button interactionButton;
        [SerializeField] private float fadeTime;
       private void Awake()
        {
            Color oldImageColor = rewardImage.color;
            rewardText.color = new Color(0, 0, 0, 0);
            rewardImage.color = new Color(oldImageColor.r, oldImageColor.g, oldImageColor.b, 0);
        }
        public event Action OnClickReward;
        public void SetReward(string text, long amount)
        {
            if(text == "Skill") 
                rewardText.SetText(text + $": {DataManager.Instance.GetData<SkillData>(amount).skillName}");
            else
                rewardText.SetText(text + $": {amount}");
            interactionButton.onClick.AddListener(() => StartCoroutine(FadeOutUI(fadeTime)));
            StartCoroutine(FadeInUI(fadeTime));
        }

        private IEnumerator FadeInUI(float time)
        {
            float elapsed = 0f;
            while (elapsed < time)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / time);
                var oldImageColor = rewardImage.color;
                rewardText.color = new Color(0, 0, 0, t);
                rewardImage.color = new Color(oldImageColor.r, oldImageColor.r, oldImageColor.r, t);
                yield return null;
            }
        }
        
        private IEnumerator FadeOutUI(float time)
        {
            float elapsed = 0f;
            while (elapsed < time)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / time);
                var oldImageColor = rewardImage.color;
                rewardImage.color = new Color(oldImageColor.r, oldImageColor.g, oldImageColor.b, 1 - t);
                rewardText.color = new Color(0, 0, 0, 1 - t);
                yield return null;
            }
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            OnClickReward?.Invoke();
        }
    }
}