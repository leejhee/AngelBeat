using novel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nonvel
{
    public class BackCommand : CommandLine
    {
        public string backName;
        public string transition;
        public Vector2? pos;
        public float? scale;
        public float? time;
        public bool? wait;

        public BackCommand(int index, string backName,  string transition, Vector2? pos, float? scale, float? time, bool? wait) : base(index, DialogoueType.CommandLine)
        {
            this.backName = backName;
            this.transition = transition;
            this.pos = pos;
            this.scale = scale;
            this.time = time;
            this.wait = wait;
        }

        public override void Execute()
        {
            throw new System.NotImplementedException();
        }
    }

}

