using System;
using UnityEngine;

namespace GamePlay.Explore
{
    /// <summary>
    /// 탐사 관리
    /// </summary>
    public class ExploreManager : MonoBehaviour
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
    }
}