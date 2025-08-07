using System.Collections.Generic;
using System;

namespace AngelBeat
{
    public class TurnComparer : IComparer<Turn>
    {
        private Func<Turn, Turn, int>[] comparisonFunctions;
        public TurnComparer(params Func<Turn, Turn, int>[] comparisonFunctions)
        {
            this.comparisonFunctions = comparisonFunctions;
        }
        public int Compare(Turn x, Turn y)
        {
            foreach (var func in comparisonFunctions)
            {
                int result = func(x, y);
                if (result != 0)
                    return result;
            }
            return 0;
        }
    }

//[TODO] : 속도 계산식 나오면 작업할 것.
    public static class TurnComparisonMethods
    {
        public static int VanillaComparer(Turn x, Turn y)
        {
            int xSpeed = (int)x.TurnOwner.CharStat.GetStat(SystemEnum.eStats.NSPEED);
            int ySpeed = (int)y.TurnOwner.CharStat.GetStat(SystemEnum.eStats.NSPEED);
            return ySpeed - xSpeed;
        }
    }
}
