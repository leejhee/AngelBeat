using System;
using UnityEngine;

[Serializable]
public class BaseMapNodeData : ScriptableObject
{
    public int index;
    public Sprite nodeSprite;
    public eNodeType nodeType;
}