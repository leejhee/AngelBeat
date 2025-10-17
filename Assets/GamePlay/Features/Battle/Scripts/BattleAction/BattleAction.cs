namespace GamePlay.Features.Battle.Scripts.BattleAction
{
    public abstract class BattleAction
    {
        public abstract void BuildActionPreview();
        public abstract void ExecuteAction(BattleActionContext context);
    }

    public class MoveBattleAction : BattleAction
    {
        public override void BuildActionPreview()
        {
            throw new System.NotImplementedException();
        }

        public override void ExecuteAction(BattleActionContext context)
        {
            throw new System.NotImplementedException();
        }
    }
    
    public class JumpBattleAction : BattleAction
    {
        public override void BuildActionPreview()
        {
            throw new System.NotImplementedException();
        }

        public override void ExecuteAction(BattleActionContext context)
        {
            throw new System.NotImplementedException();
        }
    }

    
    public class SkillBattleAction : BattleAction
    {
        public override void BuildActionPreview()
        {
            throw new System.NotImplementedException();
        }

        public override void ExecuteAction(BattleActionContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}