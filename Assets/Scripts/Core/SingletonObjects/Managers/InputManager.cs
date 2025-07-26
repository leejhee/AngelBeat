using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 업데이트문에 항상 키를 감지하게 하려면 너무 부하가 커지기 때문에,
/// 인풋을 체크해서 이벤트로 쫙 퍼트려주는 식으로(리스너 패턴) 매니저를 구현한다.
/// </summary>
public class InputManager : SingletonObject<InputManager>
{
    #region 생성자
    private InputManager() {}
    #endregion

    public override void Init()
    {
        base.Init();
    }

    // void 반환형의 Delegate다.
    public Action KeyAction = null;

    /// <summary>
    /// Monobehavior 받아서 사용하는 업데이트문과 다르다,
    /// 리스너 패턴으로 구현.
    /// </summary>
    public void OnUpdate()
    {
        // 키 입력이 아무것도 없었다면
        if (Input.anyKey == false) { return; }

        // 키 액션이 있었다면 
        if (KeyAction != null)
            KeyAction.Invoke();
    }
}
