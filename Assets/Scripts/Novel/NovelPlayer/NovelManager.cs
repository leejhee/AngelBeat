using Core.Foundation;
using Core.Foundation.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using novel;
using System;
using Core.Managers;
using Unity.VisualScripting;


public class NovelManager : MonoBehaviour
{
    private static NovelManager instance;
    public static NovelManager Instance
    {
        get
        {
            if (!instance) Init();
            return instance;
        }
    }

    private static GameObject novelPlayer;
    private const string novelPlayerPrefabPath = "";

    //private SerializableDict<string, NovelCharacterSO> _characterSODict = new();
    //private const string characterSOPath = "Novel/NovelResourceData/CharacterData/CharacterSO";

    //public SerializableDict<string, NovelCharacterSO> characterSODict
    //{
    //    get { return _characterSODict; }
    //    private set { _characterSODict = value; }
    //}
    static void Init()
    {
        GameObject go = GameObject.Find("@Novel");
        if (!go)
        {
            go = new GameObject { name = "@Novel" };
            go.AddComponent<NovelManager>();
        }

        instance = go.GetComponent<NovelManager>();


    }

    //public  void CreateCharacterSOAssets()
    //{
    //    string[] characterNames = Enum.GetNames(typeof(CharacterName));
    //    foreach (var characterName in characterNames)
    //    {
    //        Debug.Log($"{characterName}");
    //        NovelCharacterSOFactory.CreateSpriteDataFromAtlas(characterName);
    //    }
    //}
    //private void LoadCharacterSO()
    //{
    //    _characterSODict.Clear();

    //    string[] characterNames = Enum.GetNames(typeof(CharacterName));
    //    NovelCharacterSO[] characterSOs = ResourceManager.LoadAllAssets<NovelCharacterSO>(characterSOPath);
    //    if (characterSOs == null || characterSOs.Length == 0)
    //    {
    //        Debug.LogError($"캐릭터 SO 불러오기 실패 : {characterSOPath}");
    //    }
    //    else
    //    {
    //        Debug.Log($"{characterNames.Length}명 SO 불러옴");
    //    }

    //    foreach (var character in characterSOs)
    //    {
    //        _characterSODict.Add(character.name, character);
    //    }
        
    //}
    //public NovelCharacterSO GetCharacterSO(string name)
    //{
    //    NovelCharacterSO characterSO = _characterSODict.GetValue(name);
    //    //_characterSODict.TryGetValue(name, out characterSO);
    //    if (characterSO == null)
    //    {
    //        Debug.LogError($"{name} SO 불러오기 실패");
    //        return null;
    //    }
    //    return characterSO;
    //}
    public void PlayScript(string scriptTitle)
    {

    }


}