//using System.Collections;
//using System;
//using System.Collections.Generic;
//using UnityEngine;

//[Serializable]
//public class NovelStandingFaceDict
//{
//    public List<NovelStandingKeyValuePair> pairs = new();
//    public void Add(string key, Sprite value)
//    {
//        pairs.Add(new NovelStandingKeyValuePair(key, value));
//    }
//    public Sprite GetSprite(string headName)
//    {
//        Sprite result = null;
//        foreach (var pair in pairs)
//        {
//            if (pair.key == headName)
//            {
//                result = pair.value;
//                break;
//            }
//        }
//        return result;
//    }
//    public bool ContainsKey(string headName)
//    {

//        foreach(var pair in pairs)
//        {
//            if (pair.key == headName)
//            {
//                return true;
//            }
//        }
//        return false;
//    }

//}
//[Serializable]
//public class NovelStandingKeyValuePair
//{
//    public string key;
//    public Sprite value;
//    public NovelStandingKeyValuePair(string key, Sprite value)
//    {
//        this.key = key;
//        this.value = value;
//    }
//}
