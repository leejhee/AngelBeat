using System;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public class NovelAct
    {
        [SerializeReference]
        public List<NovelLine> novelLines = new();

        private int currentIndex = 0;

        public NovelLine GetNextLine()
        {
            if (currentIndex >= novelLines.Count) return null;

            return novelLines[currentIndex++];
        }

        // 이거 인덱스 어디로 가야할지 나중에 한번 체크해야함
        public void JumpToLabel(string label)
        {
            //foreach (var line in novelLines)
            //{
            //    if (line is LabelLine labelLine)
            //    {
            //        if (labelLine.labelName == label)
            //            currentIndex = labelLine.index;
            //    }
            //}
        }

        public void ResetAct() => currentIndex = 0;
    }
}
