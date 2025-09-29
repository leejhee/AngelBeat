using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    [NovelParameterAlias("Boolean")]
    [Order(3)]
    public class BooleanParameter : NovelParameter
    {

        public bool value;
    }
}