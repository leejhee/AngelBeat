using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public class CommandLine : NovelLine
    {
        public CommandType commandType;

        public CommandLine(CommandType cmdType) : base(DialogoueType.CommandLine)
        {
            commandType = cmdType;
        }
        public CommandLine() : base(DialogoueType.CommandLine) { }
    }
}