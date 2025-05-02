using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public abstract class NovelLine
    {
        public int index;
        public DialogoueType type;

        protected NovelLine(DialogoueType type)
        {
            this.type = type;
        }
    }
}