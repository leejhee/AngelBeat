using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public class GotoCommand : CommandLine
    {
        public string label;
        public GotoCommand(int index, string label, int depth = 0) : base(index, DialogoueType.CommandLine, depth)
        {
            this.label = label;
        }

        public override void Execute()
        {
            Debug.Log($"goto 실행 : {label}");

            NovelPlayer.Instance.currentAct.JumpToLabel(label);
        }

        public override bool? IsWait()
        {
            return null;
        }
    }

}
