using novel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;
using UnityEngine.UIElements;
public static class NovelParser
{
    private static Regex labelLine = new Regex(@"^#(?<name>.+?)\s*$");
    private static Regex commentLine = new Regex(@"^//.*$");
    private static Regex commandLine = new Regex(@"^@\s*");
    private static Regex personLine = new Regex(@"(?<name>.+?)\s*:\s*(?<line>.+)\s*$");

    //Command Regex
    private static Regex backCommand = new Regex(@"^@back\s+(?<name>\w+)(\.(?<transition>\w+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex bgmCommand = new Regex(@"^@bgm\s+(?<name>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex sfxCommand = new Regex(@"^@sfx\s+(?<name>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex charCommand = new Regex(@"^@char\s+(?<name>\w+)(\.(?<appearance>\w+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex gotoCommand = new Regex(@"^@goto\s+(?<label>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Regex hideCommand = new Regex(@"^@hide\s+(?<name>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex hideAllCommand = new Regex(@"^@hideall\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex hideCharsCommand = new Regex(@"^@hidechars\s+(?<name>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex hidePrinterCommand = new Regex(@"^@hideprinter\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex hideUICommand = new Regex(@"^@hideui\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Regex stopCommand = new Regex(@"^@stop\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex stopBGMCommand = new Regex(@"^@stopbgm\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex stopSFXCommand = new Regex(@"^@stopsfx\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex stopVoiceCommand = new Regex(@"^@stopvoice\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);


    //Command Parameter Regex
    private static Regex posPattern = new Regex(@"pos\s*:\s*(?<posX>[\d.]+)\s*,\s*(?<posY>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex scalePattern = new Regex(@"scale\s*:\s*(?<scale>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex timePattern = new Regex(@"time\s*:\s*(?<time>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex waitPattern = new Regex(@"(?<wait>!wait|wait!)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Regex volumePattern = new Regex(@"volume\s*:\s*(?<volume>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex loopPattern = new Regex(@"(?<loop>!loop|loop!)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex fadePattern = new Regex(@"fade\s*:\s*(?<fade>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Regex transitionPattern = new Regex(@"transition\s*:\s*(?<transition>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);


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
                //act.novelLines.Add(new LabelLine(index, labelName));
                NovelPlayer.Instance.labelDict.Add(labelName, index);
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
            var PosMatch = posPattern.Match(line);
            if (PosMatch.Success)
            {
                float x = float.Parse(PosMatch.Groups["posX"].Value);
                float y = float.Parse(PosMatch.Groups["posY"].Value);
                pos = new Vector2(x, y);
            }

            float? scale = null;
            var scaleMatch = scalePattern.Match(line);
            if (scaleMatch.Success)
                scale = float.Parse(scaleMatch.Groups["scale"].Value);

            float? time = null;
            var timeMatch = timePattern.Match(line);
            if (timeMatch.Success)
                time = float.Parse(timeMatch.Groups["time"].Value);

            bool? wait = null;
            var waitMatch = waitPattern.Match(line);
            if (waitMatch.Success)
            {
                var waitStr = waitMatch.Groups["wait"].Value;
                if (waitStr == "!wait")
                    wait = true;
                else if (waitStr == "wait!")
                    wait = false;
            }

            Debug.Log($"BackName : {backName}\nTransition : {transition}\npos : {pos}\nscale : {scale}\nTime : {time}\nWait : {wait}");
            act.novelLines.Add(new BackCommand(index, backName, transition, pos, scale, time, wait));
        }
        else if (bgmCommand.IsMatch(line))
        {
            var bgmMatch = bgmCommand.Match(line);
            string bgmName = bgmMatch.Groups["name"].Value;

            int? volume = null;
            var volumeMatch = volumePattern.Match(line);
            if (volumeMatch.Success)
                volume = int.Parse(volumeMatch.Groups["volume"].Value);

            bool? loop = null;
            var loopMatch = loopPattern.Match(line);
            if (loopMatch.Success)
            {
                var loopStr = loopMatch.Groups["loop"].Value;
                if (loopStr == "!loop")
                    loop = true;
                else if (loopStr == "loop!")
                    loop = false;
            }

            float? time = null;
            var timeMatch = timePattern.Match(line);
            if (timeMatch.Success)
                time = float.Parse(timeMatch.Groups["time"].Value);

            float? fade = null;
            var fadeMatch = fadePattern.Match(line);
            if (fadeMatch.Success)
                fade = float.Parse(fadeMatch.Groups["fade"].Value);

            bool? wait = null;
            var waitMatch = waitPattern.Match(line);
            if (waitMatch.Success)
            {
                var waitStr = waitMatch.Groups["wait"].Value;
                if (waitStr == "!wait")
                    wait = true;
                else if (waitStr == "wait!")
                    wait = false;
            }

            Debug.Log($"BGM : {bgmName}\nvolume : {volume}\nloop : {loop}\ntime : {time}\nfade : {fade}\nwait : {wait}");
            act.novelLines.Add(new BgmCommand(index, bgmName, volume, time, fade, loop, wait));
        }
        else if (stopBGMCommand.IsMatch(line))
        {
            act.novelLines.Add(new BgmCommand(index, null, null, null, null, null, null, BGMCommandType.Stop));
            Debug.Log($"StopBGM");
        }
        else if (charCommand.IsMatch(line))
        {
            var charMatch = charCommand.Match(line);
            string name = charMatch.Groups["name"].Value;
            string appearance = charMatch.Groups["appearance"].Value;

            Vector2? pos = null;
            var PosMatch = posPattern.Match(line);
            if (PosMatch.Success)
            {
                float x = float.Parse(PosMatch.Groups["posX"].Value);
                float y = float.Parse(PosMatch.Groups["posY"].Value);
                pos = new Vector2(x, y);
            }

            float? scale = null;
            var scaleMatch = scalePattern.Match(line);
            if (scaleMatch.Success)
                scale = float.Parse(scaleMatch.Groups["scale"].Value);

            string transition = null;
            var transitionMatch = transitionPattern.Match(line);
            if (transitionMatch.Success)
                transition = transitionMatch.Groups["transition"].Value;

            float? time = null;
            var timeMatch = timePattern.Match(line);
            if (timeMatch.Success)
                time = float.Parse(timeMatch.Groups["time"].Value);

            bool? wait = null;
            var waitMatch = waitPattern.Match(line);
            if (waitMatch.Success)
            {
                var waitStr = waitMatch.Groups["wait"].Value;
                if (waitStr == "!wait")
                    wait = true;
                else if (waitStr == "wait!")
                    wait = false;
            }

            Debug.Log($"Character : {name}\nPos : {pos}\nAppearance : {appearance}\nScale : {scale}\ntransition : {transition}\ntime : {time}\nwait : {wait}");
            act.novelLines.Add(new CharCommand(index, name, appearance, transition, pos, scale, time, wait));
        }
        else if (hideCommand.IsMatch(line))
        {
            Debug.Log("hide command");
        }
        else if (hideAllCommand.IsMatch(line))
        {
            Debug.Log("Hide All Command");
            act.novelLines.Add(new CharCommand(index, null, null, null, null, null, null, null, CharCommandType.HideAll));
        }
    }
}