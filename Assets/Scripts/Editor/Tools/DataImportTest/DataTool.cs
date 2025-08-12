using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace AngelBeat
{
    public class DataTool
    {
        [MenuItem("Data/데이터검증")]
        public static void DataVerification()
        {
            global::Core.Managers.DataManager.Instance.ClearCache();
            global::Core.Managers.DataManager.Instance.DataLoad();

            Debug.Log("데이터 검증 끝.");
        }

    }
#endif
}
