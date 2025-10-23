using AngelBeat;
using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.Utils;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Common.Scripts.Keyword;
using GamePlay.Common.Scripts.Skill;
using GamePlay.Features.Scripts.Keyword;
using GamePlay.Features.Scripts.Skill;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using DataManager = Core.Scripts.Managers.DataManager;

namespace GamePlay.Features.Battle.Scripts.Unit
{
    public abstract class CharBase : MonoBehaviour
    {
        private static readonly int Move = Animator.StringToHash("Move");
        private static readonly int Idle = Animator.StringToHash("Idle");
        
        #region Member Field
        [SerializeField] private long _index;
        [SerializeField] private GameObject _SkillRoot;
        [SerializeField] protected Animator _Animator;
        [SerializeField] private Collider2D _battleCollider;
        [SerializeField] private GameObject _CharCameraPos;
        [SerializeField] private GameObject _charSnapShot;
        [SerializeField] private Rigidbody2D _rigid;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private Transform _charUnitRoot;
        
        //TODO : 점프 효과 관련 프리팹을 어떻게 관리할지 생각할 것
        [SerializeField] private static GameObject jumpOutFX;
        [SerializeField] private static GameObject jumpInFX;
        
        private SpriteRenderer  _spriteRenderer;
        private Transform       _charTransform;
        private CharAnim        _charAnim;
        
        private CharacterModel  _charInfo;
        private CharStat        _runtimeStat;
        private SkillInfo       _skillInfo;
        private KeywordInfo     _keywordInfo;

        private List<SkillBase> _skills = new();
        
        private bool _isAction = false;    // 행동중인가? 판별
        protected long _uid;

        private float _movePoint;
        private float _currentMovePoint;
        
        private Camera _mainCamera;
        #endregion
        
        #region Properties
        public long Index => _index;
        public string UnitName;
        public Transform CharTransform => _charTransform;
        public Transform CharUnitRoot => _charUnitRoot;
        public GameObject SkillRoot => _SkillRoot;
        public Collider2D BattleCollider => _battleCollider;
        public GameObject CharCameraPos => _CharCameraPos;
        public GameObject CharSnapShot => _charSnapShot;
        public CharAnim CharAnim => _charAnim;
        public Rigidbody2D Rigid => _rigid;
        public Camera MainCamera => _mainCamera;
        
        //public Sprite CharacterSprite => _characterSprite;
        public List<SkillBase> Skills => _skills;
        public CharStat RuntimeStat
        {
            get => _runtimeStat;
            private set
            {
                _runtimeStat = value;
                _runtimeStat.ClearChangeEvent();
                _runtimeStat.OnStatChanged += (stat, delta, changed) =>
                {
                    if (stat == SystemEnum.eStats.NHP && changed <= 0)
                    {
                        Debug.Log("사망");
                        CharDead();
                    }

                };
                
            }
        }
        
        #region Stat Properties for Utility
        public float CurrentHP => _runtimeStat.GetStat(SystemEnum.eStats.NHP);
        public float MaxHP => _runtimeStat.GetStat(SystemEnum.eStats.NMHP);
        public float Armor => _runtimeStat.GetStat(SystemEnum.eStats.DEFENSE);
        public float Dodge => _runtimeStat.GetStat(SystemEnum.eStats.EVATION);
        public float BonusAccuracy => _runtimeStat.GetStat(SystemEnum.eStats.ACCURACY_INCREASE);
        public float DamageIncrease => _runtimeStat.GetStat(SystemEnum.eStats.DAMAGE_INCREASE);
        public float MovePoint => _movePoint;
        
        #endregion
        public Transform FloatingUIRoot
        {
            get
            {
                Transform uiRoot = CharTransform.Find("FloatingUIRoot");
                if (uiRoot == null)
                {
                    GameObject root = new("FloatingUIRoot");
                    root.transform.SetParent(CharTransform);
                    return root.transform;
                }
                return uiRoot;
            }
        }
        
        public SkillInfo SkillInfo => _skillInfo;
        public KeywordInfo KeywordInfo => _keywordInfo;

        public CharacterModel CharInfo
        {
            get => _charInfo;
            protected set
            {
                _charInfo = value;
                if (value.Index != _index)
                {
                    Debug.LogError("인덱스 불일치. 코드 이상 혹은 데이터의 캐릭터 정보와 프리팹 차이 학인 바람");
                    return;
                }
                
                _skillInfo = new SkillInfo(this);
                _keywordInfo = new KeywordInfo(this);
                _runtimeStat = value.BaseStat;
                
            }
        }
        
