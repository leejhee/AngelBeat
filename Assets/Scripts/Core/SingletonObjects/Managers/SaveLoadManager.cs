using Core.GameSave;
using System.Collections.Generic;
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

        private Dictionary<SystemEnum.GameState, ISavableEntity> _stateCache = new();
        
        #region Properties
        public GlobalSaveData GlobalSave => _globalSave;
        public GameSlotData CurrentSlot => _cachedSlotData;
        public bool HasCurrentSlot => _cachedSlotData != null;
        public bool HasLastPlayed => _globalSave?.LastPlayedSlotData is { isEmpty: false };
        #endregion
        
        public override void Init()
        {
            base.Init();
            //_globalSave = Util.LoadSaveData<GlobalSaveData>("save") ?? new GlobalSaveData();
            LoadGlobalData();
        }
        
        // 이벤트 함수에 꼭 등록하세요 반드시반드시반드시반드시반드시반드시반드시반드시반드시
        public void OnApplicationQuit()
        {
            if (HasCurrentSlot)
            {
                //그냥 현재 슬롯을 현 게임 상태에 따라 저장.
                SaveCurrentSlot();
                Debug.Log("강제 종료 관계로 저장.");
            }
        }
        
        #region Global Data Management
        public void LoadGlobalData()
        {
            _globalSave = Util.LoadSaveDataNewtonsoft<GlobalSaveData>(SystemString.GlobalSaveDataPath);
            if (_globalSave == null)
            {
                _globalSave = new GlobalSaveData();
                SaveGlobalData();
                Debug.Log("글로벌 데이터가 없는 관계로 하나 새로 만들겠습니다잉");
            }
            else
            {
                Debug.Log($"Successfully loaded global save data : {_globalSave.UID}");
            }
        }

        public void SaveGlobalData()
        {
            if (_globalSave == null)
            {
                Debug.LogError("자네 지금 null을 저장하려고 했는가?");
                return;
            }
            
            Util.SaveJsonNewtonsoft(_globalSave, SystemString.GlobalSaveDataPath);
            Debug.Log("Global Data Saved!");
        }
        #endregion
        
        #region Slot Data Management

        public bool CreateNewSlot(string slotName, out int slotIndex)
        {
            slotIndex = -1;
            if (_globalSave.GameSlots.Count >= _globalSave.maxSlotCount)
            {
                Debug.LogWarning("이 부분 일단 막아둘 것");
                return false;
            }
            
            var newSlot = new GameSlotData(slotName);
            
            slotIndex = _globalSave.GetOrCreateSlot(slotName);
            _globalSave.LastPlayedSlotIndex = slotIndex;
            SaveGlobalData();

            _cachedSlotData = newSlot;
            ClearCache();

            string slotFileName = $"{SystemString.SlotPrefix}{slotIndex}";
            Util.SaveJsonNewtonsoft(_cachedSlotData, slotFileName);
            
            Debug.Log($"[New Slot Created]: {slotName}, Index : {slotIndex}");
            return true;
        }

        public bool LoadSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _globalSave.maxSlotCount)
            {
                Debug.LogError($"[로드 실패] : 유효하지 않은 슬롯 넘버 {slotIndex}");
                return false;
            }
            var slotMetaData = _globalSave.GameSlots[slotIndex];
            if (slotMetaData.isEmpty)
            {
                Debug.LogWarning($"비어있는데요? : {slotIndex}");
                return false;
            }
            
            ClearCache();
            string slotFileName = $"{SystemString.SlotPrefix}{slotIndex}";
            var gameSlot = Util.LoadSaveDataNewtonsoft<GameSlotData>(slotFileName);
            if (gameSlot == null)
            {
                // TODO : 메타데이터가 있는데 게임 슬롯이 없는 경우는 흔하지는 않다. 이 경우 경고를 때려야 한다.
                gameSlot = CreateGameSlotFromMetadata(slotMetaData);
                Debug.LogWarning("습... 일부러 삭제하셨어요? 이번만 조금 복구해드립니다?");
            }
            
            _cachedSlotData = gameSlot;
            _globalSave.LastPlayedSlotIndex = slotIndex;
            SaveGlobalData();
            
            return true;
        }
        
        //필요한가...? 일단 귀찮은 일은 만들지 맙시다.
        private GameSlotData CreateGameSlotFromMetadata(SlotMetaData meta)
        {
            var gameSlot = new GameSlotData(meta.slotName);
            gameSlot.lastSavedTime = meta.lastSavedTime;
            gameSlot.lastGameState = meta.lastGameState;
            gameSlot.playTimeTicks = meta.playTimeTicks;
            return gameSlot;
        }
        
        public void SaveCurrentSlot()
        {
            if (!HasCurrentSlot)
            {
                Debug.LogWarning("지금 저장할 슬롯이 따로 없습니다?");
                return;
            }
            
            SaveAllDirtyStates();
            
            string slotFileName = $"{SystemString.SlotPrefix}{_globalSave.LastPlayedSlotIndex}";
            Util.SaveJsonNewtonsoft(_globalSave, slotFileName);

            if (_globalSave.LastPlayedSlotIndex >= 0 &&
                _globalSave.LastPlayedSlotIndex < _globalSave.GameSlots.Count)
            {
                UpdateSlotMetadata();
                SaveGlobalData();
            }
            Debug.Log($"Current Slot Saved : {_cachedSlotData.slotName}");
        }

        private void UpdateSlotMetadata()
        {
            if (_globalSave.LastPlayedSlotIndex >= 0 && 
                _globalSave.LastPlayedSlotIndex < _globalSave.GameSlots.Count)
            {
                _globalSave.UpdateSlotMetadata(_globalSave.LastPlayedSlotIndex, _cachedSlotData);
            }
        }
        
        public bool DeleteSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _globalSave.GameSlots.Count)
            {
                Debug.LogError($"Invalid slot index: {slotIndex}");
                return false;
            }

            string deletedSlotName = _globalSave.GameSlots[slotIndex].slotName;
            
            // 개별 슬롯 파일 삭제
            string slotFileName = $"{SystemString.SlotPrefix}{slotIndex}";
            DeleteSlotFile(slotFileName);
            
            // 글로벌 데이터에서 슬롯을 빈 상태로 만들기
            bool success = _globalSave.DeleteSlot(slotIndex);
            
            // 현재 캐시된 슬롯이 삭제된 경우 정리
            if (_globalSave.LastPlayedSlotIndex == slotIndex)
            {
                _cachedSlotData = null;
                ClearCache();
            }
            
            SaveGlobalData();
            Debug.Log($"Slot deleted: {deletedSlotName}");
            return success;
        }
        
        private void DeleteSlotFile(string fileName)
        {
            try
            {
                string filePath = System.IO.Path.Combine(GetSaveDirectory(), $"{fileName}.json");
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    Debug.Log($"Deleted slot file: {fileName}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to delete slot file {fileName}: {ex.Message}");
            }
        }
        
        private string GetSaveDirectory()
        {
            return System.IO.Path.Combine(Application.persistentDataPath, "userdata");
        }
        
        //public List<GameSlotData> GetAllSlots()
        //{
        //    return _globalSave.GameSlots;
        //}

        public void SaveAllDirtyStates()
        {
            if (!HasCurrentSlot)
            {
                Debug.LogWarning("애초에 Current로 지정된 슬롯이 없습니다.");
                return;
            }

            int savedCount = 0;
            foreach (var kvp in _stateCache)
            {
                var state = kvp.Key;
                var savableEntity = kvp.Value;

                if (savableEntity.IsDirty())
                {
                    savableEntity.Save();
                    
                }
            }
        }

        private void SaveEntityToSlot(SystemEnum.GameState state, ISavableEntity entity)
        {
            switch (entity)
            {
                
            }
        }
        
        #endregion

        public void ClearCache()
        {
            _stateCache.Clear();
        }
    }
}
