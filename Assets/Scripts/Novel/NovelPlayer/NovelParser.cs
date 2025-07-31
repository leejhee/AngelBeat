using novel;
using System.Text.RegularExpressions;
using UnityEngine;

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
    private static Regex choiceCommand = new Regex(@"^@choice\s+(?<choice>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Regex hideCommand = new Regex(@"^@hide\s+(?<name>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex hideAllCommand = new Regex(@"^@hideall\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex hideCharsCommand = new Regex(@"^@hidechars\s+(?<name>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex hidePrinterCommand = new Regex(@"^@hideprinter\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex hideUICommand = new Regex(@"^@hideui\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Regex stopCommand = new Regex(@"^@stop\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex stopBGMCommand = new Regex(@"^@stopbgm\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex stopSFXCommand = new Regex(@"^@stopsfx\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex stopVoiceCommand = new Regex(@"^@stopvoice\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Regex waitCommand = new Regex(@"^@wait(?:\s+(?<time>[\d.]+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);


    //Command Parameter Regex
    private static Regex posPattern = new Regex(@"pos\s*:\s*(?<posX>[\d.]+)\s*,\s*(?<posY>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex scalePattern = new Regex(@"scale\s*:\s*(?<scale>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex timePattern = new Regex(@"time\s*:\s*(?<time>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex waitPattern = new Regex(@"(?<wait>!wait|wait!)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Regex volumePattern = new Regex(@"volume\s*:\s*(?<volume>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex loopPattern = new Regex(@"(?<loop>!loop|loop!)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex fadePattern = new Regex(@"fade\s*:\s*(?<fade>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Regex transitionPattern = new Regex(@"transition\s*:\s*(?<transition>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Regex subLinePattern = new Regex("^ {4}(?<argument>.*)", RegexOptions.Compiled);


    public static NovelAct Parse(string[] lines)
    {
        NovelAct act = new();
        int index = 1;

        foreach(string raw in lines)
        {
            string line = raw;

            if (string.IsNullOrEmpty(line)) continue;

            if (commentLine.IsMatch(line))
            {
                // 주석은 파싱 안함
                continue;
            }
            else if (subLinePattern.IsMatch(line))
            {
                string trimedLine = line.Trim();
                if (trimedLine == "" || trimedLine == null)
                {
                    continue;
                }


                // 비어있으면 컨티뉴
                if (string.IsNullOrEmpty(trimedLine)) continue;

                if (commentLine.IsMatch(trimedLine))
                {
                    // 주석은 파싱 안함
                    continue;
                }


                if (gotoCommand.IsMatch(trimedLine))
                {
                    var labelMatch = gotoCommand.Match(trimedLine);
                    string label = labelMatch.Groups["label"].Value;

                    Debug.Log(act.novelLines.Count - 1);

                    if (act.novelLines[act.novelLines.Count - 1] is ChoiceCommand choice)
                    {
                        choice.subLine = new GotoCommand(index, label);

                        Debug.Log($"SubLine Goto \nIndex : {index}, Label : {label}");
                    }
                }

                continue;
            }
            else if (labelLine.IsMatch(line))
            {
                var match = labelLine.Match(line);
                string labelName = match.Groups["name"].Value;
                NovelPlayer.Instance.labelDict.Add(labelName, index);
                act.novelLines.Add(new LabelLine(index, labelName));
                Debug.Log($"Label Name : {labelName}\nIndex : {index}");
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

                Debug.Log($"Person : {actorName}, Line : {actorLine}\nIndex : {index}");
            }

            else
            {
                string normalLine = line.Trim();
                if (normalLine == null || normalLine == "")
                {
                    continue;
                }
                act.novelLines.Add(new NormalLine(index, normalLine));

                Debug.Log($"Normal : {line}\nIndex : {index}");
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

            Debug.Log($"BackName : {backName}\nTransition : {transition}\npos : {pos}\nscale : {scale}\nTime : {time}\nWait : {wait}\nIndex : {index}");
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

            Debug.Log($"BGM : {bgmName}\nvolume : {volume}\nloop : {loop}\ntime : {time}\nfade : {fade}\nwait : {wait}\nIndex : {index}");
            act.novelLines.Add(new BgmCommand(index, bgmName, volume, time, fade, loop, wait));
        }
        else if (stopBGMCommand.IsMatch(line))
        {
            act.novelLines.Add(new BgmCommand(index, null, null, null, null, null, null, BGMCommandType.Stop));
            Debug.Log($"StopBGM\nIndex : {index}");
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

            Debug.Log($"Character : {name}\nPos : {pos}\nAppearance : {appearance}\nScale : {scale}\ntransition : {transition}\ntime : {time}\nwait : {wait}\nIndex : {index}");
            act.novelLines.Add(new CharCommand(index, name, appearance, transition, pos, scale, time, wait));
        }
        else if (hideCommand.IsMatch(line))
        {
            Debug.Log("hide command\nIndex : {index}");
        }
        else if (hideAllCommand.IsMatch(line))
        {
            Debug.Log("Hide All Command\nIndex : {index}");
            act.novelLines.Add(new CharCommand(index, null, null, null, null, null, null, null, CharCommandType.HideAll));
        }
        else if (choiceCommand.IsMatch(line))
        {
            string choice = null;
            var choiceMatch = choiceCommand.Match(line);
            if (choiceMatch.Success)
                choice = choiceMatch.Groups["choice"].Value;

            act.novelLines.Add(new ChoiceCommand(index, choice));
            Debug.Log($"Choice : {choice}\nIndex : {index}");
        }
        else if (waitCommand.IsMatch(line))
        {
            var waitMatch = waitCommand.Match(line);

            float waitTime = waitMatch.Groups["time"].Success ? float.Parse(waitMatch.Groups["time"].Value) : 0f;

            act.novelLines.Add(new WaitCommand(index, waitTime));
            Debug.Log($"Wait \nTime : {waitTime}\nIndex : {index}");
        }
        else if (gotoCommand.IsMatch (line))
        {
            var labelMatch = gotoCommand.Match(line);
            string label = labelMatch.Groups["label"].Value;

            act.novelLines.Add(new GotoCommand(index, label));
            Debug.Log($"Goto \nIndex : {index}, Label : {label}");
        }
    }
}