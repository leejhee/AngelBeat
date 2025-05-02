using novel;
using System;
using System.Collections;
using System.Collections.Generic;

public static class NovelParser
{
    public static NovelAct Parse(string[] lines)
    {
        NovelAct act = new();
        int index = 0;

        foreach (string raw in lines)
        {
            string line = raw.Trim();

            if (string.IsNullOrEmpty(line)) continue;

            if (line.StartsWith("#"))
            {
                act.novelLines.Add(new LabelLine(line.Substring(1)) { index = index++ });
            }
            else if (line.StartsWith("@"))
            {
                act.novelLines.Add(new CommandLine() { index = index++ });
            }
            else if (line.Contains(":"))
            {
                int colonIndex = line.IndexOf(':');
                string name = line.Substring(0, colonIndex).Trim();
                string content = line.Substring(colonIndex + 1).Trim();

                act.novelLines.Add(new PersonLine(name, content) { index = index++});
            }
            else if (line.StartsWith("//"))
            {
                continue;
            }
            else
            {
                act.novelLines.Add(new NormalLine(line) { index = index++});
            }
        }

        return act; 
    }

    private static void ParseCommand()
    {

    }
}