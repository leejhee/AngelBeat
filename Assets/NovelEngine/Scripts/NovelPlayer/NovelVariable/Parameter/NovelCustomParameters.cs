using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static novel.NovelCustomParameters.CharacterStateParameter;

namespace novel
{
    public class NovelCustomParameters
    {
        [System.Serializable]
        [NovelParameterAlias("CharacterState")]
        public class CharacterStateParameter : CustomParameter
        {
            public CharacterState state;


            [System.Serializable]
            public class CharacterState
            {
                public int state1;
                public int state2;
                public int state3;
            }
        }
    }
}
