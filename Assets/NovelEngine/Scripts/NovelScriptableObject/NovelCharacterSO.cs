using Core.Foundation.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Novel/Character Data")]
public class NovelCharacterSO : ScriptableObject
{
    public string characterName;
    public Sprite body;
    public SerializableDict<string, Sprite> faceDict = new();

    [Header("직접 수정해야 하는것")]
    public Vector2 headOffset;
    public string novelName;

    public void Init(string name, List<Sprite> heads)
    {
        characterName = name;
        MakeHeadDictionary(heads);
    }
    public Sprite GetHead(string head)
    {
        Sprite sprite = faceDict.GetValue(head);
        if (sprite == null)
        {
            Debug.LogError($"{head} 표정이 존재하지 않음");
            return null;
        }

        return sprite;
    }
    private void MakeHeadDictionary(List<Sprite> sprites)
    {
        foreach (var sprite in sprites)
        {
            if (!faceDict.ContainsKey(sprite.name))
            {
                if (sprite.name == "Body")
                    body = sprite;
                else
                {
                    faceDict.Add(sprite.name, sprite);
                }
                    
            }
        }
    }
}