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
        public BGMCommandType commandType;

        public BgmCommand(int index, string bgmName, int? volume, float? time, float? fade, bool? loop, bool? wait, int depth = 0, BGMCommandType commandType = BGMCommandType.Play) : base(index, DialogoueType.CommandLine, depth)
        {
            this.bgmName = bgmName;
            this.volume = volume;
            this.time = time;
            this.fade = fade;
            this.loop = loop;
            this.wait = wait;
            this.commandType = commandType;
        }

        public override void Execute()
        {
            if (commandType == BGMCommandType.Stop)
            {
                Debug.Log("BGM 중단");
                SoundManager.Instance.StopBGM();
                return;
            }
            string path = $"Novel/NovelResourceData/SoundData/BGMData/{bgmName}";
            SoundManager.Instance.Play(path, SystemEnum.Sound.Bgm);
        }
        public override bool? IsWait()
        {
            return this.wait;
        }
    }

}