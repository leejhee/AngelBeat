namespace Modules.BT.Nodes
{
    /// <summary>
    /// '배치'를 실행하는 시퀀스 노드. 자식들 중 하나라도 실패면 Failure
    /// </summary>
    public class BTSequence : BTCompositeNode
    {
        public override State Evaluate()
        {
            foreach (BTNode child in Children)
            {
                var result = child.Evaluate();
                if(result != State.Failure)
                    return result;
            }
            return State.Success;
        }
    }
}