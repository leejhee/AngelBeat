using Core.GameSave;
using UnityEngine;

namespace Core.SingletonObjects.Managers
{
    public class SaveLoadManager : SingletonObject<SaveLoadManager>
    {
        #region Constructor
        private SaveLoadManager() { }
        #endregion

        private GlobalSaveData _globalSave;
        private GameSlotData _cachedSlotData;
        
        public override void Init()
        {
            base.Init();
            _globalSave = Util.LoadSaveData<GlobalSaveData>("save") ?? new GlobalSaveData();
            _cachedSlotData = _globalSave.LastPlayedSlotData;
        }
        
        //TODO : 세이브 데이터 볼륨이 늘어날 것이므로 대입때리지 말고 영역별로 편집할 수 있도록 한다.
        public void SaveGameData(GameSlotData tempSlotData)
        {
            _cachedSlotData = tempSlotData;
        }

        public GameSlotData LoadGameData()
        {
            if (_cachedSlotData == null)
            {
                Debug.LogError("No Slot Dat Yet. Press \"NEW GAME\"");
                return null;
            }
            return _cachedSlotData;
        }

        public GameSlotData LoadGameSlotData(int idx)
        {
            return _globalSave.GameSlots[idx];
        }
    }
}
