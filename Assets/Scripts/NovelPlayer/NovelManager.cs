using Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    public class NovelManager : MonoBehaviour
    {
        private static NovelParser parser;
        public List<NovelDataSample> nowNovel;
        private void Start()
        {
            for (int i = 1; i < 5; i++)
            {
                nowNovel.Add(DataManager.Instance.GetData<NovelDataSample>(i));
            }
        }

        private void LoadNovel(int index)
        {
            // 인덱스 범위 지정해서 해당하는 스크립트들 불러오기
        }
    }

}