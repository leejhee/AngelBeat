using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public class WaitCommand : CommandLine
    {
        float? waitTime;
        public WaitCommand(int index, float? waitTime) : base(index, DialogoueType.CommandLine)
        {
            this.waitTime = waitTime;
        }

        public override async UniTask Execute()
        {
            var player = NovelManager.Player;

            if (waitTime == null)
            {
                player.SetHardWait(true);
                player.SetWaitForTime(false);
                return;
            }

            float realTime = Mathf.Max(0f, waitTime.Value);
            player.SetHardWait(false);
            player.SetWaitForTime(true);

            try
            {
                await UniTask
                    .Delay(TimeSpan.FromSeconds(realTime), DelayType.DeltaTime, PlayerLoopTiming.Update, player.CommandToken)
                    .AttachExternalCancellation(player.CommandToken);
            }
            catch (OperationCanceledException)
            {
                // 클릭으로 취소됨
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                player.SetWaitForTime(false);
                player.ContinueFromWait();
            }

        }

    }

}
