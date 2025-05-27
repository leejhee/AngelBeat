#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public static class NovelCharacterSOFactory
{
    private const string characterSOPath = "Assets/Resources/Novel/NovelResourceData/CharacterData/CharacterSO/";
    private const string characterSpritePath = "Novel/NovelResourceData/GraphicData/StandingData/";
    public static void CreateSpriteDataFromAtlas(string name)
    {
        // 아틀라스 스프라이트 리스트로 받아옴
        List<Sprite> sprites = ResourceManager.LoadAtlasSprites(characterSpritePath + name);
        string assetPathAndName = $"{characterSOPath}{name}.asset";

        //기존 에셋 불러옴
        NovelCharacterSO oldAsset = AssetDatabase.LoadAssetAtPath<NovelCharacterSO>(assetPathAndName);

        NovelCharacterSO asset = ScriptableObject.CreateInstance<NovelCharacterSO>();

        asset.Init(name, sprites);

        //기존에 이미 SO파일이 있다면 머리 위치랑 이름 남기고 삭제
        if (oldAsset != null)
        {
            asset.headOffset = oldAsset.headOffset;
            asset.novelName = oldAsset.novelName;
            AssetDatabase.DeleteAsset(assetPathAndName);
            Debug.Log("SO 덮어씌우기");
        }

        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Asset created at {assetPathAndName}");
    }
}
#endif