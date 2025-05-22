using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using novel;
using System;


public class NovelManager : SingletonObject<NovelManager>
{
    #region 생성자
    NovelManager() { }
    #endregion

    private Dictionary<string, NovelCharacterSO> characterSODict = new();
    private const string characterSOPath = "Novel/NovelResourceData/CharacterData/CharacterSO";
    public override void Init()
    {
        //CreateCharacterSOAssets();
        LoadCharacterSO();
    }
    private void CreateCharacterSOAssets()
    {
        string[] characterNames = Enum.GetNames(typeof(CharacterName));
        foreach (var characterName in characterNames)
        {
            NovelCharacterSOFactory.CreateSpriteDataFromAtlas(characterName);
        }
    }
    private void LoadCharacterSO()
    {
        characterSODict.Clear();

        string[] characterNames = Enum.GetNames(typeof(CharacterName));
        NovelCharacterSO[] characterSOs = ResourceManager.LoadAllAssets<NovelCharacterSO>(characterSOPath);
        if (characterSOs == null || characterSOs.Length == 0)
        {
            Debug.LogError($"캐릭터 SO 불러오기 실패 : {characterSOPath}");
        }
        else
        {
            Debug.Log($"{characterNames.Length}명 SO 불러옴");
        }

        foreach (var character in characterSOs)
        {
            characterSODict.Add(character.name, character);
        }
        
    }
    public void PlayScript(string scriptTitle)
    {

    }
}