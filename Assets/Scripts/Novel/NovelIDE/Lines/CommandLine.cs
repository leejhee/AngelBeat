using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public abstract class CommandLine : NovelLine, IExecutable
    {
        [SerializeReference]
        public  List<NovelLine> subLines = new();
        public CommandLine(int index, DialogoueType type, int? depth) : base(index, type, depth) { }

        public abstract void Execute();
        public abstract bool? IsWait();
    }
}