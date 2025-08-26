using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using novel;

public static class NovelEngine
{
    private static async Task<NovelManager> EnsureReady()
    {
        var mgr = await NovelManager.EnsureInitialized();
        await mgr.Initialization;
        return mgr;
    }


    public static async void InitEngine()
    {
        await EnsureReady();
    }
    public static async Task PlayNovel(string name)
    {
        var mgr = await EnsureReady();

        if (!mgr.IsReady)
        {
            Debug.LogError("[NovelEngine] NovelManager 초기화 실패");
            return;
        }
    }
}