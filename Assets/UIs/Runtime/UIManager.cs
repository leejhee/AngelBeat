using Core.Scripts.Foundation.Singleton;
using UnityEngine;

namespace UIs.Runtime
{
    public enum UIScope { Global, Scene, World }
    public class UIManager : SingletonObject<UIManager> 
    {
        #region Constructor
        private UIManager(){ }
        #endregion

        
    }
}