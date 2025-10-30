using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    public class HideUICommand : CommandLine
    {

        public HideUICommand(int index) : base(index, DialogoueType.CommandLine)
        {
        }

        public override UniTask Execute()
        {
            NovelManager.Player.gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }
    }

}
