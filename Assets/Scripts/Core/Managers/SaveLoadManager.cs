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
        #if UNITY_INCLUDE_TESTS
        public static bool suppressSlotLoadEvent = false;
        public void FlushCurrentSlotSync()
        {
            if (!HasCurrentSlot) return;
            string path = SystemString.GetSlotName(_globalSave.LastPlayedSlotIndex) + SystemString.JsonExtension;
            SlotIO.SaveAsync(path, _cachedSlotData, CancellationToken.None).GetAwaiter().GetResult();
        }
        #endif
        
        
        #region Constructor
        private SaveLoadManager() { }
        #endregion
        
        private GlobalSaveData _globalSave;
        private GameSlotData _cachedSlotData;
        public event Action<GameSlotData> SlotLoaded;

        private readonly Dictionary<string, IFeatureSaveProvider> _providers = new();
        private readonly Dictionary<SystemEnum.GameState, List<string>> _stateToFeatures = new()
        {
            [SystemEnum.GameState.Explore] = new() {"Explore"},
            [SystemEnum.GameState.Battle] = new() {"Battle"},
            [SystemEnum.GameState.Village] = new() {"Village"},
        };
        private DateTime _lastAutoSave = DateTime.MinValue;
        private const double AutoSaveIntervalSec = 5d;
        
        #region Properties
        public GlobalSaveData GlobalSave => _globalSave;
        public GameSlotData CurrentSlot => _cachedSlotData;
        public bool HasCurrentSlot => _cachedSlotData != null;
        public bool HasLastPlayed => _globalSave?.LastPlayedSlotData is { isEmpty: false };
        
        #endregion
        
        public override void Init()
        {
            base.Init();
            string root = System.IO.Path.Combine(Application.persistentDataPath, "userdata");
            SlotIO.InitUserRoot(root);
            LoadGlobalData();
            
        }
        
        #region Synchronous Save & Load
        
        #region Global Data Management
        
        #region Events
        public void OnApplicationQuit()
        {
            if (HasCurrentSlot)
            {
                try
                {
                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(150); //밀리세컨
                    string fileName = SystemString.GetSlotName(_globalSave.LastPlayedSlotIndex) + SystemString.JsonExtension;
                    SlotIO.SaveAsync(fileName, _cachedSlotData, cts.Token).GetAwaiter().GetResult();
                }
                catch{/* 일이 잘못돼도 소란피우지 맙시다*/}
                Debug.Log("강제 종료 관계로 저장.");
            }
        }
        
        public void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus) return;
            if (HasCurrentSlot)
            {
                // 앱 내릴 시 자동저장인데... 이거 맞나? 아닌거같으면 바로 없애자.
                if ((DateTime.Now - _lastAutoSave).TotalSeconds > AutoSaveIntervalSec)
                {
                    _lastAutoSave = DateTime.Now;
                    SaveSlotByState(GameManager.Instance.GameState);
                }
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
            //string slotFileName = SystemString.GetSlotName(slotIndex);
            //Util.SaveJsonNewtonsoft(_cachedSlotData, slotFileName);

            try
            {
                string slotFileName = SystemString.GetSlotName(slotIndex) + SystemString.JsonExtension;
                SlotIO.SaveAsync(slotFileName, _cachedSlotData, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Debug.LogError($"[New Slot Save Error] {slotName} : {e}");
            }
            
            
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
            
            string slotFileName = $"{SystemString.SlotPrefix}{slotIndex}.json";
            var gameSlot = SlotIO.LoadAsync(slotFileName, CancellationToken.None).GetAwaiter().GetResult();
            if (gameSlot == null)
            {
                // TODO : 메타데이터가 있는데 게임 슬롯이 없는 경우는 흔하지는 않다. 이러면 그냥 유효하지 않다 하고 튕겨버리자.
                Debug.LogError("[로드 실패] : 슬롯 파일이 없어 로드할 수 없습니다. 해당 슬롯을 삭제해주세요.");
            }
            
            // 할당부
            _cachedSlotData = gameSlot;
            _globalSave.LastPlayedSlotIndex = slotIndex;
            SaveGlobalData();
            
            #if UNITY_INCLUDE_TESTS
            if(!suppressSlotLoadEvent)
                SlotLoaded?.Invoke(_cachedSlotData); //할당할거 다 하고 호출!
            #else
            SlotLoaded?.Invoke(_cachedSlotData);
            #endif
            
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
            
            //string slotFileName = SystemString.GetSlotName(_globalSave.LastPlayedSlotIndex);
            //Util.SaveJsonNewtonsoft(_cachedSlotData, slotFileName); // IO
            
            string slotFileName = SystemString.GetSlotName(_globalSave.LastPlayedSlotIndex) + SystemString.JsonExtension;
            //??
            AsyncJobQueue.EnqueueKeyed("save-slot",
                ct => SlotIO.SaveAsync(slotFileName, _cachedSlotData, ct));
            
            if (_globalSave.LastPlayedSlotIndex >= 0 &&
                _globalSave.LastPlayedSlotIndex < _globalSave.GameSlots.Count)
            {
                UpdateSlotMetadata(); // 연결용 메타데이터 업데이트 및 글로벌 데이터 저장
                SaveGlobalData();
            }
            Debug.Log($"Current Slot Saved : {_cachedSlotData.slotName}");
        }
        
        /// <summary>
        /// 외부 요인으로 어쩔 수 없이 저장을 해야할 때 호출하는 저장함수입니다.
        /// </summary>
        /// <param name="state">인게임 상태를 캡처해서 저장하므로, GameState에 따라 저장됩니다.</param>
        public void SaveSlotByState(SystemEnum.GameState state)
        {
            if (!HasCurrentSlot) return;

            if (_stateToFeatures.TryGetValue(state, out List<string> features) && features != null)
            {
                foreach(string key in features)
                    if(_providers.TryGetValue(key, out IFeatureSaveProvider provider))
                        _cachedSlotData.WriteSnapshot(provider.Capture());
            }
            
            _cachedSlotData.lastGameState = state;
            _cachedSlotData.lastSavedTime = DateTime.Now;

            string slotFileName = SystemString.GetSlotName(_globalSave.LastPlayedSlotIndex) + SystemString.JsonExtension;
            AsyncJobQueue.EnqueueKeyed("save-slot",
                ct => SlotIO.SaveAsync(slotFileName, _cachedSlotData, ct));
            
            UpdateSlotMetadata();
            SaveGlobalData();
        }

        public void SaveSlotByCurrentState() => SaveSlotByState(GameManager.Instance.GameState);
        
        
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
            string path = SystemString.GetSlotName(slotIndex) + SystemString.JsonExtension;
            GameSlotData loaded = await SlotIO.LoadAsync(path, ct);

            _cachedSlotData = loaded;
            _globalSave.LastPlayedSlotIndex = slotIndex;
            SaveGlobalData();
            
            SlotLoaded?.Invoke(_cachedSlotData);
            return true;
        }

        public async Task SaveCurrentSlotAsync(SystemEnum.GameState state)
        {
            if (!HasCurrentSlot) return;
            if (_stateToFeatures.TryGetValue(state, out List<string> features) && features != null)
            {
                foreach (string key in features) 
                    if(_providers.TryGetValue(key, out IFeatureSaveProvider provider))
                      _cachedSlotData.WriteSnapshot(provider.Capture());
            }
            
            _cachedSlotData.lastGameState = state;
            _cachedSlotData.lastSavedTime = DateTime.Now;
            
            string path = SystemString.GetSlotName(_globalSave.LastPlayedSlotIndex) + SystemString.JsonExtension;
            await AsyncJobQueue.EnqueueKeyedWithCompletion("save-slot", token => SlotIO.SaveAsync(path, _cachedSlotData, token));
            
            UpdateSlotMetadata();
            SaveGlobalData();
        }
        #endregion


        public void RegisterProvider(IFeatureSaveProvider featureProvider)
        {
            if (featureProvider == null) return;
            _providers[featureProvider.FeatureName] = featureProvider;
        }

        public void UnregisterProvider(IFeatureSaveProvider featureProvider)
        {
            if (featureProvider == null) return;
            _providers.Remove(featureProvider.FeatureName);
        }
    }
}