        protected virtual SystemEnum.eCharType CharType => SystemEnum.eCharType.None;
        
        public long GetID() => _uid;

        public SystemEnum.eCharType GetCharType()
        {
            return CharType;
        }
        
        #endregion       
        
        #region Constructor
        protected CharBase() { }
        #endregion
        
        #region Initialization
        private void Awake()
        {
            _charTransform = transform;
            _charUnitRoot = Util.FindChild<Transform>(gameObject, "UnitRoot");
            _spriteRenderer = _charUnitRoot.GetComponent<SpriteRenderer>();
            _uid = BattleCharManager.Instance.GetNextID();
            _charAnim = new();
            _mainCamera = Camera.main;
            
        }
        
        /// <summary>
        /// 현재 start에서는 애니메이션 초기화만 함
        /// </summary>
        private void Start()
        {
            if (_charAnim != null)
            {
                _charAnim.Initialized(_Animator);
            }
        }

        public virtual async UniTask CharInit(CharacterModel charModel)
        {
            _charInfo = charModel; //모델
            _runtimeStat = charModel.BaseStat; // 스탯 복사
            
            //스킬 초기화 - 이미 ActiveSkills로 저장해놓은 애들만 뽑아줌.
            IReadOnlyList<SkillModel> skillModels = charModel.ActiveSkills;
            _skillInfo = new SkillInfo(this);
            await _skillInfo.InitAsync(skillModels);
            
        }
        #endregion

        public int GetSKillIndexFromModel(SkillModel skillModel)
        {
            int index = 0;
            foreach (var skill in _skills)
            {
                if (skillModel.SkillName == skill.SkillModel.SkillName)
                {
                    return index;
                }
                index++;
            }

            return -1;
        }
        
        
        #region Initialize & Save Character Model
        public void UpdateCharacterInfo(CharacterModel charInfo)
        {
            // TODO : 나중에 MODEL을 제외. 
            CharInfo = charInfo;
        }
        
        public CharacterModel SaveCharModel()
        {
            return new CharacterModel(_charInfo);
        }
        #endregion
        
        #region Damage & Stat Section
        
        /// <summary>
        /// 공격자의 공격 시도 시 호출하는 이벤트 
        /// </summary>
        public Action<DamageParameter> OnAttackTrial;
        
        /// <summary>
        /// 공격자의 공격 성공 시 호출하는 이벤트
        /// </summary>
        public Action<DamageParameter> OnAttackSuccess;
        
        /// <summary>
        /// 피격자의 피격 시 호출하는 이벤트
        /// </summary>
        public event Action<DamageParameter> OnHit;
        
        /// <summary>
        /// 회피 시 호출하는 이벤트
        /// </summary>
        public event Action<DamageParameter> OnMiss;
        
        /// <summary>
        /// 대미지 관련 파라미터로 이벤트 발생시킴.
        /// </summary>
        /// <param name="damageInfo">기존 대미지 파라미터</param>
        /// <param name="accuracy">스킬 적중 요소 : 명중률</param>
        /// <param name="skillDamageMultiplier">스킬 대미지 요소 : 스킬 고유 인자</param>
        public void SkillDamage(DamageParameter damageInfo, float accuracy, float skillDamageMultiplier)
        {
            CharBase attacker = damageInfo.Attacker;
            attacker.OnAttackTrial?.Invoke(damageInfo);
            if (TryEvade(attacker, accuracy))
            {
                Debug.Log($"{attacker.name}의 공격을 {name}이 회피했습니다.");
                OnMiss?.Invoke(damageInfo);
            }
            else
            {
                Debug.Log($"{attacker.name}의 공격이 {name}에게 적중했습니다.");
                attacker.OnAttackSuccess?.Invoke(damageInfo);
                
                float finalDamage = damageInfo.FinalDamage *
                                    skillDamageMultiplier * 
                                    (1f + attacker.DamageIncrease * 0.01f) *
                                    (1f - Armor * 0.01f);
                
                
                // 나중에 바꿔
                RuntimeStat.ReceiveDamage(damageInfo.FinalDamage);
                
                Debug.Log($"{damageInfo.FinalDamage}");
                Debug.Log($"{finalDamage}데미지");
                Debug.Log($"{CurrentHP} / {MaxHP}");
                OnHit?.Invoke(damageInfo);
            }
        }
        
