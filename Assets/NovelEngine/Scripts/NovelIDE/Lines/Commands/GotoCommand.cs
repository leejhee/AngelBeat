using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NovelParser;

namespace novel
{
    [System.Serializable]
    public class GotoCommand : CommandLine
    {
        public string label;
        public GotoCommand(int index, 
            string label, 
            IfParameter ifParameter = null) 
            : base(index, DialogoueType.CommandLine)
        {
            this.label = label;
            this.ifParameter = ifParameter;
        }

        public override async UniTask Execute()
        {
            //if (!ifParameter)
            //    return;
            Debug.Log($"goto 실행 : {label}");
            NovelManager.Player.CurrentAct.JumpToLabel(label);
        }
    }
}
