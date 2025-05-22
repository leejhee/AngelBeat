using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Novel/Character Data")]
public class NovelCharacterSO : ScriptableObject
{
    public string characterName;
    public Sprite body;
    public Dictionary<string, Sprite> headDict = new();
    [Tooltip("얘는 나중에 직접 수정해줘야함")]
    public Vector2 headOffset;

    public void Init(string name, List<Sprite> heads)
    {
        characterName = name;
        MakeHeadDictionary(heads);
    }
    public Sprite GetHead(string head)
    {
        Sprite sprite;
        headDict.TryGetValue(head, out sprite);

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
            if (!headDict.ContainsKey(sprite.name))
            {
                if (sprite.name == "Body")
                    body = sprite;
                else
                    headDict.Add(sprite.name, sprite);
            }
        }
    }
}