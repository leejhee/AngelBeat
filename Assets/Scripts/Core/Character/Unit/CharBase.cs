using AngelBeat.Core;
using AngelBeat.Core.Character;
using AngelBeat.Core.SingletonObjects.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AngelBeat
{
    public abstract class CharBase : MonoBehaviour
    {
        #region Member Field
        [SerializeField] private long _index;
        [SerializeField] private GameObject _SkillRoot;
        [SerializeField] protected Animator _Animator;
        [SerializeField] private Collider2D _battleCollider;
        [SerializeField] private GameObject _CharCameraPos;
        [SerializeField] private GameObject _charSnapShot;
        [SerializeField] private Rigidbody2D _rigid;
        [SerializeField] private float moveSpeed = 5f;
        
        //TODO : 현 CharBase와 겹치는 사항 관리하기
        private CharacterModel  _charInfo;
        
        private Transform       _charTransform;
        private Transform       _charUnitRoot;
        
        private CharData        _charData;  
        private CharStat        _charStat;
        private CharAnim        _charAnim = null;
        
        private ExecutionInfo   _executionInfo;
        private SkillInfo       _skillInfo;
        private KeywordInfo     _keywordInfo;

        private PlayerState _currentState; // 현재 상태
        private bool _isAction = false;    // 행동중인가? 판별
        private Dictionary<PlayerState, int> _indexPair = new();
        
        protected long _uid;
        #endregion
        
        #region Properties
        public long Index => _index;
        public string UnitName => _charData.charName;
        public Transform CharTransform => _charTransform;
        public Transform CharUnitRoot => _charUnitRoot;
        public GameObject SkillRoot => _SkillRoot;
        public Collider2D BattleCollider => _battleCollider;
        public GameObject CharCameraPos => _CharCameraPos;
        public GameObject CharSnapShot => _charSnapShot;
        public CharAnim CharAnim => _charAnim;
        public Rigidbody2D Rigid => _rigid;
        public float MoveSpeed => moveSpeed;
        public CharStat CharStat
        {
            get => _charStat;
            private set
            {
                _charStat = value;
                _charStat.ClearChangeEvent();
                _charStat.OnStatChanged += (stat, changed) =>
                {
                    if (stat == SystemEnum.eStats.NHP && changed <= 0)
                        CharDead();
                };
            }
        }

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
        
        public ExecutionInfo ExecutionInfo => _executionInfo;
        public SkillInfo SkillInfo => _skillInfo;
        public KeywordInfo KeywordInfo => _keywordInfo;
        public PlayerState PlayerState => _currentState;

        // RULE : set은 반드시 초기화 시에 사용하고, 값 수정 필요 시 get으로만 할 것.
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
                _charData = DataManager.Instance.GetData<CharData>(value.Index);
            
                _skillInfo = new SkillInfo(this);
                _skillInfo?.Init(_charData.charSkillList);
                _executionInfo = new();
                _keywordInfo = new(this);
                _charStat = value.Stat;
                
            }
        }
        
        protected virtual SystemEnum.eCharType CharType => _charData.defaultCharType;
        
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
            _uid = BattleCharManager.Instance.GetNextID();
            _charAnim = new();
            
            _charData = DataManager.Instance.GetData<CharData>(_index);
            if (_charData != null)
            {
                CharStatData charStat = DataManager.Instance.GetData<CharStatData>(_charData.charStat);
                if (charStat == null)
                {
                    Debug.LogError($"캐릭터 ID : {_index} 데이터 Get 성공 charStat {_charData.charStat} 데이터 Get 실패");
                }
                CharStat = new CharStat(charStat);
            }
            else
            {
                Debug.LogError($"캐릭터 ID : {_index} Data 데이터 Get 실패");
            }
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
            foreach (PlayerState state in Enum.GetValues(typeof(PlayerState)))
            {
                _indexPair[state] = 0;
            }
            
            CharInit();
        }

        

        protected virtual void CharInit()
        {
            
        }
        #endregion
        
        
        
        #region Initialize & Save Character Model
        public void UpdateCharacterInfo(CharacterModel charInfo)
        {
            // TODO : 나중에 MODEL을 제외. 
            CharInfo = charInfo;
        }
        
        public CharacterModel SaveCharModel()
        {
            return new CharacterModel(_charData, _charStat, _charInfo.Skills);
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
        public Action<DamageParameter> OnHit;
        
        /// <summary>
        /// 회피 시 호출하는 이벤트
        /// </summary>
        public Action<DamageParameter> OnMiss;
        
        
        public void SkillDamage(DamageParameter damageInfo)
        {
            // 여기에 명중 굴림
            OnAttackTrial?.Invoke(damageInfo);
            
            
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
            CharStat.ClearChangeEvent();
            
            Destroy(gameObject);
        }
        
        public virtual void CharDestroy()
        {
            Type myType = this.GetType();
            BattleCharManager.Instance.Clear(myType, _uid);
            Destroy(gameObject);
        }
        #endregion

        #region Character Animation
        public void SetStateAnimationIndex(PlayerState state, int index = 0)
        {
            _indexPair[state] = index;
        }
        public void PlayStateAnimation(PlayerState state)
        {
            _charAnim.PlayAnimation(state);
        }
        #endregion
        
        #region Character movement Control

        private bool _isGrounded = true;
        public bool IsGrounded { get => _isGrounded; private set { _isGrounded = value; } }

        public void CharMove(Vector3 targetDir, float speed = 5f)
        {
            if (CharStat.UseActionPoint(speed))
            {
                Vector3 scale = CharTransform.localScale;
                scale.x = -targetDir.x;
                CharTransform.localScale = scale;
                CharTransform.position += targetDir * (speed * Time.deltaTime);
            }
        }
        
        public IEnumerator CharJump()
        {
            if (CharStat.UseActionPoint(SystemConst.fps))
            {
                if (_isGrounded)
                {
                    _isGrounded = false;
                    gameObject.layer = LayerMask.NameToLayer("Ignore Collision");
                    _battleCollider.enabled = false;
                    _rigid.AddForce(new Vector2(0, 12.5f), ForceMode2D.Impulse);
                    yield return new WaitForSeconds(1f);
                    gameObject.layer = LayerMask.NameToLayer("Character");
                    _battleCollider.enabled = true;
                }
                
            }
        }

        public IEnumerator CharDownJump()
        {
            if (_isGrounded)
            {
                gameObject.layer = LayerMask.NameToLayer("Ignore Collision");
                IsGrounded = false;
                _battleCollider.enabled = false;
                yield return new WaitForSeconds(1f);
                gameObject.layer = LayerMask.NameToLayer("Character");
                _battleCollider.enabled = true;
            }
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
    }
}

