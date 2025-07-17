namespace Modules.BT.Nodes
{
    public abstract class BTNode
    {
        public enum State
        {
            Success,
            Failure,
            Running
        }
        
        public abstract State Evaluate();
    }
}

