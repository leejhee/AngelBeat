using Client;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace novel
{
    public class NovelParser
    {
        public void Parse(NovelDataSample data)
        {
            NovelCommand command = (NovelCommand)Enum.Parse(typeof(NovelCommand), data.command);
            switch (command)
            {
                case NovelCommand.NormalText:

                    break;
                case NovelCommand.PersonText:
                    break;
                case NovelCommand.BackGround:
                    break;
                case NovelCommand.Stand:
                    break;
                case NovelCommand.BGM:
                    break;
                case NovelCommand.SFX:
                    break;
                case NovelCommand.Choice:
                    break;
                case NovelCommand.Effect:
                    break;
                default:
                    break;
            }
        }
    }
}

