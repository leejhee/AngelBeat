using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    [NovelParameterAlias("String")]
    [Order(2)]
    public class StringParameter : NovelParameter
    {
        public string value;

    }
}
