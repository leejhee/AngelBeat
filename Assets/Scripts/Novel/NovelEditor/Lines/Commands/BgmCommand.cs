using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public class BgmCommand : CommandLine
    {
        public string bgmName;
        public int? volume;
        public float? time;
        public float? fade;
        public bool? loop;
        public bool? wait;

        public BgmCommand(int index, string bgmName, int? volume, float? time, float? fade, bool? loop, bool? wait) : base(index, DialogoueType.CommandLine)
        {
            this.bgmName = bgmName;
            this.volume = volume;
            this.time = time;   
            this.fade = fade;
            this.loop = loop;
            this.wait = wait;
        }

        public override void Execute()
        {
            string path = $"Novel/NovelResourceData/SoundData/BGMData/{bgmName}";
            SoundManager.Instance.Play(path, SystemEnum.Sound.Bgm);
        }
    }

}