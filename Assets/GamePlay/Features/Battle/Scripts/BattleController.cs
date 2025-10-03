using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.SceneUtil;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Common.Scripts.Skill.Preview;
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
        
        //private SkillPreview _preview;
        [SerializeField] private GameObject indicatorPrefab;
        [SerializeField] private List<GameObject> indicatorLists = new();
        [SerializeField] private Color possibleColor;
        [SerializeField] private Color blockedColor;
        
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
        
        #region 튜토부울

        public bool TutorialPlayerMove1;
        public bool TutorialEnemyMove1;
        //노벨2번 실행
        public bool TutorialPlayerMove2;
        public bool TutorialEnemyMove2;
        //노벨3번 실행
        public bool TutorialPlayerPush;
        public bool TutorialEnemyDead;
        //노벨4번 실행
        
        #endregion
        
        
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
            _turnManager.OnRoundProceeds.Invoke();
            // _turnManager.OnTurnChanged.Invoke(new TurnController.TurnModel(_turnManager.CurrentTurn));
            
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

        public bool IsModal;
        
        public void ShowPushPreview()
        {
            _battleStage.ShowGridOverlay(true);
            List<Vector2Int> aroundOne = new List<Vector2Int>() { };
        }

        public void ShowJumpPreview()
        {
            
        }
        
        // 이걸 토글용으로 사용 가능할듯함.
        public void TogglePreview(SkillModel target)
        {
            if (IsModal)
            {
                HideSkillPreview();
                IsModal = false;
            }
            else
            {
                ShowSkillPreview(target);
                IsModal = true;
            }
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
            List<Vector2Int> blockedVector = new();
            SkillRangeData data = targetSkill.skillRange;

            Vector3Int nowPosVec3 = _battleStage.Grid.WorldToCell(FocusChar.transform.position);
            Debug.Log(nowPosVec3);
            int nowX = nowPosVec3.x;
            int nowY = nowPosVec3.y;
            
            bool blocked = false;
            if (data.Origin)
            {
                Vector2Int nowPos = new Vector2Int(nowX, nowY);
                if (_battleStage.ObstacleGridCells.Contains(nowPos) ||
                    !_battleStage.PlatformGridCells.Contains(nowPos))
                    blockedVector.Add(nowPos);
                else
                    rangeVector.Add(nowPos);
            }
            {
                // 각 방향의 셀들에 따라서 도중에 장애물 있으면 그 너머는 불가한거로.
                
                for (int i = 1; i <= data.Forward; i++)
                {
                    int newX = nowX + i;
                    Vector2Int newPos = new(newX, nowY);
                    if (_battleStage.ObstacleGridCells.Contains(newPos) ||
                        !_battleStage.PlatformGridCells.Contains(newPos))
                    {
                        blocked = true;
                    }
                    if(blocked)
                        blockedVector.Add(newPos);
                    else
                        rangeVector.Add(newPos);
                }

                blocked = false;
                for (int i = 1; i <= data.Backward; i++)
                {
                    int newX = nowX - i;
                    Vector2Int newPos = new(newX, nowY);
                    if (_battleStage.ObstacleGridCells.Contains(newPos) ||
                        !_battleStage.PlatformGridCells.Contains(newPos))
                    {
                        blocked = true;
                    }
                    if(blocked)
                        blockedVector.Add(newPos);
                    else
                        rangeVector.Add(newPos);
                }
            }
            blocked = false;
            if (data.Down)
            {
                int newY = nowY - 1;
                Vector2Int newPos = new(nowX, newY);
                if (_battleStage.ObstacleGridCells.Contains(newPos) ||
                    !_battleStage.PlatformGridCells.Contains(newPos))
                    blockedVector.Add(newPos);
                else
                    rangeVector.Add(newPos);
            }

            {
                int newY = nowY - 1;
                for (int i = 1; i <= data.DownForward; i++)
                {
                    int newX = nowX + i;
                    
                    Vector2Int newPos = new(newX, newY);
                    if (_battleStage.ObstacleGridCells.Contains(newPos) ||
                        !_battleStage.PlatformGridCells.Contains(newPos))
                    {
                        blocked = true;
                    }

                    if (blocked)
                        blockedVector.Add(newPos);
                    else
                        rangeVector.Add(newPos);
                }

                blocked = false;
                for (int i = 1; i <= data.DownBackward; i++)
                {
                    int newX = nowX - i;
                    Vector2Int newPos = new(newX, nowY);
                    if (_battleStage.ObstacleGridCells.Contains(newPos) ||
                        !_battleStage.PlatformGridCells.Contains(newPos))
                    {
                        blocked = true;
                    }

                    if (blocked)
                        blockedVector.Add(newPos);
                    else
                        rangeVector.Add(newPos);
                }
            }
            blocked = false;
            if (data.Up)
            {
                int newY = nowY + 1;
                Vector2Int newPos = new(nowX, newY);
                if (_battleStage.ObstacleGridCells.Contains(newPos) ||
                    !_battleStage.PlatformGridCells.Contains(newPos))
                    blockedVector.Add(newPos);
                else
                    rangeVector.Add(newPos);
            }

            {
                int newY = nowY + 1;
                for (int i = 1; i <= data.UpForward; i++)
                {
                    int newX = nowX + i;
                    Vector2Int newPos = new(newX, newY);
                    if (_battleStage.ObstacleGridCells.Contains(newPos) ||
                        !_battleStage.PlatformGridCells.Contains(newPos))
                    {
                        blocked = true;
                    }

                    if (blocked)
                        blockedVector.Add(newPos);
                    else
                        rangeVector.Add(newPos);
                }

                blocked = false;
                for (int i = 1; i <= data.UpBackward; i++)
                {
                    int newX = nowX - i;
                    Vector2Int newPos = new(newX, newY);
                    if (_battleStage.ObstacleGridCells.Contains(newPos) ||
                        !_battleStage.PlatformGridCells.Contains(newPos))
                    {
                        blocked = true;
                    }

                    if (blocked)
                        blockedVector.Add(newPos);
                    else
                        rangeVector.Add(newPos);
                }
            }

            blocked = false;
            
            foreach (Vector2Int vector in rangeVector)
            {
                Vector3 pos = _battleStage.Grid.GetCellCenterWorld(new Vector3Int(vector.x, vector.y, 0));
                GameObject go = Instantiate(indicatorPrefab, pos, Quaternion.identity, _battleStage.Grid.transform);
                SpriteRenderer sprite = go.GetComponent<SpriteRenderer>();
                go.transform.localScale = new Vector3(_battleStage.Grid.cellSize.x, _battleStage.Grid.cellSize.y, 1);
                sprite.color = possibleColor;
                SkillIndicator indi = go.GetComponent<SkillIndicator>();
                indi.Init(FocusChar, targetSkill, false, targetSkill.skillRange.skillPivot);
                
                indicatorLists.Add(go);
            }
            foreach (Vector2Int vector in blockedVector)
            {
                Vector3 pos = _battleStage.Grid.GetCellCenterWorld(new Vector3Int(vector.x, vector.y, 0));
                GameObject go = Instantiate(indicatorPrefab, pos, Quaternion.identity, _battleStage.Grid.transform);
                go.transform.localScale = new Vector3(_battleStage.Grid.cellSize.x, _battleStage.Grid.cellSize.y, 1);
                SpriteRenderer sprite = go.GetComponent<SpriteRenderer>();
                sprite.color = blockedColor;
                SkillIndicator indi = go.GetComponent<SkillIndicator>();
                indi.Init(FocusChar, targetSkill, true, targetSkill.skillRange.skillPivot);
                
                indicatorLists.Add(go);
            }
            
            
        }
        
        public void HideSkillPreview()
        {
            foreach (GameObject go in indicatorLists)
            {
                Destroy(go);
            }
            indicatorLists.Clear();
            _battleStage.ShowGridOverlay(false);
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
            SceneLoader.LoadSceneWithLoading(SystemEnum.eScene.ExploreScene);
        }
        
    }
}


