using System;

namespace Modules.BT.Nodes
{
    public class BTAction : BTNode
    {
        private readonly Func<State> _action;

        public BTAction(Func<State> action)
        {
            _action = action;
        }

        public override State Evaluate()
        {
            return _action.Invoke();
        }
    }
}