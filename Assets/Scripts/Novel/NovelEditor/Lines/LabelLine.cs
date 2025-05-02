using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public class LabelLine : NovelLine
    {
        public string labelName;

        public LabelLine(string labelName) : base(DialogoueType.LabelLine)
        {
            this.labelName = labelName;
        }
        public LabelLine() : base(DialogoueType.LabelLine) { }
    }
}

