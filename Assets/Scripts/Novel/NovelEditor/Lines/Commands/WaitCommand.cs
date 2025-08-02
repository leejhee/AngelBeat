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
        public WaitCommand(int index, float waitTime, int depth = 0) : base(index, DialogoueType.CommandLine, depth)
        {
            this.waitTime = waitTime;
        }

        public override void Execute()
        {
            if (waitTime > 0)
            {
                NovelPlayer.Instance.StartWait(waitTime);
            }
            else
            {
                NovelPlayer.Instance.isWait = true;
            }
                
        }

        public override bool? IsWait()
        {
            return null;
        }
    }

}
