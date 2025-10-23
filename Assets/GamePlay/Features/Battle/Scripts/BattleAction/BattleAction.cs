using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts.BattleMap;
using System.Threading;

namespace GamePlay.Features.Battle.Scripts.BattleAction
{
    public abstract class BattleAction
    {
        protected readonly BattleActionContext Context;
        protected BattleAction(BattleActionContext ctx) => Context = ctx;

        public abstract UniTask<BattleActionPreviewData> BuildActionPreview(CancellationToken ct);
        public abstract UniTask<BattleActionResult> ExecuteAction(CancellationToken ct);
    }
}