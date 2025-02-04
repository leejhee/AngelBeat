using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecutionBase : MonoBehaviour
{
    protected CharBase _TargetChar = null; 
    protected CharBase _CastChar = null; // 기능 캐스팅 캐릭터
    protected ExecutionData _ExecutionData = null; // 기능 데이터

    public ExecutionBase(ExecutionParameter buffParam)
    {
        _TargetChar = buffParam.TargetChar;
        _CastChar = buffParam.CastChar;
        _ExecutionData = DataManager.Instance.GetData<ExecutionData>(buffParam.ExecutionIndex);

        if (_ExecutionData == null)
        {
            Debug.LogError($"Execution : {buffParam.ExecutionIndex} 데이터 획득 실패");
        }
    }

    // 작동 함수 필요
}
