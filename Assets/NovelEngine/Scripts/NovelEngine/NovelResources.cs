using Core.Foundation.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novle
{
    //노블 리소스에 들어가야할 데이터들
    // 변수들
    // 캐릭터
    // 배경
    // 사운드
    // 스크립트

    public class NovelValues
    {
        public SerializableDict<string, float> novelValuesDict = new();
    }

    public class NovelCharacters
    {
        public List<NovelCharacterSO> charList = new();
    }
    public class NovelBackGrounds
    {

    }
    public class NovelSounds
    {
        public List<string> bgmList = new();
        public List<string> sfxList = new();
    }
    public class NovelScripts
    {
        public SerializableDict<string, TextAsset> novelScripts = new();
    }
}