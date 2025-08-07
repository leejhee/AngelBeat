using GamePlay.Explore.Map.Data;
using UnityEngine;

namespace GamePlay.Explore.Map.Logic
{
    /// <summary>
    /// 탐사 맵 생성기
    /// </summary>
    public static class ExploreMapGenerator
    {
        #region Explore Map DB
        private static ExploreMapConfigDB configDB;
        
        private static ExploreMapConfigDB GetConfigDB()
        {
            if (configDB == null)
            {
                configDB = Resources.Load<ExploreMapConfigDB>(SystemString.MapConfigDBPath);
            }
            return configDB;
        }
        #endregion

        /// <summary> 탐사 던전 맵을 생성하는 메서드 </summary>
        /// <param name="dungeon"> 생성할 던전 타입 </param>
        /// <param name="floor"> 던전의 층 </param>
        /// <param name="seed"> 맵 생성 관련 랜덤 시드 </param>
        /// <returns>생성된 던전을 반환한다.</returns>
        public static ExploreMap GenerateMap(SystemEnum.eDungeon dungeon, int floor, int seed)
        {
            #region Managing Config
            ExploreMapConfigDB db = GetConfigDB();
            ExploreMapConfig config = db.GetConfig(dungeon, floor);
            if (!config)
            {
                Debug.LogError("[Error] - Map Config Load Error");
                return null;
            }
            #endregion
            
            #region Managing Seed
            System.Random rnd = new(seed);
            
            #endregion
            
            var map = new ExploreMap();
            return map;
        }
    }
}