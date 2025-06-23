using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public abstract class CommandLine : NovelLine, IExecutable
    {
        [SerializeField]
        public List<NovelLine> subLines = new();
        public CommandLine(int index, DialogoueType type) : base(index, type) { }

        public abstract void Execute();
        public abstract bool? IsWait();
    }
}