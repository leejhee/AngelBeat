using System;
using UnityEngine;

namespace Core.GameSave
{
    [Serializable, Obsolete]
    public abstract class SavableEntity
    {
        #region Event Methods

        public virtual void OnSave()
        {
            Debug.Log($"[Save Complete] : {GetType().ToString().Replace("Core.GameSave", "")}");
        }

        public virtual void OnLoad()
        {
            Debug.Log($"[Load Complete] : {GetType().ToString().Replace("Core.GameSave", "")}");
        }
        #endregion
        
        public bool IsDirty { get; protected set; }

        public void ClearDirty()
        {
            IsDirty = false;
        }
    }
}