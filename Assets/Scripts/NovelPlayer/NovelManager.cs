using Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using novel;


public class NovelManager : SingletonObject<NovelManager>
{
    #region 생성자
    NovelManager() { }
    #endregion

    NovelPlayer novelPlayer;
    public override void Init()
    {
        novelPlayer = Object.FindObjectOfType<NovelPlayer>(true);
    }

    public void PlayScript(string scriptTitle)
    {
        novelPlayer.gameObject.SetActive(true);
        // 여기 스트링으로 받아올지 이넘으로 받아올지는 고민 일단 스트링으로

        NovelScript script = AssetDatabase.LoadAssetAtPath<NovelScript>($"Assets/NovelScriptData/{scriptTitle}.asset");
        if (script == null)
        {
            Debug.LogError($"Error: {scriptTitle} 스크립트 없음");
            return;
        }
        novelPlayer.SetDialogue(script);
    }
}