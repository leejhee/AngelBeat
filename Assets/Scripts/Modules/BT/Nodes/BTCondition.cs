using System;

namespace Modules.BT.Nodes
{
    public class BTCondition : BTNode
    {
        private readonly Func<bool> _condition;

        public BTCondition(Func<bool> condition)
        {
            _condition = condition;
        }

        public override State Evaluate()
        {
            return _condition.Invoke() ? State.Success : State.Failure;
        }
    }
}