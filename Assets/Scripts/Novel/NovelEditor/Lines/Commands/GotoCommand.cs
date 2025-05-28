using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    public class GotoCommand : CommandLine
    {
        public string label;
        public GotoCommand(int index, string label) : base(index, DialogoueType.CommandLine)
        {
            this.label = label;
        }

        public override void Execute()
        {
            NovelPlayer.Instance.currentAct.JumpToLabel(label);
        }

        public override bool? IsWait()
        {
            return null;
        }
    }

}
