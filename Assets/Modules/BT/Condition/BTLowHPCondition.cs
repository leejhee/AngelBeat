using Modules.BT.Nodes;
using Unity.PlasticSCM.Editor.WebApi;

namespace Modules.BT.Condition
{
    public class BTLowHPCondition : IBTCondition
    {
        public bool Evaluate(BTContext context)
        {
            return context.HP < context.MaxHP * 0.3f;
        }
    }
}