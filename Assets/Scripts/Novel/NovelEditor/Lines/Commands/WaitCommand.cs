using System.Collections;
using System.Collections.Generic;
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

        public override void Execute()
        {
            NovelPlayer.Instance.isWait = true;


            // 시간 있을때 시간만큼만 멈추는거 나중에 해야함
        }

        public override bool? IsWait()
        {
            return null;
        }
    }

}
