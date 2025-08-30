using Core.Scripts.Foundation.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    public class NovelCharacterData : ScriptableObject
    {
        [SerializeField] private SerializableDict<string, NovelCharacterSO> charDict = new();
    }
    [System.Serializable]
    public class NovelAudioData : ScriptableObject
    {
        [SerializeField] private SerializableDict<string, AudioClip> bgmDict = new();
        [SerializeField] private SerializableDict<string, AudioClip> sfxDict = new();
    }
    public class NovelVariableData : ScriptableObject
    {
        [SerializeField] private SerializableDict<string, float> novelVariableDict = new();
    }
    public class NovelBackgroundData : ScriptableObject
    {
        [SerializeField] private SerializableDict<string, Texture2D> novelBackgroundDict = new();
    }
    public class NovelScriptData : ScriptableObject
    {
        [SerializeField] private SerializableDict<string, TextAsset> scriptList = new();

        public TextAsset GetScriptByTitle(string title)
        {
            if (scriptList.TryGetValue(title, out var script))
            {
                return script;
            }
            Debug.LogError($"스크립트 {title} 을(를) 찾을 수 없음.");
            return null;
        }
    }
}
