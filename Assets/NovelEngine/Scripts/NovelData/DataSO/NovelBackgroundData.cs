using Core.Scripts.Foundation.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    public class NovelBackgroundData : ScriptableObject
    {

        [SerializeField] private NovelEngine.Scripts.SerializableDict<string, Texture2D> novelBackgroundDict = new();
        
    }
}
