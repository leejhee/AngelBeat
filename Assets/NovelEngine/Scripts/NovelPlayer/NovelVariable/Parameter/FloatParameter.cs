using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    [NovelParameterAlias("Float")]
    [Order(1)]
    public class FloatParameter : NovelParameter
    {
        public float value;
    }
}
