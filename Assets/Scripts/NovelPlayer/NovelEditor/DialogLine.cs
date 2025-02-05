using UnityEngine;

namespace novel
{
    [System.Serializable]
    public class DialogueLine
    {
        public long index;
        [TextArea(2, 5)]
        public string dialogue; // 대사 내용
        public CommandType command;
    }
}
