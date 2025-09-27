using Core.Scripts.Foundation;
using Core.Scripts.Foundation.Singleton;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Scripts.Managers
{
    /// <summary>
    /// Unity Input System 사용
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        #region 생성자
        private InputManager() {}
        #endregion

        [SerializeField] private PlayerInput input;

        
    }
}
