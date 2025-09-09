using Cysharp.Threading.Tasks;
using novel;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
namespace novel
{
    public static class NovelUtils
    {
        public static bool ConditinalStateMent(float left, CompOP op, float right)
        {

            switch (op)
            {
                case CompOP.GreaterThan:
                    if (left > right)
                    {
                        return true;
                    }
                    break;
                case CompOP.LessThan:
                    if (left < right)
                    {
                        return true;
                    }
                    break;
                case CompOP.GreaterThanOrEqual:
                    if (left >= right)
                    {
                        return true;
                    }
                    break;
                case CompOP.LessThanOrEqual:
                    if (left <= right)
                    {
                        return true;
                    }
                    break;
                case CompOP.Equal:
                    if (left == right)
                    {
                        return true;
                    }
                    break;
                case CompOP.NotEqual:
                    if (left != right)
                    {
                        return true;
                    }
                    break;
                default:
                    Debug.LogError("Error : 정의되지 않은 연산자");
                    break;
            }

            return false;
        }
        /// <summary>
        /// GameObject나 자식 전체를 CanvasGroup 기반으로 페이드.
        /// 없으면 CanvasGroup을 자동 추가.
        /// </summary>
        public static async UniTask Fade(GameObject go, float duration, bool fade, CancellationToken token = default)
        {
            if (go == null) return;

            var cg = go.GetComponent<CanvasGroup>();
            if (!cg) cg = go.gameObject.AddComponent<CanvasGroup>();

            float target = fade ? 1f : 0f;

            if (!go.activeSelf)
            {
                if (fade)
                {
                    cg.alpha = 0f;
                    go.SetActive(true);
                }
            }

            float start = cg.alpha;

            if (duration <= 0f || Mathf.Approximately(start, target))
            {
                cg.alpha = target;
                return;
            }

            float t = 0f;
            while (t < duration)
            {
                if (token.IsCancellationRequested)
                {
                    cg.alpha = target;
                    return;
                }
                t += UnityEngine.Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(t / duration));
                await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming.Update); // 토큰 X

            }
            cg.alpha = target;
        }


    }

}
