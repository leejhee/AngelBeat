using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public class NormalLine : NovelLine
    {
        
        public string line;

        public NormalLine(string line) : base(DialogoueType.NormalLine)
        {
            this.line = line;
        }
        public NormalLine() : base(DialogoueType.NormalLine) { }
    }
}