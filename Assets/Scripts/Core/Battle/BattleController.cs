using AngelBeat.Core.Character;
using AngelBeat.Core.Map;
using AngelBeat.Core.SingletonObjects;
using AngelBeat.Core.SingletonObjects.Managers;
using AngelBeat.Scene;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        
        #region UI Member
        [SerializeField] private GameObject gameOverPrefab;
        [SerializeField] private GameObject gameWinPrefab;
        [SerializeField] private GameObject previewPrefab;
        
        private SkillPreview _preview;
        #endregion
        
        private IBattleStageSource _stageSource;
        private IMapLoader _mapLoader;
        
        private TurnController _turnManager;
        private CharBase FocusChar => _turnManager.TurnOwner;
        
        private void Start()
        {
            Debug.Log("Starting Battle...");
            if (_stageSource == null)
            {
                Debug.Log("Stage source not set : Using Battle Payload");
                _stageSource = new BattlePayloadSource();
            }
            _mapLoader = new StageLoader(_stageSource);
            
            BattleInitialize();
        }
        
        /// <summary> 테스트 용도로 stage source를 관리체에 제공한다. </summary>
        /// <param name="stageSource"> 테스트 용도의 stage source. </param>
        public void SetStageSource(IBattleStageSource stageSource) => _stageSource = stageSource;
        
        /// <summary>
        /// 역할 : 전투 진입 시의 최초 동작 메서드. 전투 환경을 초기화한다.
        /// </summary>
        private void BattleInitialize()
        {
            Debug.Log("Starting Battle Initialization...");
            try
            {
                string stageName = _stageSource.StageName;
                Party playerParty = _stageSource.PlayerParty;
                
                StageField battleField = Instantiate(_mapLoader.GetBattleField(stageName));
            
                List<CharBase> battleMembers = battleField.SpawnAllUnits(playerParty);
                _turnManager = new TurnController(battleMembers); 
                _turnManager.ChangeTurn();
            
                BindBattleEvent();
            }
            catch (Exception e)
            {
                Debug.LogError($"[에러 발생] : {e.Message}\n{e.StackTrace}");
            }
            
            Debug.Log("Battle Initialization Complete");
            BattlePayload.Instance.Clear();
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


