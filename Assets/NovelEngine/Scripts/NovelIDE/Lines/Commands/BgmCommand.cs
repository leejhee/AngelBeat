
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NovelParser;

namespace novel
{
    [System.Serializable]
    public class SoundCommand : CommandLine
    {
        public string bgmName;
        public int? volume;
        public bool loop;
        public bool isPlay;
        public SoundType soundType;

        public SoundCommand(
            int index,
            string bgmName,
            int? volume, 
            bool loop,
            bool isPlay,
            SoundType soundType,
            IfParameter ifParameter = null)
            : base(index, DialogoueType.CommandLine)
        {
            this.bgmName = bgmName;
            this.volume = volume;
            this.loop = loop;
            this.isPlay = isPlay;
            this.soundType = soundType;
            this.ifParameter = ifParameter;
        }

        public override async UniTask Execute()
        {
            //if (!ifParameter)
            //    return;

            // 따로 사운드 관리하는거 만들기
            switch (soundType)
            {
                case SoundType.BGM:
                    if (isPlay)
                    {
                        // BGM 재생
                        //SoundManager.Instance.PlayBGM(bgmName, volume ?? 100, loop, fade ?? 0f);
                    }
                    else
                    {
                        // BGM 중단
                        //SoundManager.Instance.StopBGM(fade ?? 0f);
                    }
                    break;
                case SoundType.SFX:
                    if (isPlay)
                    {
                        // SFX 재생
                        //SoundManager.Instance.PlaySFX(bgmName, volume ?? 100, loop);
                    }
                    else
                    {
                        // SFX 중단
                        //SoundManager.Instance.StopSFX(bgmName);
                    }
                    break;
            }

        }
    }

}