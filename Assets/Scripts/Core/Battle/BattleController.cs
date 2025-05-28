using AngelBeat.Core.Character;
using AngelBeat.Core.Map;
using AngelBeat.Core.SingletonObjects;
using AngelBeat.Core.SingletonObjects.Managers;
using AngelBeat.Scene;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using EventBus = AngelBeat.Core.SingletonObjects.Managers.EventBus;

namespace AngelBeat.Core.Battle
{
    // 잠시 싱글턴 사용한다.
    public class BattleController : MonoBehaviour
    {
        #region singleton
        public static BattleController Instance { get; private set; }
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
        #endregion
        [SerializeField] private GameObject gameOverPrefab;
        [SerializeField] private GameObject gameWinPrefab;
        [SerializeField] private GameObject previewPrefab;
        private SkillPreview _preview;
        
        private List<CharBase> _playerParty;
        private List<CharBase> _enemyParty;
        private TurnController _turnManager;
        private CharBase FocusChar => _turnManager.TurnOwner;
        
        private void Start()
        {
            Debug.Log("Starting Battle...");
            BattleInitialize();
        }
        
        // TODO : Initializer 객체 따로 사용해서 초기화할 것
        private void BattleInitialize()
        {
            Debug.Log("Starting Battle Initialization...");
            SystemEnum.eDungeon dungeon = BattlePayload.Instance.DungeonName;
            StageField battleField = SetMapEnvironment(dungeon);
            if (!battleField)
            {
                Debug.LogError("Map Load Error");
                return;
            }
            
            Party party = BattlePayload.Instance.PlayerParty;
            List<CharBase> battleMembers = battleField.SpawnAllUnits(party);
            _turnManager = new TurnController(battleMembers); 
            _turnManager.ChangeTurn();
            
            BindBattleEvent();
            
            
            Debug.Log("Battle Initialization Complete");
            BattlePayload.Instance.Clear();
        }

        private StageField SetMapEnvironment(SystemEnum.eDungeon dungeon)
        {
            BattleFieldGroup battleFieldGroup = 
                Resources.Load<BattleFieldGroup>("ScriptableObjects/BattleFieldGroup/" + dungeon);
            return Instantiate(battleFieldGroup.GetRandomBattleField());
        }

        private void BindBattleEvent()
        {
            EventBus.Instance.SubscribeEvent<OnTurnEndInput>(this, _ =>
            {
                _turnManager.ChangeTurn();
            });
            EventBus.Instance.SubscribeEvent<OnMoveInput>(this, _ =>
            {
                // 움직임 관련 
                Debug.Log("Message Received : OnMoveInput");
            }); 
            BattleCharManager.Instance.SubscribeDeathEvents();
        }
        
        public void ShowSkillPreview(SkillModel targetSkill)
        {
            if (!_preview)
                _preview = Instantiate(previewPrefab.GetComponent<SkillPreview>(), FocusChar.CharTransform);
            _preview.gameObject.SetActive(true);
            _preview.InitPreview(FocusChar, targetSkill);
        }
        
        public void EndBattle(SystemEnum.eCharType winnerType)
        {
            // 결과 내보내기(onBattleEnd 필요)
            if (winnerType == SystemEnum.eCharType.Player)
            {
                // 이겼을 때 보수를 주는 UI를 올린다.
                UIManager.Instance.ShowUI(gameWinPrefab);
            }
            else
            {
                UIManager.Instance.ShowUI(gameOverPrefab);
            }
			// 캐릭터 모델 갱신
            
            // 탐사로 비동기 로딩. 
            //SceneLoader.LoadSceneWithLoading(SystemEnum.eScene.ExploreScene);
        }
        
    }
}


