using nonvel;
using novel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public static class NovelParser
{
    private static Regex labelLine = new Regex(@"^#(?<name>.+?)\s*$");
    private static Regex commentLine = new Regex(@"^//.*$");
    private static Regex commandLine = new Regex(@"^@\s*");
    private static Regex personLine = new Regex(@"(?<name>.+?)\s*:\s*(?<line>.+)\s*$");

    // 각 인수 별 우선순위
    private static Regex backCommand = new Regex(@"^@back\s+(?<name>\w+)(\.(?<transition>\w+))?" + 
                                                @"(\s+pos\s*:\s*(?<posX>[\d.]+)\s*,\s*(?<posY>[\d.]+))?(\s+scale\s*:\s*(?<scale>[\d.]+))?" + 
                                                @"(\s+time\s*:\s*(?<time>[\d.]+))?(\s+(?<wait>!wait|wait!))?", RegexOptions.Compiled);
    
    
    public static NovelAct Parse(string[] lines)
    {
        NovelAct act = new();
        int index = 1;

        foreach(string raw in lines)
        {
            string line = raw.Trim();

            if (string.IsNullOrEmpty(line)) continue;

            if (commentLine.IsMatch(line))
            {
                // 주석은 파싱 안함
                continue;
            }
            else if (labelLine.IsMatch(line))
            {
                var match = labelLine.Match(line);
                string labelName = match.Groups["name"].Value;
                act.novelLines.Add(new LabelLine(index, labelName));
                Debug.Log($"Label Name : {labelName}");
            }
            else if (commandLine.IsMatch(line))
            {
                ParseCommand(act, index, line);
            }
            else if (personLine.IsMatch(line))
            {
                var match = personLine.Match(line);
                string actorName = match.Groups["name"].Value;
                string actorLine = match.Groups["line"].Value;
                act.novelLines.Add(new PersonLine(index, actorName, actorLine));

                Debug.Log($"Person : {actorName}, Line : {actorLine}");
            }
            else
            {
                act.novelLines.Add(new NormalLine(index, line));
                Debug.Log($"Normal : {line}");
            }
            index++;

        }
        Debug.Log("파싱 끝");
        return act; 
    }

    private static void ParseCommand(NovelAct act, int index, string line)
    {
        if (backCommand.IsMatch(line))
        {
            var match = backCommand.Match(line);
            string backName = match.Groups["name"].Value;
            string transition = match.Groups["transition"].Value;
            Vector2? pos = null;
            if (match.Groups["posX"].Success && match.Groups["posY"].Success)
            {
                Debug.Log("이거 작동 안해?");
                float x = float.Parse(match.Groups["posX"].Value);
                float y = float.Parse(match.Groups["posY"].Value);
                pos = new Vector2(x, y);
            }

            Debug.Log(backName);
            Debug.Log(transition);
            Debug.Log(pos);


            float? scale = float.Parse(match.Groups["scale"].Value);
            float? time = float.Parse(match.Groups["time"].Value);

            bool? wait = null;

            if (match.Groups["wait"].Success)
            {
                var waitStr = match.Groups["wait"].Value;
                if (waitStr == "!wait")
                    wait = true;
                else if (waitStr == "wait!")
                    wait = false;
            }

            Debug.Log($"BackName : {backName}\nTransition : {transition}\npos : {pos}\nscale : {scale}\nTime : {time}\nWait : {wait}");
            act.novelLines.Add(new BackCommand(index, backName, transition, pos, scale, time, wait));
        }
    }
}