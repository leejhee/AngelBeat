using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.IO;

namespace novel
{
    public static class NovelEngine
    {
        // 엔진 초기화 할때, 하는중, 하고 나서 해줄 액션들 추가해줄수 있음 아직은 기능 안넣음



        static void Init()
        {
            // 여기서 노블매니저 초기화 해주기
            
        }

        public static void Play(string name)
        {

        }
        public static void CloseEngine()
        {
            GameObject.Destroy(NovelPlayer.Instance.gameObject);
            
        }
    }

}
