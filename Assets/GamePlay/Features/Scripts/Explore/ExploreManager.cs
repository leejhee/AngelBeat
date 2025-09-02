using Core.GameSave;
using Core.GameSave.Contracts;
using Core.Scripts.Foundation.Define;
using Core.Scripts.GameSave;
using Core.Scripts.Managers;
using GamePlay.Character;
using GamePlay.Entities.Scripts.Character;
using GamePlay.Explore.Map.Logic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamePlay.Explore
{
    /// <summary>
    /// 탐사 관리
    /// </summary>
    public class ExploreManager : MonoBehaviour, IFeatureSaveProvider
    {
        #region Singleton
        private static ExploreManager instance;
        public static ExploreManager Instance
        {
            get
            {
                instance = FindObjectOfType<ExploreManager>();
                if(!instance)
                    instance = new GameObject("ExploreManager").AddComponent<ExploreManager>();

                return instance;
            }
        }
        #endregion
        
        [FormerlySerializedAs("eDungeon")] public SystemEnum.Dungeon dungeon;
        public ExploreMap mapData;
        public Party playerParty;
        
        private void Awake()
        {
            if (!instance)
            {
                instance = this;
            }
            else if(instance != this)
            {
                Destroy(gameObject);
            }
        }

        
        private void OnEnable()
        {
            SaveLoadManager.Instance.RegisterProvider(this);
            if (SaveLoadManager.Instance.CurrentSlot.TryGet("Explore", out ExploreSnapshot explore))
            {
                RebuildExploreState(explore);
            }
            else
            {
                
            }
            //SaveLoadManager.Instance.SlotLoaded += OnSlotLoaded;
        }

        private void OnDisable()
        {
            //SaveLoadManager.Instance.SlotLoaded -= OnSlotLoaded;
            SaveLoadManager.Instance.UnregisterProvider(this);
        }

        private void RebuildExploreState(ExploreSnapshot snapshot)
        {
            
        }
        
        #region IFeatureSaveProvider Members
        public string FeatureName => "Explore";

        public FeatureSnapshot Capture()
        {
            return new ExploreSnapshot();
        }

        #endregion
    }
}