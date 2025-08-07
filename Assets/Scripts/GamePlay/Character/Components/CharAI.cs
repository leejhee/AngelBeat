using Modules.BT;
using Modules.BT.Nodes;
using Modules.BT.Runtime;
using System;

namespace AngelBeat
{
    public class CharAI
    {
        private BTNode _root;
        private readonly BTContext _context;
        
        public CharAI(BTContext context)
        {
            _context = context;
            _root = BuildMockTree();
        }

        public void Initialize(BTGraphData asset)
        {
            _root = BTBuilder.BuildTree(asset, _context);
        }

        private BTNode BuildMockTree()
        {
            throw new NotImplementedException("좀만 기다려봐~");
        }

        public void Execute()
        {
            _root.Evaluate(_context);
        }
    }
}
