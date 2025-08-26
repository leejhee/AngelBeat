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

        public PersonLine(int index, string name , string line) : base(index, DialogoueType.NormalLine)
        {
            this.actorLine = line;

            // 얘도 나중에 바꿔줘야함
            
            //this.actorName = NovelManager.Instance.GetCharacterSO(name).novelName;
        }
    }
}