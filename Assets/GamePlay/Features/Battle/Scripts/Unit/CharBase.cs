using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.Utils;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Contracts;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Common.Scripts.Entities.Character.Components;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Common.Scripts.Keyword;
using GamePlay.Common.Scripts.Skill;
using GamePlay.Features.Battle.Scripts.UI.BattleHovering;
using GamePlay.Features.Battle.Scripts.Unit.Components;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GamePlay.Features.Battle.Scripts.Unit
{
    public abstract class CharBase : MonoBehaviour, IDamageable
    {
        #region Member Field
        [SerializeField] private long _index;
        [SerializeField] private GameObject _SkillRoot;
        [SerializeField] protected Animator _Animator;
        [SerializeField] private BoxCollider2D _battleCollider;
        [SerializeField] private Transform charCameraPos;
        [SerializeField] private GameObject _charSnapShot;
        [SerializeField] private Rigidbody2D _rigid;
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private Transform _charUnitRoot;
        [SerializeField] private CharacterHpBar _hpBar;
        [SerializeField] private GameObject protectionIndicator;
        [SerializeField] private GameObject tauntIndicator;
        [SerializeField] private CharAnimDriver anim;
        
        //TODO : 점프 효과 관련 프리팹을 어떻게 관리할지 생각할 것
        [SerializeField] private GameObject jumpOutFX;
        [SerializeField] private GameObject jumpInFX;
        [SerializeField] private GameObject pushFX;
        
        private SpriteRenderer  _spriteRenderer;
        private SpriteRenderer  _outlineRenderer;
        private Transform       _charTransform;
        
        private CharacterModel  _charInfo;
        private CharStat        _runtimeStat;
        private SkillInfo       _skillInfo;
        private KeywordInfo     _keywordInfo;
        
        private bool _isAction = false;    // 행동중인가? 판별
        protected long _uid;

        private float _movePoint;
        private float _currentMovePoint;
        
        private Camera _mainCamera;

        private FieldCover _coverage; // 등록된 엄폐물
        #endregion
        
        #region Properties
        public long Index => _index;
        public string UnitName;
        public Transform CharTransform => _charTransform;
        public Transform CharUnitRoot => _charUnitRoot;
        public GameObject SkillRoot => _SkillRoot;
        public BoxCollider2D BattleCollider => _battleCollider;
        public Transform CharCameraPos => charCameraPos;
        public GameObject CharSnapShot => _charSnapShot;
        public Animator Animator => _Animator;
        public CharAnimDriver Anim => anim;
        public Rigidbody2D Rigid => _rigid;
        public Camera MainCamera => _mainCamera;
        
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
            //_charAnim = new();
            anim = GetComponent<CharAnimDriver>();
            _mainCamera = Camera.main;
            _battleCollider.enabled = false;
        }
        

        public virtual async UniTask CharInit(CharacterModel charModel)
        {
            _charInfo = charModel; //모델
            _runtimeStat = charModel.BaseStat; // 스탯 복사
            _hpBar.SetFillAmount(MaxHP, MaxHP);
            
            //스킬 초기화 - 이미 ActiveSkills로 저장해놓은 애들만 뽑아줌.
            IReadOnlyList<SkillModel> skillModels = charModel.UsingSkills;
            _skillInfo = new SkillInfo(this);
            await _skillInfo.InitAsync(skillModels);

            _battleCollider.enabled = true;
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

        public async UniTask DamageAsync(long finalDamage, CancellationToken ct)
        {
            _runtimeStat.ReceiveDamage(finalDamage);
            Debug.Log($"{finalDamage}데미지");
            Debug.Log($"{CurrentHP} / {MaxHP}");
            GameObject damageText =
                await BattleFXManager.Instance.PlayFX(FX.DamageFX, transform, new Vector2(1, 1), ct);
            IngameDamageObject txt = damageText.GetComponent<IngameDamageObject>();
            txt.Init(finalDamage);
            if (_hpBar)
            {
                _hpBar.SetFillAmount(CurrentHP, MaxHP);
            }
        }
        
        /// <summary>
        /// 대미지 관련 파라미터로 이벤트 발생시킴.
        /// </summary>
        /// <param name="damageInfo">기존 대미지 파라미터</param>
        /// <param name="playOnAttack">피격 모션 재생할까?</param>
        /// <param name="suppressIdle">Idle suppression</param>
        public async UniTask SkillDamage(
            DamageParameter damageInfo, 
            bool playOnAttack=true, 
            bool suppressIdle=false)
        {
            CharBase attacker = damageInfo.Attacker;
            SkillModel model = damageInfo.Model;
            
            Debug.Log($"{attacker.name}의 공격이 {name}에게 적중했습니다.");
            attacker.OnAttackSuccess?.Invoke(damageInfo);
            
            // Calculation
            float attackStat = attacker.RuntimeStat.GetAttackStat(model.SkillType);
            float defenseStat = _runtimeStat.GetDefenseStat(model.SkillType);
            SkillDamageData damageData = model.SkillDamage;
            
            long finalDamage = model.SkillType switch
            {
                SystemEnum.eSkillType.Heal => -(long)Mathf.Ceil(
                    damageData.DamageCoefficient *
                    Random.Range(damageData.RandMin, damageData.RandMax + 1)),
                SystemEnum.eSkillType.Debuff => (long)Mathf.Ceil(
                    damageData.DamageCoefficient *
                    (1 + attacker.RuntimeStat.GetStat(SystemEnum.eStats.DAMAGE_INCREASE) / 100f)),
                _ => (long)Mathf.Ceil(
                    attackStat *
                    damageData.DamageCoefficient *
                    Random.Range(damageData.RandMin, damageData.RandMax + 1) *
                    (1 + attacker.RuntimeStat.GetStat(SystemEnum.eStats.DAMAGE_INCREASE) / 100f) *
                    (100 - defenseStat) / 100f)
            };
            
            bool canBeBlocked = model.SkillType == SystemEnum.eSkillType.PhysicalAttack && finalDamage > 0;
            
            async UniTask Apply(CancellationToken t)
            {
                if (_coverage && canBeBlocked)
                    await _coverage.DamageAsync(finalDamage, t);
                else
                    await DamageAsync(finalDamage, t);
            
                OnHit?.Invoke(damageInfo);
                await UniTask.Delay(60, cancellationToken: t); // 히트스톱/여유
            }
            
            if (playOnAttack) await anim.WithOnAttack(Apply);
            else              await Apply(CancellationToken.None);
        }

        /// <summary>
        /// 명중 계산 -> 성공하면 이벤트 발행하고 연출
        /// </summary>
        /// <returns>피했나? 안피했나?</returns>
        public async UniTask<bool> TryEvade(DamageParameter damageInfo)
        {
            SkillModel model = damageInfo.Model;
            if (model.SkillType == SystemEnum.eSkillType.MagicAttack) return false;

            float hitChance = model.SkillAccuracy + damageInfo.Attacker.BonusAccuracy - Dodge + 5;
            bool succeed = UnityEngine.Random.Range(0f, 100f) > Mathf.Clamp(hitChance, 0, 100);
            if (!succeed) return false;

            Debug.Log($"{damageInfo.Attacker.name}의 공격을 {name}이 회피했습니다.");

            OnMiss?.Invoke(damageInfo);
            await anim.WithEvade(async t =>
            {
                await UniTask.Delay(60, cancellationToken: t);
            });
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
        
        #region Character movement Control
        
        private bool _lastDirectionRight; // true 오른쪽, false 왼쪽
        
        public bool LastDirectionRight
        {
            get => _lastDirectionRight;
            set
            {
                _lastDirectionRight = value;
                if(CharType == SystemEnum.eCharType.Player)
                    _spriteRenderer.flipX = !LastDirectionRight;
                else
                    _spriteRenderer.flipX = LastDirectionRight;
            }
        }
        
        private void SetCharacterToward(Vector3 direction)
        {
            LastDirectionRight = direction.x > 0;
        }
        
        private void SetCharacterToward(Vector3 originPos, Vector3 targetPos)
        {
            SetCharacterToward(targetPos - originPos);
        }
        
        /// <summary>
        /// 캐릭터 이동 연출 메서드
        /// 외부에서 이동력을 조사하므로, 이동력을 초과하지 않음을 전제로 함.
        /// </summary>
        public async UniTask CharMove(Vector3 targetPos)
        {
            await anim.WithMoving(async _ =>
            {
                Vector2 target = targetPos;
                const float eps = 0.05f;

                while (Vector2.Distance(_rigid.position, target) > eps)
                {
                    Vector2 dir = (target - _rigid.position).normalized;
                    Vector2 next = _rigid.position + dir * (moveSpeed * Time.fixedDeltaTime);

                    _rigid.MovePosition(next);
                    SetCharacterToward(dir);

                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
                }

                _rigid.MovePosition(target);
                _rigid.velocity = Vector2.zero;
            });
        }
        
        /// <summary>
        /// 목적지로 점프 연출하는 메서드
        /// </summary>
        public async UniTask CharJump(Vector3 targetPos, CancellationToken ct)
        {
            SetCharacterToward(transform.position, targetPos);
            
            // JumpOut 연출 (FX는 기다리지 않음)
            _ = PlayFxOnce(jumpOutFX, transform.position, ct, maxSeconds: 1f, detachFromCharacter: true, wait: false);
            await anim.WithJumpOut(async t =>
            {
                await UniTask.Delay(1000, cancellationToken: t); // 살짝 텐션
            }, ct);
            
            var sr = _charUnitRoot.GetComponent<SpriteRenderer>();
            var outlineSR = _charUnitRoot.GetChild(0).GetComponent<SpriteRenderer>();
            if(outlineSR) outlineSR.enabled = false;
            if (sr) sr.enabled = false;
            
            await UniTask.Delay(250, cancellationToken: ct);
            transform.position = targetPos;
            if (sr) sr.enabled = true;
            if(outlineSR) outlineSR.enabled = true;
            
            _ = PlayFxOnce(jumpInFX, transform.position, ct, maxSeconds: 1f, detachFromCharacter: true, wait: false);
            await anim.WithJumpIn(async t =>
            {
                await UniTask.Delay(1000, cancellationToken: t);
            }, ct);
            
        }

        /// <summary>
        /// 넉백 '되는' 메서드
        /// </summary>
        /// <param name="targetPos">목적지</param>
        /// <param name="playOnAttack">피격 애니메이션 플레이 여부</param>
        /// <param name="suppressIdle">Idle로 돌아가지 않을 것인지</param>
        public async UniTask CharKnockBack(Vector3 targetPos, bool playOnAttack = false, bool suppressIdle = false)
        {
            async UniTask Motion(CancellationToken t)
            {
                Vector3 displacement = targetPos - transform.position;
                SetCharacterToward(-displacement);
                while (displacement.sqrMagnitude > 0.05f)
                {
                    Vector3 delta = displacement.normalized * (Time.deltaTime * moveSpeed);
                    transform.position += delta;
                    displacement -= delta;
                    await UniTask.Yield(PlayerLoopTiming.Update, t);
                }
                await UniTask.Delay(30, cancellationToken: t);
            }

            if (playOnAttack) await anim.WithOnAttack(Motion);
            else              await Motion(CancellationToken.None);

        }

        public void CharPushPlay(Vector3 lookAtPos)
        {
            SetCharacterToward(transform.position, lookAtPos);
            anim.SetPush(true);
        }
        public void CharPushEnd() => anim.SetPush(false);

        public void CharReturnIdle() => anim.ResetAllFlags();

        #region Coverage Logic
        public void RegisterCoverage(FieldCover cover)
        {
            _coverage = cover;
            protectionIndicator.SetActive(true);
        }

        public void ExitCoverage()
        {
            _coverage = null;
            protectionIndicator.SetActive(false);
        }
        
        #endregion
        #endregion
        
        #region Unity Events
        public event Action OnUpdate;
        private void Update()
        {
            OnUpdate?.Invoke();
        }
        
        #endregion
        
        #region Util로 옮길 부분
        
        //TODO : Particlesystem인지 animator인지 결정되면 알아서 최적화할것
        private async UniTask PlayFxOnce(
            GameObject prefab,
            Vector3 worldPos,
            CancellationToken ct,
            float maxSeconds = 0.8f,
            bool detachFromCharacter = true,
            bool wait = false)
        {
            if (!prefab) return;

            Transform parent = detachFromCharacter ? null : transform;
            var go = Instantiate(prefab, worldPos, Quaternion.identity, parent);

            // 1) 파티클
            var ps = go.GetComponentsInChildren<ParticleSystem>(true);
            if (ps.Length > 0)
            {
                foreach (var p in ps)
                    if (p)
                        p.Play();

                if (wait)
                {
                    float t = 0f;
                    while (!ct.IsCancellationRequested &&
                           Array.Exists(ps, p => p && p.IsAlive(true)) &&
                           t < maxSeconds)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, ct);
                        t += Time.deltaTime;
                    }

                    if (go) Destroy(go);
                }
                else
                {
                    UniTask.Void(async () =>
                    {
                        try
                        {
                            float t = 0f;
                            while (t < maxSeconds && Array.Exists(ps, p => p && p.IsAlive(true)))
                            {
                                await UniTask.Yield();
                                t += Time.deltaTime;
                            }
                        }
                        finally
                        {
                            if (go) Destroy(go);
                        }
                    });
                }

                return;
            }

            // 2) 애니메이터 FX
            var fxAnim = go.GetComponentInChildren<Animator>();
            float clipLen = 0.5f;
            if (fxAnim && fxAnim.runtimeAnimatorController)
            {
                var clips = fxAnim.runtimeAnimatorController.animationClips;
                for (int i = 0; i < clips.Length; i++)
                    clipLen = Mathf.Max(clipLen, clips[i].length);
            }

            clipLen = Mathf.Min(clipLen, maxSeconds);

            if (wait)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(clipLen), cancellationToken: ct);
                if (go) Destroy(go);
            }
            else
            {
                UniTask.Void(async () =>
                {
                    try { await UniTask.Delay(TimeSpan.FromSeconds(clipLen), cancellationToken: ct); }
                    finally
                    {
                        if (go) Destroy(go);
                    }
                });
            }
        }

        public async UniTask BlinkSpriteOnce()
        {
            _spriteRenderer.color = new Color(0, 0, 0, 0);
            await UniTask.Delay(50);
            _spriteRenderer.color = new Color(1, 1, 1, 1);
        }

        public void OutlineCharacter(Color outlineColor, float outlineSize)
        {
            if (!_spriteRenderer || !_spriteRenderer.enabled)
            {
                if (_outlineRenderer) _outlineRenderer.enabled = false;
                return;
            }
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

