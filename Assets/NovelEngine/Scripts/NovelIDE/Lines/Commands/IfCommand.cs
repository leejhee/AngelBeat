using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    public enum IfType
    {
        If,
        Else,
        None
    }
    public enum CompOP
    {
        GreaterThan,       // >
        LessThan,          // <
        GreaterThanOrEqual, // >=
        LessThanOrEqual,    // <=
        Equal,             // ==
        NotEqual,           // !=,
        None
    }
    [System.Serializable]
    public class IfCommand : CommandLine
    {
        public IfType typeOfIf;
        public string entity;
        public string var;
        public CompOP op;
        public float? value;
        

        public IfCommand(int index, IfType typeOfIf = IfType.None, string var = null,
                        CompOP op = CompOP.None, float? value = null) : base(index, DialogoueType.CommandLine)
        {
            this.typeOfIf = typeOfIf;
            this.value = value;
            this.var = var;
            this.op = op;
            
        }

        // 변수끼리 비교하는 기능 추가할수도 있음
        public override async UniTask Execute()
        {
            // 데이터에서 변수값 받아오기
            float realVar = 0;
            float realValue = value ?? 0;
            bool returnBool = NovelUtils.ConditinalStateMent(realVar, op, realValue);
        }

        public override bool? IsWait()
        {
            return null;
        }
    }

}
