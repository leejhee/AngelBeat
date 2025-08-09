using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public class LabelLine : NovelLine
    {
        public string labelName;

        public LabelLine(int index, string labelName, int depth = 0) : base(index, DialogoueType.LabelLine, depth)
        {
            this.labelName = labelName;
        }
    }
}

