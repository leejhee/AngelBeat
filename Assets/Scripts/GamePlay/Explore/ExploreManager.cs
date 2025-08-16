using Core.GameSave;
using Core.GameSave.Contracts;
using Core.Managers;
using System;
using UnityEngine;

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

        private ExploreSaveData _exploreData;

        public void SaveCurrentExploration()
        {
            
        }
        
        private void OnEnable()
        {
            SaveLoadManager.Instance.RegisterProvider(this);
            SaveLoadManager.Instance.SlotLoaded += OnSlotLoaded;
        }

        private void OnDisable()
        {
            SaveLoadManager.Instance.SlotLoaded -= OnSlotLoaded;
            SaveLoadManager.Instance.UnregisterProvider(this);
        }

        private void OnSlotLoaded(GameSlotData data)
        {
            if (data == null) return;
            if (data.TryGet<ExploreSnapshot>(FeatureName, out var snap))
            {
               
            }
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