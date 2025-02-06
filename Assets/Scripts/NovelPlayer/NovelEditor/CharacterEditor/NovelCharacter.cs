using UnityEngine;
using System.Collections.Generic;

namespace novel
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "Novel/Character")]
    public class NovelCharacter : ScriptableObject
    {
        public string characterName;
        public List<CharacterStandingBody> bodySpirtes = new();
        public List<CharacterStandingFace> faceSprites = new();
        public List<CharacterStandingEffect> effectSprites = new();
    }
}