using AngelBeat;
using Character;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Managers;
using GamePlay.Battle;
using GamePlay.Character;
using GamePlay.Entities.Scripts.Character;
using GamePlay.Features.Battle.Scripts;
using GamePlay.Features.Scripts.Battle;
using System.Collections.Generic;
using UnityEngine;

namespace Scene
{
    /// <summary>
    /// BattleScene에서만 테스트하는 용도의 씬 초기화용 클래스
    /// </summary>
    public class MonoBattleTestScene : MonoBehaviour
    {
        [SerializeField] 
        private List<GameObject> battleUI;
        
        private DebugMockSource _src;
        void Awake()
        {
            GameManager instance = GameManager.Instance;
        }
        
        async void Start()
        {
            await DataManager.Instance.WhenReady();
            
            var testXiaoModel = new CharacterModel(88888888);
            Party playerParty = new (new List<CharacterModel> { testXiaoModel });
            _src = new DebugMockSource(SystemEnum.Dungeon.MOUNTAIN_BACK, playerParty, "TestMap");
            
            BattleController.Instance.SetStageSource(_src);
            
            //foreach(var go in battleUI)
            //    UIManager.Instance.ShowUI(go);
        }
        
        
    }
}