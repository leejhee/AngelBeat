using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    public enum IfType
    {
        If,
        Else
    }
    public enum CompOP
    {
        GreaterThan,       // >
        LessThan,          // <
        GreaterThanOrEqual, // >=
        LessThanOrEqual,    // <=
        Equal,             // ==
        NotEqual           // !=
    }
    [System.Serializable]
    public class IfCommand : CommandLine
    {
        public IfType typeOfIf;
        public string entity;
        public string var;
        public CompOP op;
        public float value;
        

        public IfCommand(int index, IfType typeOfIf , string var, CompOP op, float value, int depth = 0) : base(index, DialogoueType.CommandLine, depth)
        {
            this.typeOfIf = typeOfIf;
            //this.entity = entity;
            this.value = value;
            this.var = var;
            this.op = op;
            
        }

        // 변수끼리 비교하는 기능 추가할수도 있음
        public override void Execute()
        {
            // 데이터에서 변수값 받아오기
            float realVar = 0;

            switch (op)
            {
                case CompOP.GreaterThan:
                    if (realVar > value)
                    {

                    }

                    break;
                case CompOP.LessThan:
                    break;
                case CompOP.GreaterThanOrEqual:
                    break;
                case CompOP.LessThanOrEqual:
                    break;
                case CompOP.Equal:
                    break;
                case CompOP.NotEqual:
                    break;
                default:
                    Debug.LogError("Error : 정의되지 않은 연산자");
                    break;
            }
        }

        public override bool? IsWait()
        {
            return null;
        }
    }

}
