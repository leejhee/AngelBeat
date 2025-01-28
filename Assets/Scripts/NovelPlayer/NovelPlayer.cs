using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace novel
{
    public class NovelPlayer : MonoBehaviour
    {
        [SerializeField]
        private Button NextButton;
        [SerializeField]
        private TextMeshProUGUI text;
        [SerializeField]
        private NovelManager _novelManager;
        // Start is called before the first frame update
        void Start()
        {
            NextButton.onClick.AddListener(ProgressNovel);
        }
        private void ProgressNovel()
        {
            Debug.Log("다음 대사");

        }
    }
}

