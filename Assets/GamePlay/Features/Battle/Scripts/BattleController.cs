using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.BattleTurn;
using GamePlay.Features.Scripts.Skill.Preview;
using GamePlay.Features.Battle.Scripts.Unit;
using System.Collections.Generic;
using UIs.Runtime;
using Unity.VisualScripting;
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
        [SerializeField] private string DebugMapName;
        [SerializeField] private float cameraSize = 11;
        
        private IBattleStageSource _stageSource;
        private IMapLoader _mapLoader;
        private StageField _battleStage;
        
        private TurnController _turnManager;
        public CharBase FocusChar => _turnManager.TurnOwner;
        public IReadOnlyList<CharacterModel> PartyList => _stageSource.PlayerParty.partyMembers;
        
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
                BattlePayload.Instance.SetBattleData(new Party(), DebugDungeon, DebugMapName);
                Debug.Log("Stage source not set : Using Battle Payload");
                _stageSource = new BattlePayloadSource();
            }
            _mapLoader = new StageLoader(_stageSource, battleFieldDB);
            await BattleInitialize();
            await UIManager.Instance.ShowViewAsync(ViewID.BattleSceneView);

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
            List<CharBase> battleMembers = await _battleStage.SpawnAllUnits(playerParty);
            
            // 턴 관리기 초기화
            _turnManager = new TurnController(battleMembers); 
            _turnManager.ChangeTurn();
            
            // 전투 공통 이벤트 처리
            BindBattleEvent();
            
            Debug.Log("Battle Initialization Complete");
            BattlePayload.Instance.Clear();
            
            //카메라 초기화
            Camera.main.orthographicSize = cameraSize;
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

        public void ShowPushPreview()
        {
            _battleStage.ShowGridOverlay(true);
            List<Vector2Int> aroundOne = new List<Vector2Int>() { };
        }

        public void ShowSkillPreview(SkillModel targetSkill)
        {
            if (!_battleStage)
            {
                Debug.LogError("[BattleController] : Battle Stage not set");
                return;
            }

            _battleStage.ShowGridOverlay(true);
            List<Vector2Int> rangeVector = new();
            ;
            List<Vector2Int> blockedVector = new();
            SkillRangeData data = targetSkill.skillRange;

            Vector3Int nowPosVec3 = _battleStage.Grid.WorldToCell(FocusChar.transform.position);
            int nowX = nowPosVec3.x;
            int nowY = nowPosVec3.y;
            if (data.Origin)
            {
                // 각 방향의 셀들에 따라서 도중에 장애물 있으면 그 너머는 불가한거로.

                for (int i = 1; i <= data.Forward; i++)
                {
                    rangeVector.Add(new Vector2Int(nowX + i, nowY));
                }

                for (int i = 1; i <= data.Backward; i++)
                {
                    rangeVector.Add(new Vector2Int(nowX - i, nowY));
                }
            }

            if (data.Down)
            {
                for (int i = 1; i <= data.DownForward; i++)
                {
                    rangeVector.Add(new Vector2Int(nowX + i, nowY - 1));
                }

                for (int i = 1; i <= data.DownBackward; i++)
                {
                    rangeVector.Add(new Vector2Int(nowX - i, nowY - 1));

                }
            }

            if (data.Up)
            {
                for (int i = 1; i <= data.UpForward; i++)
                {
                    rangeVector.Add(new Vector2Int(nowX + i, nowY + 1));

                }

                for (int i = 1; i <= data.UpBackward; i++)
                {
                    rangeVector.Add(new Vector2Int(nowX - i, nowY + 1));

                }
            }

            //_battleStage.PaintRange(rangeVector, blockedVector);
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


