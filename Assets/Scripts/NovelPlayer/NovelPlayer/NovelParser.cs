using Client;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace novel
{
    public class NovelParser : MonoBehaviour
    {
        public ParseObject Parse(DialogueLine dialogue)
        {
            ParseObject parseObj = new();
            switch (dialogue.command)
            {
                case CommandType.None:
                    parseObj.command = dialogue.command;
                    string[] split_data = dialogue.dialogue.Split(": ");
                    if (split_data.Length <= 1)
                    {
                        //사람 이름이 없는 경우
                        parseObj.name = "";
                        parseObj.text = split_data[0];
                    }
                    else
                    {
                        parseObj.name = split_data[0];
                        parseObj.text = split_data[1];
                    }
                    break;
                case CommandType.Background:
                    break;
                case CommandType.BGM:
                    break;
                case CommandType.SFX:
                    break;
                case CommandType.Effect:
                    break;
                case CommandType.ShowCharacter:
                    break;
                case CommandType.HideCharacter:
                    break;
                case CommandType.Clearall:
                    break;
                case CommandType.Choice:
                    break;
                case CommandType.Goto:
                    break;
            }
            return parseObj;
        }
    }

    public class ParseObject
    {
        public CommandType command { get; set; }
        public string text { get; set; }
        public string name { get; set; }
        public string standing { get; set; }
    }
}

