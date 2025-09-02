using Core.Foundation.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [Serializable] public class StringAudioClipDict : SerializableDict<string, AudioClip> { }
    [Serializable] public class StringTextAssetDict : SerializableDict<string, TextAsset> { }
    [Serializable] public class StringTexture2DDict : SerializableDict<string, Texture2D> { }
    [Serializable] public class StringFloatDict : SerializableDict<string, float> { }
    [Serializable] public class StringCharacterSoDict : SerializableDict<string, NovelCharacterSO> { }


    public class NovelCharacterData : ScriptableObject
    {
        [SerializeField] StringCharacterSoDict charDict = new();
        public NovelCharacterSO GetCharacterSO(string name)
            => charDict.TryGetValue(name, out var character) ? character : null;
    }

    [System.Serializable]
    public class NovelAudioData : ScriptableObject
    {
        [SerializeField] private StringAudioClipDict bgmDict = new();
        [SerializeField] private StringAudioClipDict sfxDict = new();
    }
    public class NovelVariableData : ScriptableObject
    {
        [SerializeField] private StringFloatDict novelVariableDict = new();
    }
    public class NovelBackgroundData : ScriptableObject
    {
        [SerializeField] private StringTexture2DDict novelBackgroundDict = new();
    }
    public class NovelScriptData : ScriptableObject
    {
        [SerializeField] private StringTextAssetDict scriptList = new();
        public TextAsset GetScriptByTitle(string title)
            => scriptList.TryGetValue(title, out var script) ? script : null;
    }
}
