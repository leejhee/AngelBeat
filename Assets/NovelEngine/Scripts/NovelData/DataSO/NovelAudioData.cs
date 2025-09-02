using NovelEngine.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [CreateAssetMenu(fileName = "NovelAudioData", menuName = "Novel/NovelAudioData", order = 0)]
    public class NovelAudioData : ScriptableObject
    {
        [SerializeField] private SerializableDict<string, AudioClip> bgmDict = new();
        [SerializeField] private SerializableDict<string, AudioClip> sfxDict = new();
        public AudioClip GetBGMByName(string name)
        {
            if (bgmDict.TryGetValue(name, out var bgm))
            {
                return bgm;
            }
            Debug.LogError($"BGM {name} 을(를) 찾을 수 없음.");
            return null;
        }
        public AudioClip GetSEByName(string name)
        {
            if (sfxDict.TryGetValue(name, out var se))
            {
                return se;
            }
            Debug.LogError($"SFX {name} 을(를) 찾을 수 없음.");
            return null;
        }
    }
}