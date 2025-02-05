using UnityEngine;
using System.Collections.Generic;
namespace novel
{
    [CreateAssetMenu(fileName = "NewScript", menuName = "Novel/Script")]
    public class NovelScript : ScriptableObject
    {
        public string scriptTitle;
        public List<DialogueLine> dialogueLines = new();
    }
}
