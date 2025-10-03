using Core.Scripts.Foundation.Define;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Entities.Scripts.Character;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.BattleTurn;
using GamePlay.Features.Scripts.Skill.Preview;
using GamePlay.Features.Battle.Scripts.Unit;
using GamePlay.Skill;
using System.Collections.Generic;
using UIs.Runtime;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts
{
    // 잠시 싱글턴 사용한다.
    public class BattleController : MonoBehaviour
    {
        #region singleton
        private static BattleController instance;

        public static BattleController Instance
        {
            get
            {
                GameObject go = GameObject.Find("BattleController");
                if (!go)
                {
                    go = new GameObject("BattleController");
                    instance = go.AddComponent<BattleController>();
                }
                return instance;
            }
            private set => instance = value;
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            GameManager.Instance.GameState = SystemEnum.GameState.Battle;
        }
        #endregion
        
        #region UI Member
        [SerializeField] private GameObject gameOverPrefab;
        [SerializeField] private GameObject gameWinPrefab;
        [SerializeField] private GameObject previewPrefab;
        
        private SkillPreview _preview;
        #endregion

        [SerializeField] private BattleFieldDB battleFieldDB;

        [SerializeField] private SystemEnum.Dungeon DebugDungeon;
        
        private IBattleStageSource _stageSource;
        private IMapLoader _mapLoader;
        
        private TurnController _turnManager;
        public CharBase FocusChar => _turnManager.TurnOwner;
        public IReadOnlyList<CharacterModel> PartyList => _stageSource.PlayerParty.partyMembers;

        private StageField _battleStage;
        
        #region UI Model
        
        public class TurnStructureModel
        {
            public IReadOnlyCollection<Turn> TurnCollection;
            
            public TurnStructureModel(IReadOnlyCollection<Turn> turnCollection) =>  TurnCollection = turnCollection;
        }
        public TurnStructureModel GetChangedTurnStructureModel => new(_turnManager.TurnCollection);
        
        //temporary
        public TurnController TurnController => _turnManager;
        
        #endregion
        private async void Start()
        {
            Debug.Log("Starting Battle...");
            if (_stageSource == null)
            {
                Debug.Log("Stage source not set : Using Battle Payload");
                _stageSource = new BattlePayloadSource();
            }
            _mapLoader = new StageLoader(_stageSource, battleFieldDB);
            await UIManager.Instance.ShowViewAsync(ViewID.BattleSceneView);
            await BattleInitialize();
            
        }
        
        /// <summary> 테스트 용도로 stage source를 관리체에 제공한다. </summary>
        /// <param name="stageSource"> 테스트 용도의 stage source. </param>
        public void SetStageSource(IBattleStageSource stageSource) => _stageSource = stageSource;
        
        /// <summary>
        /// 역할 : 전투 진입 시의 최초 동작 메서드. 전투 환경을 초기화한다.
        /// </summary>
        private async UniTask BattleInitialize()
        {
            Debug.Log("Starting Battle Initialization...");

            string stageName = _stageSource.StageName;
            Party playerParty = _stageSource.PlayerParty;
            
            // 맵 띄우기
            _battleStage = await _mapLoader.InstantiateBattleFieldAsync(stageName);
            
            // 맵에다가 파티를 포함시켜서 모든 애들 띄우기
            List<CharBase> battleMembers = _battleStage.SpawnAllUnits(playerParty);
            
            // 턴 관리기 초기화
            _turnManager = new TurnController(battleMembers); 
            _turnManager.ChangeTurn();
            
            // 전투 공통 이벤트 처리
            BindBattleEvent();
            
            Debug.Log("Battle Initialization Complete");
            BattlePayload.Instance.Clear();
        }
        
        private void BindBattleEvent()
        {
            //EventBus.Instance.SubscribeEvent<OnTurnEndInput>(this, _ =>
            //{
            //    _turnManager.ChangeTurn();
            //});
            //EventBus.Instance.SubscribeEvent<OnMoveInput>(this, _ =>
            //{
            //    // 움직임 관련 
            //    Debug.Log("Message Received : OnMoveInput");
            //}); 
            //BattleCharManager.Instance.SubscribeDeathEvents();
        }
        
        public void ShowSkillPreview(SkillModel targetSkill)
        {
            //if (!_preview)
            //    _preview = Instantiate(previewPrefab.GetComponent<SkillPreview>(), FocusChar.CharTransform);
            //_preview.gameObject.SetActive(true);
            //_preview.InitPreview(FocusChar, targetSkill);

            if (!_battleStage)
            {
                Debug.LogError("[BattleController] : Battle Stage not set");
                return;
            }
            //List<SkillRangeData> ranges = DataManager.Instance.
        }
        
        public void EndBattle(SystemEnum.eCharType winnerType)
        {
            // 결과 내보내기(onBattleEnd 필요)
            if (winnerType == SystemEnum.eCharType.Player)
            {
                // 이겼을 때 보수를 주는 UI를 올린다.
                //UIManager.Instance.ShowUI(gameWinPrefab);
            }
            else
            {
                //UIManager.Instance.ShowUI(gameOverPrefab);
            }
			// 캐릭터 모델 갱신
            
            // 탐사로 비동기 로딩. 
            //SceneLoader.LoadSceneWithLoading(SystemEnum.eScene.ExploreScene);
        }
        
    }
}


