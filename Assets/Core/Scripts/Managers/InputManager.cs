using Core.Scripts.Foundation;
using Core.Scripts.Foundation.Singleton;
using System;
using UnityEngine;

namespace Core.Scripts.Managers
{
    /// <summary>
    /// Unity Input System 사용
    /// </summary>
    public class InputManager : SingletonObject<InputManager>
    {
        #region 생성자
        private InputManager() {}
        #endregion
        
        #region Input Action Enum

        public enum EInputAction
        {
            None = 0,
            
        }
        #endregion
        
        //private Dictionary<
        
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
            //if (Input.anyKey == false) { return; }

            // 키 액션이 있었다면 
            if (KeyAction != null)
                KeyAction.Invoke();
        }
    }
}
