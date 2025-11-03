using AngelBeat;
using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.Utils;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Common.Scripts.Entities.Character.Components;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Common.Scripts.Keyword;
using GamePlay.Common.Scripts.Skill;
using GamePlay.Features.Battle.Scripts.UI.BattleHovering;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GamePlay.Features.Battle.Scripts.Unit
{
    public abstract class CharBase : MonoBehaviour
    {
        private static readonly int Move = Animator.StringToHash("Move");
        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int OnAttack = Animator.StringToHash("OnAttack");
        private static readonly int JumpOut = Animator.StringToHash("JumpOut");
        private static readonly int JumpIn = Animator.StringToHash("JumpIn");
        private static readonly int Push = Animator.StringToHash("Push");
        private static readonly int Evade = Animator.StringToHash("Evade");
        
        
        #region Member Field
        [SerializeField] private long _index;
        [SerializeField] private GameObject _SkillRoot;
        [SerializeField] protected Animator _Animator;
        [SerializeField] private BoxCollider2D _battleCollider;
        [SerializeField] private GameObject _CharCameraPos;
        [SerializeField] private GameObject _charSnapShot;
        [SerializeField] private Rigidbody2D _rigid;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private Transform _charUnitRoot;
        [SerializeField] private CharacterHpBar _hpBar;
        
        //TODO : 점프 효과 관련 프리팹을 어떻게 관리할지 생각할 것
        [SerializeField] private GameObject jumpOutFX;
        [SerializeField] private GameObject jumpInFX;
        [SerializeField] private GameObject pushFX;
        
        private SpriteRenderer  _spriteRenderer;
        private SpriteRenderer  _outlineRenderer;
        private Transform       _charTransform;
        private CharAnim        _charAnim;
        
        private CharacterModel  _charInfo;
        private CharStat        _runtimeStat;
        private SkillInfo       _skillInfo;
        private KeywordInfo     _keywordInfo;

        
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
        public BoxCollider2D BattleCollider => _battleCollider;
        public GameObject CharCameraPos => _CharCameraPos;
        public GameObject CharSnapShot => _charSnapShot;
        public CharAnim CharAnim => _charAnim;
        public Rigidbody2D Rigid => _rigid;
        public Camera MainCamera => _mainCamera;
        
        //public Sprite CharacterSprite => _characterSprite;
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

        public bool IsDead;
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
            _outlineRenderer = _charUnitRoot.GetChild(0).GetComponent<SpriteRenderer>();
            _uid = BattleCharManager.Instance.GetNextID();
            _charAnim = new();
            _mainCamera = Camera.main;
            
            
            Debug.Log($"여기에요 시발련들아 {_charTransform}");
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
        /// <param name="playOnAttack">피격 모션 재생할까?</param>
        /// <param name="suppressIdle">Idle suppression</param>
        public async UniTask SkillDamage(DamageParameter damageInfo, bool playOnAttack=true, bool suppressIdle=false)
        {
            CharBase attacker = damageInfo.Attacker;
            SkillModel model = damageInfo.Model;
            //attacker.OnAttackTrial?.Invoke(damageInfo); 외부에서 발행하도록 한다.
            
            Debug.Log($"{attacker.name}의 공격이 {name}에게 적중했습니다.");
            attacker.OnAttackSuccess?.Invoke(damageInfo);
            
            // Calculation
            float attackStat = attacker.RuntimeStat.GetAttackStat(model.skillType);
            float defenseStat = _runtimeStat.GetDefenseStat(model.skillType);
            SkillDamageData damageData = model.skillDamage;
            
            long finalDamage = 0;
            switch (model.skillType)
            {
                case SystemEnum.eSkillType.Heal:
                    finalDamage = -(long)Mathf.Ceil(
                        damageData.DamageCoefficient *
                        Random.Range(damageData.RandMin, damageData.RandMax + 1)
                    );
                    break;
                case SystemEnum.eSkillType.Debuff:
                    finalDamage = (long)Mathf.Ceil(
                        damageData.DamageCoefficient *
                        (1 + attacker.RuntimeStat.GetStat(SystemEnum.eStats.DAMAGE_INCREASE) / 100f)
                    );
                    break;
                default:
                    finalDamage = (long)Mathf.Ceil(
                        attackStat *
                        damageData.DamageCoefficient *
                        Random.Range(damageData.RandMin, damageData.RandMax + 1) *
                        (1 + attacker.RuntimeStat.GetStat(SystemEnum.eStats.DAMAGE_INCREASE) / 100f) *
                        (100 - defenseStat) / 100f
                    );
                    break;
            }
            
            _runtimeStat.ReceiveDamage(finalDamage);
            Debug.Log($"{finalDamage}데미지");
            Debug.Log($"{CurrentHP} / {MaxHP}");
            OnHit?.Invoke(damageInfo);
            if(playOnAttack)
                _Animator.SetTrigger(OnAttack);
            
            await UniTask.Delay(60);
            if(!suppressIdle)
                _Animator.SetTrigger(Idle);
        }

        /// <summary>
        /// 명중 계산 -> 성공하면 이벤트 발행하고 연출
        /// </summary>
        /// <returns>피했나? 안피했나?</returns>
        public async UniTask<bool> TryEvade(DamageParameter damageInfo)
        {
            SkillModel model = damageInfo.Model;
            if (model.skillType == SystemEnum.eSkillType.MagicAttack) return false;
            float hitChance = model.skillAccuracy + damageInfo.Attacker.BonusAccuracy - Dodge + 5;
            bool succeed = Random.Range(0f, 100f) > Mathf.Clamp(hitChance, 0, 100);
            if (!succeed)
            {
                return false;
            }
            Debug.Log($"{damageInfo.Attacker.name}의 공격을 {name}이 회피했습니다.");
            _Animator.SetTrigger(Evade);
            OnMiss?.Invoke(damageInfo); // '회피 시'
            await UniTask.Delay(60);
            _Animator.SetTrigger(Idle);
            return true;
        }
        
        
        
        #endregion
        
        
        #region Character Death
        public event Action OnCharDeadPersonal;
        public virtual void CharDead()
        {
            IsDead = true;
            BattleController.Instance?.HandleUnitDeath(this);
            
            OnCharDeadPersonal?.Invoke();
            OnUpdate = null;
            RuntimeStat.ClearChangeEvent();
            
            Type myType = GetType();
            BattleCharManager.Instance.Clear(myType, _uid);
            Debug.Log($"{gameObject.name} is dead");
            gameObject.SetActive(false);
            BattleCharManager.Instance.CheckDeathEvents(CharType);
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
        
        private bool _lastDirectionRight; // true 오른쪽, false 왼쪽
        
        public bool LastDirection
        {
            get => _lastDirectionRight;
            set
            {
                _lastDirectionRight = value;
                if(CharType == SystemEnum.eCharType.Player)
                    _spriteRenderer.flipX = !LastDirection;
                else
                    _spriteRenderer.flipX = LastDirection;
            }
        }

        /// <summary>
        /// 캐릭터 이동 연출 메서드
        /// 외부에서 이동력을 조사하므로, 이동력을 초과하지 않음을 전제로 함.
        /// </summary>
        public async UniTask CharMove(Vector3 targetPos)
        {
            _Animator.SetTrigger(Move);
    
            Vector2 target = targetPos;
            float arrivalThreshold = 0.05f;
    
            while (Vector2.Distance(_rigid.position, target) > arrivalThreshold)
            {
                Vector2 direction = (target - _rigid.position).normalized;
                Vector2 newPosition = _rigid.position + direction * (moveSpeed * Time.fixedDeltaTime);
        
                _rigid.MovePosition(newPosition);
                LastDirection = direction.x > 0;
        
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
            }
    
            // 정확한 최종 위치
            _rigid.MovePosition(target);
            _rigid.velocity = Vector2.zero; // 혹시 모를 관성 제거
    
            _Animator.SetTrigger(Idle);
        }
        
        /// <summary>
        /// 목적지로 점프 연출하는 메서드
        /// </summary>
        public async UniTask CharJump(Vector3 targetPos, CancellationToken ct)
        {
            LastDirection = (targetPos - transform.position).x > 0;
            
            _Animator.SetTrigger(JumpOut);
            SpriteRenderer sr = _charUnitRoot.GetComponent<SpriteRenderer>();
            if (sr) sr.enabled = false;
            await PlayFxOnce(jumpOutFX, transform.position, ct);
            
            _Animator.SetTrigger(Idle);
            transform.position = targetPos;
            
            _Animator.SetTrigger(JumpIn);
            await UniTask.Delay(50, false, PlayerLoopTiming.Update, ct);
            sr.enabled = true;
            await PlayFxOnce(jumpInFX, transform.position, ct);
            _Animator.SetTrigger(Idle);
        }

        /// <summary>
        /// 넉백 '되는' 메서드
        /// </summary>
        /// <param name="targetPos">목적지. 플랫폼 없을 경우, rigidbody라서 알아서 떨어짐. 일단은..</param>
        /// <param name="playOnAttack">피격 애니메이션 플레이 여부</param>
        /// <param name="suppressIdle">Idle로 돌아가지 않을 것인지</param>
        public async UniTask CharKnockBack(Vector3 targetPos, bool playOnAttack=false, bool suppressIdle=false)
        {
            if(playOnAttack)
                _Animator.SetTrigger(OnAttack);
            while ((transform.position - targetPos).sqrMagnitude > 0.05f)
            {
                transform.position += (targetPos - transform.position).normalized * Time.deltaTime * moveSpeed;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            await UniTask.Delay(30);
            if(!suppressIdle)
                _Animator.SetTrigger(Idle);
        }

        public void CharPushPlay()
        {
            _Animator.SetTrigger(Push);
        }

        public async void CharPushFXPlay() => await PlayFxOnce(
            pushFX,
            transform.position + pushFX.transform.position,
            CancellationToken.None
        );
        
        public void CharReturnIdle() => _Animator.SetTrigger(Idle);
        
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
        private async UniTask PlayFxOnce(GameObject prefab, Vector3 offset, CancellationToken ct)
        {
            if (!prefab) return;
            var go = Instantiate(prefab, offset, Quaternion.identity, transform);

            // ParticleSystem이면 모두 재생 후 살아있는 동안 대기
            #region If Particle System
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
            #endregion
            
            #region If Animator
            // Animator 기반 FX면 트리거나 자동 플레이를 가정, 적당한 보호 딜레이(필요시 Animation Event로 대체)
            var anim = go.GetComponentInChildren<Animator>();
            if (anim)
                await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: ct);

            Destroy(go);
            #endregion
        }

        public async UniTask BlinkSpriteOnce()
        {
            _spriteRenderer.color = new Color(0, 0, 0, 0);
            await UniTask.Delay(50);
            _spriteRenderer.color = new Color(1, 1, 1, 1);
        }

        public void OutlineCharacter(Color outlineColor, float outlineSize)
        {
            _outlineRenderer.sprite = _spriteRenderer.sprite;
            _outlineRenderer.material.SetColor("_OutlineColor", outlineColor);
            _outlineRenderer.material.SetFloat("_OutlineSize", outlineSize);
            
            // 정렬을 기본 SR 기준으로 보정
            _outlineRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
            _outlineRenderer.sortingOrder   = _spriteRenderer.sortingOrder;

            // 스프라이트/플립 동기화 후 켜기
            _outlineRenderer.sprite = _spriteRenderer.sprite;
            _outlineRenderer.flipX  = _spriteRenderer.flipX;
            _outlineRenderer.flipY  = _spriteRenderer.flipY;
            _outlineRenderer.enabled = true;
            
        }

        public void ClearOutline()
        {
            if (_outlineRenderer)
            {
                _outlineRenderer.enabled = false;
                _outlineRenderer.sprite  = null;      // 깔끔히 끊기(선택)
            }
        }
        #endregion
        
    }
}

