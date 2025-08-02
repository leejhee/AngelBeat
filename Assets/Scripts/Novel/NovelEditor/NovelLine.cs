using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public abstract class NovelLine
    {
        public int index;
        public DialogoueType type;
        public int depth;
        protected NovelLine(int index, DialogoueType type, int depth)
        {
            this.index = index;
            this.type = type;
            this.depth = depth;
        }
    }
}