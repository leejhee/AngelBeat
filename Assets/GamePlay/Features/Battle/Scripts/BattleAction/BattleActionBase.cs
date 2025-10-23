using Cysharp.Threading.Tasks;
using System.Threading;

namespace GamePlay.Features.Battle.Scripts.BattleAction
{
    public abstract class BattleActionBase
    {
        protected readonly BattleActionContext Context;
        protected BattleActionBase(BattleActionContext ctx) => Context = ctx;

        public abstract UniTask<BattleActionPreviewData> BuildActionPreview(CancellationToken ct);
        public abstract UniTask<BattleActionResult> ExecuteAction(CancellationToken ct);
    }
}