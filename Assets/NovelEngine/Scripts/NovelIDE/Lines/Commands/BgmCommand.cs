
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
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
        public BGMCommandType commandType;

        public BgmCommand(int index, string bgmName, int? volume, float? time, float? fade,bool? loop,
                             BGMCommandType commandType = BGMCommandType.Play) : base(index, DialogoueType.CommandLine)
        {
            this.bgmName = bgmName;
            this.volume = volume;
            this.time = time;
            this.fade = fade;
            this.loop = loop;
            this.commandType = commandType;
        }

        public override async UniTask Execute()
        {
            // 새로운 노벨엔진 전용 믹서, 사운드 매니저(필요한가?) 사용해서 만들기

            //if (commandType == BGMCommandType.Stop)
            //{
            //    Debug.Log("BGM 중단");
            //    SoundManager.Instance.StopBGM();
            //    return;
            //}
            //string path = $"Novel/NovelResourceData/SoundData/BGMData/{bgmName}";
            //SoundManager.Instance.Play(path, SystemEnum.Sound.Bgm);
        }
    }

}