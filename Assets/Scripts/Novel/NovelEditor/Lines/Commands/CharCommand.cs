using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public class CharCommand : CommandLine
    {
        string charName;
        string appearance;
        string transition;
        Vector2? pos;
        float? scale;
        float? time;
        bool? wait;
        public CharCommand(int index, string name, string appearance, string transition, Vector2? pos, float? scale, float? time, bool? wait) : base(index, DialogoueType.CommandLine)
        {
            this.charName = name;
            this.appearance = appearance;
            this.transition = transition;
            this.pos = pos;
            this.scale = scale;
            this.time = time;
            this.wait = wait;
        }

        public override void Execute()
        {

        }
    }
}

