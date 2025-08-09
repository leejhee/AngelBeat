using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public class NormalLine : NovelLine
    {
        
        public string line;

        public NormalLine(int index, string line, int depth = 0) : base(index, DialogoueType.NormalLine, depth)
        {
            this.line = line;
        }
    }
}