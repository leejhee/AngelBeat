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

        public PersonLine(string name, string line) : base(DialogoueType.PersonLine)
        {
            this.actorLine = line;
            this.actorName = name;
        }
        public PersonLine() : base(DialogoueType.PersonLine) { }
    }
}