using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public class PersonLine : NovelLine
    {
        public string actorName;
        public string actorLine;

        public PersonLine(int index, string name , string line, int depth = 0) : base(index, DialogoueType.NormalLine, depth)
        {
            this.actorLine = line;

            
            this.actorName = NovelManager.Instance.GetCharacterSO(name).novelName;
        }
    }
}