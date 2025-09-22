using novel;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;

public static class NovelParser
{
    #region 정규표현식 정의
    private static Regex labelLine = new Regex(@"^#(?<name>.+?)\s*$");
    private static Regex commentLine = new Regex(@"^//.*$");
    private static Regex commandLine = new Regex(@"^@\s*");
    private static Regex personLine = new Regex(@"(?<name>.+?)\s*:\s*(?<line>.+)\s*$");

    #region 커맨드 정규식
    private static Regex backCommand = new Regex(@"^@back\s+(?<name>\w+)(\.(?<transition>\w+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex bgmCommand = new Regex(@"^@bgm\s+(?<name>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex sfxCommand = new Regex(@"^@sfx\s+(?<name>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex charCommand = new Regex(@"^@char\s+(?<name>\w+)(\.(?<appearance>\w+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex gotoCommand = new Regex(@"^@goto\s+(?<label>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex choiceCommand = new Regex(@"^@choice\s+(?<choice>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex ifCommand = new Regex(@"^@if\s+(?<var>\w+)\s*(?<op>>=|<=|==|!=|>|<)\s*(?<value>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex elseCommand = new Regex(@"^@else", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
    #endregion

    #region 커맨드 매개변수 정규식
    private static Regex posPattern = new Regex(@"pos\s*:\s*(?<posX>[\d.]+)\s*,\s*(?<posY>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex scalePattern = new Regex(@"scale\s*:\s*(?<scale>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex timePattern = new Regex(@"time\s*:\s*(?<time>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex ifPattern = new Regex(@"if\s*:\s*(?<var>\w+)\s*(?<op>>=|<=|==|!=|>|<)\s*(?<value>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Regex volumePattern = new Regex(@"volume\s*:\s*(?<volume>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex loopPattern = new Regex(@"(?<loop>!loop|loop!)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex fadePattern = new Regex(@"fade\s*:\s*(?<fade>[\d.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Regex transitionPattern = new Regex(@"transition\s*:\s*(?<transition>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Regex subLinePattern = new Regex("^ {4}(?<argument>.*)", RegexOptions.Compiled);
    #endregion
    #endregion
    static int parseCound = 0;
    public static NovelAct Parse(string[] lines)
    {
        Debug.Log($"파싱 시작 {++parseCound}"); 

        NovelAct act = new();
        act.novelLines = new();
        List<NovelLine> novelLines = new();
        int index = 1;

        foreach(string raw in lines)
        {
            NovelLine line = ParseLine(act, raw, index, novelLines);
            if (line != null)
            {
                novelLines.Add(line);

                index++;
            }

        }
        act.novelLines = novelLines;
        Debug.Log("파싱 끝");
        return act; 
    }
    private static DialogoueType LineType(string line)
    {
        if (commentLine.IsMatch(line))
            return DialogoueType.CommentLine;
        else if (labelLine.IsMatch(line))
            return DialogoueType.LabelLine;
        else if (commandLine.IsMatch(line))
            return DialogoueType.CommandLine;
        else if (personLine.IsMatch(line))
            return DialogoueType.PersonLine;
        else
            return DialogoueType.NormalLine;
    }
    private static NovelLine ParseLine(NovelAct act, string line, int index, List<NovelLine> lineList = null)
    {
        NovelLine result = null;
        if (subLinePattern.IsMatch(line))
        {
            // 공백4개 제거
            line = line.Substring(4);

            if (lineList[index - 2] is CommandLine command)
            {
                var subLine = ParseLine(act, line, index - 1);
                if (subLine != null)
                {
                    command.subLine = subLine;
                    Debug.Log($"Subline of {command.GetType().Name} at index {index - 1} parsed.");
                }
            }
            // 서브라인 파싱은 직접 넣어주고 아무것도 리턴하지 않음
            return null;
        }
        else
        {
            DialogoueType type = LineType(line);
            switch (type)
            {
                case DialogoueType.CommandLine:
                    result = ParseCommand(index, line);
                    break;
                case DialogoueType.NormalLine:
                    string normalLine = line.Trim();
                    if (normalLine == null || normalLine == "")
                    {
                        return null;
                    }
                    Debug.Log($"Normal : {line}\nIndex : {index}");

                    result = new NormalLine(index, normalLine);
                    break;
                case DialogoueType.PersonLine:
                    var personMatch = personLine.Match(line);
                    string actorName = personMatch.Groups["name"].Value;
                    string actorLine = personMatch.Groups["line"].Value;

                    Debug.Log($"Person : {actorName}, Line : {actorLine}\nIndex : {index}");
                    result = new PersonLine(index, actorName, actorLine);
                    break;
                case DialogoueType.LabelLine:
                    var labelMatch = labelLine.Match(line);
                    string labelName = labelMatch.Groups["name"].Value;

                    NovelManager.Player.labelDict.Add(labelName, index);
                    Debug.Log($"Label Name : {labelName}\nIndex : {index}");
                    result = new LabelLine(index, labelName);
                    break;
            }
        }
        if (result == null)
        {
            Debug.LogWarning($"파싱 실패 : {line} at {index} line" );
            return null;
        }
        else
            return result;
    }
    private static CommandType GetCommandType(string line)
    {
        if (charCommand.IsMatch(line))
            return CommandType.ShowCharacter;
        else if (hideCommand.IsMatch(line))
            return CommandType.HideCharacter;
        else if (hideAllCommand.IsMatch(line))
            return CommandType.HideAll;

        else if (choiceCommand.IsMatch(line))
            return CommandType.Choice;
        else if (gotoCommand.IsMatch(line))
            return CommandType.Goto;
        else if (ifCommand.IsMatch(line))
            return CommandType.If;
        else if (elseCommand.IsMatch(line))
            return CommandType.Else;
        else if (waitCommand.IsMatch(line))
            return CommandType.Wait;

        else if (backCommand.IsMatch(line))
            return CommandType.Background;

        else if (bgmCommand.IsMatch(line))
            return CommandType.BGM;
        else if (stopBGMCommand.IsMatch(line))
            return CommandType.StopBGM;
        else if (sfxCommand.IsMatch(line))
            return CommandType.SFX;
        else
            return CommandType.None;
    }
    private static CompOP ParseCompOP(string opStr)
    {
        return opStr switch
        {
            ">" => CompOP.GreaterThan,
            "<" => CompOP.LessThan,
            ">=" => CompOP.GreaterThanOrEqual,
            "<=" => CompOP.LessThanOrEqual,
            "==" => CompOP.Equal,
            "!=" => CompOP.NotEqual,
            _ => throw new ArgumentException($"Invalid operator: {opStr}")
        };
    }
    private static IfParameter parseIfParameter(string line)
    {
        var ifMatch = ifPattern.Match(line);
        IfParameter ifParameter = new();
        if (ifMatch.Success)
        {
            ifParameter = new IfParameter(
                ifMatch.Groups["var"].Value,
                ParseCompOP(ifMatch.Groups["op"].Value),
                float.Parse(ifMatch.Groups["value"].Value));
        }
        return ifParameter;
    }
    private static NovelLine ParseCommand(int index, string line)
    {
        CommandType commandType = GetCommandType(line);
        NovelLine result = null;

        switch (commandType)
        {
            case CommandType.None:
                Debug.LogWarning($"Unknown command at line {index}: {line}");
                break;
            case CommandType.ShowCharacter:
                {
                    var charMatch = charCommand.Match(line);
                    string charName = charMatch.Groups["name"].Value;
                    string charAppearance = charMatch.Groups["appearance"].Value.ToLower();

                    Vector2? charPos = null;
                    var charPosMatch = posPattern.Match(line);
                    if (charPosMatch.Success)
                    {
                        float x = float.Parse(charPosMatch.Groups["posX"].Value);
                        float y = float.Parse(charPosMatch.Groups["posY"].Value);
                        charPos = new Vector2(x, y);
                    }

                    float? charScale = null;
                    var charScaleMatch = scalePattern.Match(line);
                    if (charScaleMatch.Success)
                        charScale = float.Parse(charScaleMatch.Groups["scale"].Value);

                    string charTransition = null;
                    var charTransitionMatch = transitionPattern.Match(line);
                    if (charTransitionMatch.Success)
                        charTransition = charTransitionMatch.Groups["transition"].Value.ToLower();

                    float? charTime = null;
                    var charTimeMatch = timePattern.Match(line);
                    if (charTimeMatch.Success)
                        charTime = float.Parse(charTimeMatch.Groups["time"].Value);

                    CharCommandType charCommandType = CharCommandType.Show;
                    if (charTransition == "fadeout")
                    {
                        charCommandType = CharCommandType.Hide;
                    }

                    IfParameter ifParameter = parseIfParameter(line);

                    result = new CharCommand(index, charName, charAppearance, charTransition, charPos, charScale, charTime, charCommandType, ifParameter);
                    Debug.Log(
                        $"Character : {charName}\n" +
                        $"Pos : {charPos}\n" +
                        $"Appearance : {charAppearance}\n" +
                        $"Scale : {charScale}\n" +
                        $"transition : {charTransition}\n" +
                        $"Type : {charCommandType}\n" +
                        $"time : {charTime}\n" +
                        $"Index : {index}\n"+
                        $"If : {ifParameter.var} {ifParameter.op} {ifParameter.value}");
                }
                break;
            case CommandType.HideCharacter:
                {
                    var hideMatch = hideCommand.Match(line);
                    string name = hideMatch.Groups["name"].Value;

                    IfParameter ifParameter = parseIfParameter(line);

                    result = new CharCommand(index, name, null, null, null, null, null, CharCommandType.Hide, ifParameter);
                    Debug.Log(
                        $"hide command\nCharacter : {name}\n" +
                        $"Index : {index}\n" +
                        $"If : {ifParameter.var} {ifParameter.op} {ifParameter.value}");
                }
                break;
            case CommandType.HideAll:
                {
                    IfParameter ifParameter = parseIfParameter(line);

                    result = new CharCommand(index, null, null, null, null, null, null, CharCommandType.HideAll, ifParameter);
                    Debug.Log($"Hide All Command\n" +
                        $"Index : {index}\n" +
                        $"If : {ifParameter.var} {ifParameter.op} {ifParameter.value}");
                }
                break;
            case CommandType.Clearall:
                break;
            case CommandType.Choice:
                {
                    string choice = null;
                    var choiceMatch = choiceCommand.Match(line);
                    if (choiceMatch.Success)
                        choice = choiceMatch.Groups["choice"].Value;

                    var ifMatch = ifPattern.Match(line);
                    if (ifMatch.Success)
                    {
                        var ifString = ifMatch.Value;
                        choice = choice.Replace(ifString, "").Trim();
                    }


                    IfParameter ifParameter = parseIfParameter(line);

                    result = new ChoiceCommand(index, choice, ifParameter);


                    Debug.Log(
                        $"Choice : {choice}\n" +
                        $"Index : {index}\n" +
                        $"If : {ifParameter.var} {ifParameter.op} {ifParameter.value}");
                }
                break;
            case CommandType.Goto:
                {
                    var gotoMatch = gotoCommand.Match(line);
                    string label = gotoMatch.Groups["label"].Value;

                    IfParameter ifParameter = parseIfParameter(line);

                    result = new GotoCommand(index, label, ifParameter);
                    Debug.Log(
                        $"Goto : {label}\n" +
                        $"Index : {index}\n" +
                        $"If : {ifParameter.var} {ifParameter.op} {ifParameter.value}");
                }
                break;
            case CommandType.If:
                {
                    var ifMatch = ifCommand.Match(line);
                    string var = ifMatch.Groups["var"].Value;
                    CompOP op = ParseCompOP(ifMatch.Groups["op"].Value);
                    float value = float.Parse(ifMatch.Groups["value"].Value);

                    IfParameter ifParameter = parseIfParameter(line);

                    result = new IfCommand(index, IfType.If, var, op, value);
                    Debug.Log($"If Command : {var} {op} {value}");
                }
                break;
            case CommandType.Else:
                {
                    IfParameter ifParameter = parseIfParameter(line);
                    result = new IfCommand(index, IfType.Else);
                    Debug.Log($"Else Command :");
                }
                break;
            case CommandType.Wait:
                {
                    var waitMatch = waitCommand.Match(line);

                    float? waitTime = waitMatch.Groups["time"].Success ? float.Parse(waitMatch.Groups["time"].Value) : null;
                    
                    IfParameter ifParameter = parseIfParameter(line);

                    result = new WaitCommand(index, waitTime, ifParameter);
                    Debug.Log(
                        $"Wait \n" +
                        $"Time : {waitTime}\n" +
                        $"Index : {index}" +
                        $"If : {ifParameter.var} {ifParameter.op} {ifParameter.value}");
                }
                break;
            case CommandType.Background:
                {
                    var match = backCommand.Match(line);

                    string backName = match.Groups["name"].Value;
                    string backgroundTransition = match.Groups["transition"].Value;

                    Vector2? backgroundPos = null;
                    var backgroundPosMatch = posPattern.Match(line);
                    if (backgroundPosMatch.Success)
                    {
                        float x = float.Parse(backgroundPosMatch.Groups["posX"].Value);
                        float y = float.Parse(backgroundPosMatch.Groups["posY"].Value);
                        backgroundPos = new Vector2(x, y);
                    }

                    float? backgroundScale = null;
                    var backgroundScaleMatch = scalePattern.Match(line);
                    if (backgroundScaleMatch.Success)
                        backgroundScale = float.Parse(backgroundScaleMatch.Groups["scale"].Value);

                    float? backgroundTime = null;
                    var backgroundTimeMatch = timePattern.Match(line);
                    if (backgroundTimeMatch.Success)
                        backgroundTime = float.Parse(backgroundTimeMatch.Groups["time"].Value);

                    IfParameter ifParameter = parseIfParameter(line);

                    result = new BackCommand(
                        index, 
                        backName, 
                        backgroundTransition, 
                        backgroundPos,
                        backgroundScale, 
                        backgroundTime,
                        ifParameter);


                    Debug.Log(
                        $"BackName : {backName}\n" +
                        $"Transition : {backgroundTransition}\n" +
                        $"pos : {backgroundPos}\n" +
                        $"scale : {backgroundScale}\n" +
                        $"Time : {backgroundTime}\n" +
                        $"Index : {index}" +
                        $"If : {ifParameter.var} {ifParameter.op} {ifParameter.value}");

                }

                break;
            case CommandType.BGM:
                {
                    var bgmMatch = bgmCommand.Match(line);
                    string bgmName = bgmMatch.Groups["name"].Value;

                    int? volume = null;
                    var volumeMatch = volumePattern.Match(line);
                    if (volumeMatch.Success)
                        volume = int.Parse(volumeMatch.Groups["volume"].Value);

                    bool loop = false;
                    var loopMatch = loopPattern.Match(line);
                    if (loopMatch.Success)
                    {
                        var loopStr = loopMatch.Groups["loop"].Value;
                        if (loopStr == "!loop")
                            loop = true;
                    }


                    IfParameter ifParameter = parseIfParameter(line);

                    result = new SoundCommand(index, bgmName, volume, loop, true, NovelSound.Bgm ,ifParameter);
                    Debug.Log(
                        $"BGM : {bgmName}\n" +
                        $"volume : {volume}\n" +
                        $"loop : {loop}\n" +
                        $"Index : {index}" +
                        $"If : {ifParameter.var} {ifParameter.op} {ifParameter.value}");

                }

                break;
            case CommandType.StopBGM:

                {
                    IfParameter ifParameter = parseIfParameter(line);
                    result = new SoundCommand(index, null, null, false, false, NovelSound.Bgm,ifParameter);
                    Debug.Log(
                        $"StopBGM\n" +
                        $"Index : {index}" +
                        $"If : {ifParameter.var} {ifParameter.op} {ifParameter.value}");
                }

                break;
            case CommandType.SFX:
                {
                    var sfxMatch = bgmCommand.Match(line);
                    string sfxName = sfxMatch.Groups["name"].Value;

                    int? volume = null;
                    var volumeMatch = volumePattern.Match(line);
                    if (volumeMatch.Success)
                        volume = int.Parse(volumeMatch.Groups["volume"].Value);


                    bool loop = false;
                    var loopMatch = loopPattern.Match(line);
                    if (loopMatch.Success)
                    {
                        var loopStr = loopMatch.Groups["loop"].Value;
                        if (loopStr == "!loop")
                            loop = true;
                    }

                    IfParameter ifParameter = parseIfParameter(line);
                    result = new SoundCommand(index, sfxName, volume, loop, true, NovelSound.Effect, ifParameter);
                    Debug.Log(
                        $"SFX : {sfxName}\n" +
                        $"volume : {volume}\n" +
                        $"loop : {loop}\n" +
                        $"Index : {index}" +
                        $"If : {ifParameter.var} {ifParameter.op} {ifParameter.value}");
                }
                break;
            case CommandType.StopSFX:
                {
                    IfParameter ifParameter = parseIfParameter(line);
                    result = new SoundCommand(index, null, null, false, false, NovelSound.Effect, ifParameter);
                    Debug.Log(
                        $"StopSFX\n" +
                        $"Index : {index}" +
                        $"If : {ifParameter.var} {ifParameter.op} {ifParameter.value}");
                }
                break;
            case CommandType.Effect:
                break;
        }

        if (result == null)
        {
            return null;
        }
        else
            return result;

    }
    public class IfParameter
    {
        public string var;
        public CompOP op;
        public float? value;
        public IfParameter()
        {
            this.var = null;
            this.op = CompOP.None;
            this.value = null;
        }
        public IfParameter(string var, CompOP op, float value)
        {
            this.var = var;
            this.op = op;
            this.value = value;
        }

    }
}