        /// <summary>
        /// 명중 처리 역할
        /// </summary>
        /// <param name="attacker">공격자</param>
        /// <param name="accuracy">해당 스킬의 기본 명중률</param>
        /// <returns>피했나? 안피했나?</returns>
        private bool TryEvade(CharBase attacker, float accuracy)
        {
            float hitChance = accuracy + attacker.BonusAccuracy - Dodge + 5;
            return UnityEngine.Random.Range(0f, 100f) > Mathf.Clamp(hitChance, 0, 100);
        }
        #endregion
        
        
        #region Character Death
        public event Action OnCharDead;
        public virtual void CharDead()
        {
            Type myType = GetType();
            BattleCharManager.Instance.Clear(myType, _uid);
            Debug.Log($"{gameObject.name} is dead");
            
            OnCharDead?.Invoke();
            OnUpdate = null;
            RuntimeStat.ClearChangeEvent();
                gameObject.SetActive(false);
           //Destroy(gameObject);
        }
        
        public virtual void CharDestroy()
        {
            Type myType = this.GetType();
            BattleCharManager.Instance.Clear(myType, _uid);
            Destroy(gameObject);
        }
        #endregion

        #region Character Animation
        //public void SetStateAnimationIndex(PlayerState state, int index = 0)
        //{
        //    _indexPair[state] = index;
        //}
        //public void PlayStateAnimation(PlayerState state)
        //{
        //    _charAnim.PlayAnimation(state);
        //}
        #endregion
        
        #region Character movement Control

        private bool _isGrounded = true;
        public bool IsGrounded { get => _isGrounded; private set { _isGrounded = value; } }
        
        /// <summary>
        /// 캐릭터 이동 연출 메서드
        /// 외부에서 이동력을 조사하므로, 이동력을 초과하지 않음을 전제로 함.
        /// </summary>
        public async UniTask CharMove(Vector3 targetPos)
        {
            RuntimeStat.ChangeAP(Mathf.CeilToInt(Mathf.Abs((targetPos - transform.position).x)));
            _Animator.SetTrigger(Move);
            while ((transform.position - targetPos).sqrMagnitude > 0.05f)
            {
                transform.position += (targetPos - transform.position).normalized * Time.deltaTime * moveSpeed;
                if((targetPos - transform.position).x < 0)
                    _spriteRenderer.flipX = true;
                else
                    _spriteRenderer.flipX = false;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            _Animator.SetTrigger(Idle);
        }
        
        /// <summary>
        /// 목적지로 점프 연출하는 메서드
        /// </summary>
        public async UniTask CharJump(Vector3 targetPos, CancellationToken ct)
        {
            SpriteRenderer sr = _charUnitRoot.GetComponent<SpriteRenderer>();
            if (sr) sr.enabled = false;
            await PlayFxOnce(jumpOutFX, transform.position, ct);
            
            transform.position = targetPos;
            await PlayFxOnce(jumpInFX, transform.position, ct);
            sr.enabled = true;
        }
        
        #endregion
        
        #region Unity Events
        public event Action OnUpdate;
        private void Update()
        {
            OnUpdate?.Invoke();
        }
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Platform"))
            {
                if(!_isGrounded)
                    _isGrounded = true;
            }
        }
        #endregion
        
        #region Util로 옮길 부분
        
        //TODO : Particlesystem인지 animator인지 결정되면 알아서 최적화할것
        async UniTask PlayFxOnce(GameObject prefab, Vector3 pos, CancellationToken ct)
        {
            if (!prefab) return;
            var go = Instantiate(prefab, pos, Quaternion.identity);

            // ParticleSystem이면 모두 재생 후 살아있는 동안 대기
            ParticleSystem[] ps = go.GetComponentsInChildren<ParticleSystem>(true);
            // null 안터짐
            if (ps.Length > 0)
            {
                foreach (var p in ps) p.Play();
                while (!ct.IsCancellationRequested && System.Array.Exists(ps, p => p && p.IsAlive(true)))
                    await Cysharp.Threading.Tasks.UniTask.Yield(PlayerLoopTiming.Update, ct);
                Destroy(go);
                return;
            }

            // Animator 기반 FX면 트리거나 자동 플레이를 가정, 적당한 보호 딜레이(필요시 Animation Event로 대체)
            var anim = go.GetComponentInChildren<Animator>();
            if (anim)
                await Cysharp.Threading.Tasks.UniTask.Delay(System.TimeSpan.FromSeconds(0.5), cancellationToken: ct);

            Destroy(go);
        }
        #endregion
        
    }
}

