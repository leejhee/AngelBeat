using Character.Unit;
using Core.Foundation.Define;

namespace AngelBeat
{
    public struct ExecutionParameter
    {
        public SystemEnum.eExecutionType eExecutionType;
        public CharBase TargetChar;
        public CharBase CastChar;
        public long ExecutionIndex;
    }
}
