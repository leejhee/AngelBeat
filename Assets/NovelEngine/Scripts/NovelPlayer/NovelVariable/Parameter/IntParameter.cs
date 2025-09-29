using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{

    [System.Serializable]
    [NovelParameterAlias("Int")]
    [Order(0)]
    public class IntParameter : NovelParameter
    {
        public int value;

    }
}
