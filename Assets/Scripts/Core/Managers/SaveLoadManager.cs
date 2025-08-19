using Core.Foundation;
using Core.Foundation.Define;
using Core.Foundation.Utils;
using Core.GameSave;
using Core.GameSave.Contracts;
using Core.GameSave.IO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace Core.Managers
{
    public class SaveLoadManager : SingletonObject<SaveLoadManager>
    {
        #region Constructor
        private SaveLoadManager() { }
        #endregion
        
        private GlobalSaveData _globalSave;
        private GameSlotData _cachedSlotData;
        public event Action<GameSlotData> SlotLoaded;
        
        #region Properties
        public GlobalSaveData GlobalSave => _globalSave;
        public GameSlotData CurrentSlot => _cachedSlotData;
        public bool HasCurrentSlot => _cachedSlotData != null;
        public bool HasLastPlayed => _globalSave?.LastPlayedSlotData is { isEmpty: false };
        
        #endregion
        
        public override void Init()
        {
            base.Init();
            LoadGlobalData();
        }
        
       
        #region Synchronous Save & Load
        
        #region Global Data Management
        
        #region Events
        public void OnApplicationQuit()
        {
            if (HasCurrentSlot)
            {
                
                Debug.Log("강제 종료 관계로 저장.");
            }
        }

        public void OnApplicationPause(bool _pauseStatus)
        {
            if (!_pauseStatus) return;
            if (HasCurrentSlot)
            {
                SaveSlotByState(GameManager.Instance.GameState);
            }
        }
        #endregion
        
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
        
        // 새로 게임을 시작할 때는 세이브가 필요한가? - 편의성 상 있는게 나을거같긴 함.
        
        /// <summary>
        /// 새 슬롯을 생성
        /// </summary>
        /// <param name="slotName"> 새로 지정할 슬롯의 이름(저장 위치로 할거같긴 함) </param>
        /// <param name="slotIndex"> 새로 저장할 슬롯의 인덱스 </param>
        /// <returns> 잘 되었는가? </returns>
        public bool CreateNewSlot(string slotName, out int slotIndex)
        {
            slotIndex = -1;
            if (_globalSave.GameSlots.Count >= _globalSave.maxSlotCount)
            {
                Debug.LogWarning("방 없어 방 빼고 들어와~");
                return false;
            }
            
            var newSlot = new GameSlotData(slotName);
            
            // 새 슬롯 만들었으므로 전역 데이터 저장하기
            slotIndex = _globalSave.GetOrCreateSlot(slotName);
            _globalSave.LastPlayedSlotIndex = slotIndex;
            SaveGlobalData();

            _cachedSlotData = newSlot;
            
            // 슬롯 데이터는 별도로 저장한다.
            string slotFileName = SystemString.GetSlotName(slotIndex);
            Util.SaveJsonNewtonsoft(_cachedSlotData, slotFileName);
            
            Debug.Log($"[New Slot Created]: {slotName}, Index : {slotIndex}");
            return true;
        }
        
        /// <summary>
        /// 선택된 인덱스의 슬롯을 로드한다.
        /// </summary>
        /// <param name="slotIndex">선택한 슬롯의 인덱스(UI에서 받아올 거임)</param>
        /// <returns> 로드가 잘 되었는가? </returns>
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
                Debug.LogWarning($"[로드 실패] : 비어있는데요? {slotIndex}번 슬롯입니다.");
                return false;
            }
            
            string slotFileName = $"{SystemString.SlotPrefix}{slotIndex}";
            var gameSlot = Util.LoadSaveDataNewtonsoft<GameSlotData>(slotFileName);
            if (gameSlot == null)
            {
                // TODO : 메타데이터가 있는데 게임 슬롯이 없는 경우는 흔하지는 않다. 이러면 그냥 유효하지 않다 하고 튕겨버리자.
                Debug.LogError("[로드 실패] : 비유효 데이터가 감지되어 로드할 수 없습니다. 해당 슬롯을 삭제해주세요.");
            }
            
            // 할당부
            _cachedSlotData = gameSlot;
            _globalSave.LastPlayedSlotIndex = slotIndex;
            SaveGlobalData();
            
            return true;
        }
        
        /// <summary>
        /// 현재 게임에 로드되어있는 슬롯에 내용을 덮어씌워서 저장한다.
        /// </summary>
        public void SaveCurrentSlot(FeatureSnapshot snapshot)
        {
            if (!HasCurrentSlot)
            {
                Debug.LogWarning("[저장 실패] : 호출되면 안되는 로그. 지금 저장할 슬롯이 따로 없습니다?");
                return;
            }
            
            _cachedSlotData.WriteSnapshot(snapshot); // 쓰기
            
            string slotFileName = SystemString.GetSlotName(_globalSave.LastPlayedSlotIndex);
            Util.SaveJsonNewtonsoft(_cachedSlotData, slotFileName); // IO

            if (_globalSave.LastPlayedSlotIndex >= 0 &&
                _globalSave.LastPlayedSlotIndex < _globalSave.GameSlots.Count)
            {
                UpdateSlotMetadata(); // 연결용 메타데이터 업데이트 및 글로벌 데이터 저장
                SaveGlobalData();
            }
            Debug.Log($"Current Slot Saved : {_cachedSlotData.slotName}");
        }

        public void SaveSlotByState(SystemEnum.GameState state)
        {
            
        }
        
        private void UpdateSlotMetadata()
        {
            if (_globalSave.LastPlayedSlotIndex >= 0 && 
                _globalSave.LastPlayedSlotIndex < _globalSave.GameSlots.Count)
            {
                _globalSave.UpdateSlotMetadata(_globalSave.LastPlayedSlotIndex, _cachedSlotData);
            }
        }
        
        /// <summary>
        /// 슬롯 삭제 시 호출되는 메소드. 메타데이터 및 실제 파일 삭제.
        /// </summary>
        /// <param name="slotIndex"> 삭제할 슬롯 인덱스 </param>
        /// <returns> 잘 되었는가? </returns>
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
            
            // 글로벌 데이터에서의 슬롯 메타데이터 삭제
            bool success = _globalSave.DeleteSlot(slotIndex);
            
            // 현재 캐시된 슬롯이 삭제된 경우 정리
            if (_globalSave.LastPlayedSlotIndex == slotIndex)
            {
                _cachedSlotData = null;
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
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete slot file {fileName}: {ex.Message}");
            }
        }
        
        private string GetSaveDirectory()
        {
            return System.IO.Path.Combine(Application.persistentDataPath, "userdata");
        }
        
        public List<SlotMetaData> GetAllSlots()
        {
            return _globalSave.GameSlots;
        }
        
        #endregion
        
        #endregion
        
        #region Asynchronous Save & Load
        public async Task<bool> LoadSlotAsync(int slotIndex, CancellationToken ct)
        {
            string path = SystemString.GetSlotName(slotIndex) + ".json";
            GameSlotData loaded = await SlotIO.LoadAsync(path, ct);

            _cachedSlotData = loaded;
            _globalSave.LastPlayedSlotIndex = slotIndex;
            SaveGlobalData();

            // (UniTask가 있으면 await UniTask.SwitchToMainThread();)
            //foreach (var c in _contributors) c.ApplyFrom(_cachedSlotData);
            return true;
        }

        public async Task SaveCurrentSlotAsync(SystemEnum.GameState state, CancellationToken ct)
        {
            // 1) 각 시스템 상태를 DTO로 수집(메인 스레드)
            //foreach (var c in _contributors) c.CaptureTo(_cachedSlotData);
            _cachedSlotData.lastGameState = state;
            _cachedSlotData.lastSavedTime = DateTime.Now;

            // 2) 파일 쓰기(백그라운드)
            var path = SystemString.GetSlotName(_globalSave.LastPlayedSlotIndex) + ".json";
            await SlotIO.SaveAsync(path, _cachedSlotData, ct);

            // 3) 메타데이터 업데이트
            UpdateSlotMetadata();
            SaveGlobalData();
        }
        #endregion


        public void RegisterProvider(IFeatureSaveProvider featureProvider)
        {
            throw new NotImplementedException();
        }

        public void UnregisterProvider(IFeatureSaveProvider featureProvider)
        {
            throw new NotImplementedException();
        }
    }
}
