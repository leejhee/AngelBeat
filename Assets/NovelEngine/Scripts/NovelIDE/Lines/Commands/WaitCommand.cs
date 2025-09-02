using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public class WaitCommand : CommandLine
    {
        float waitTime;
        public WaitCommand(int index, float waitTime) : base(index, DialogoueType.CommandLine)
        {
            this.waitTime = waitTime;
        }

        public override async UniTask Execute()
        {
            if (waitTime > 0)
            {
                NovelManager.Player.StartWaitForseconds(waitTime);
            }
            else
            {
                NovelManager.Player.isWait = true;
            }
                
        }

        public override bool? IsWait()
        {
            return null;
        }
    }

}
