using AngelBeat.Core.Character;
using AngelBeat.Core.SingletonObjects.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AngelBeat
{
    public abstract class CharBase : MonoBehaviour
    {
        [SerializeField] private long _index;
        [SerializeField] private GameObject _SkillRoot;
        [SerializeField] protected Animator _Animator;
        [SerializeField] private Collider _battleCollider;
        [SerializeField] private GameObject _CharCameraPos;

        private CharacterInfo _charInfo;
        
        private Transform       _charTransform;
        private Transform       _charUnitRoot;

        private CharData        _charData;  
        private StackInfo       _stackInfo;
        private CharStat        _charStat;
        private CharAnim        _charAnim = null;
        
        private ExecutionInfo   _executionInfo;
        private SkillInfo       _skillInfo;

        private PlayerState _currentState; // 현재 상태
        private bool _isAction = false;    // 행동중인가? 판별
        private Dictionary<PlayerState, int> _indexPair = new();

        public long Index => _index;
        public Transform CharTransform => _charTransform;
        public Transform CharUnitRoot => _charUnitRoot;
        public GameObject SkillRoot => _SkillRoot;
        public Collider BattleCollider => _battleCollider;
        public GameObject CharCameraPos => _CharCameraPos;
        public CharAnim CharAnim => _charAnim;
        public CharStat CharStat => _charStat;

        public ExecutionInfo ExecutionInfo => _executionInfo;
        public SkillInfo SkillInfo => _skillInfo;
        public StackInfo StackInfo => _stackInfo;
        public PlayerState PlayerState => _currentState;

        protected virtual SystemEnum.eCharType CharType => _charData.defaultCharType;

        protected long _uid;



        protected CharBase() { }

        private void Awake()
        {
            _charTransform = transform;
            _charUnitRoot = Util.FindChild<Transform>(gameObject, "UnitRoot");
            _uid = BattleCharManager.Instance.GetNextID();

            _charData = DataManager.Instance.GetData<CharData>(_index);
            _charAnim = new();

            if (_charData != null)
            {
                CharStatData charStat = DataManager.Instance.GetData<CharStatData>(_charData.charStat);
                if (charStat == null)
                {
                    Debug.LogError($"캐릭터 ID : {_index} 데이터 Get 성공 charStat {_charData.charStat} 데이터 Get 실패");
                }
                _charStat = new CharStat(charStat);
            }
            else
            {
                Debug.LogError($"캐릭터 ID : {_index} Data 데이터 Get 실패");
            }
        }

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
            // 스킬
            _skillInfo = new SkillInfo(this);
            _skillInfo?.Init(_charData.charSkillList);


        }

        public void UpdateCharacterInfo(CharacterModel charInfo)
        {
            
        }
        
        public virtual void CharDistroy()
        {
            Type myType = this.GetType();
            BattleCharManager.Instance.Clear(myType, _uid);
            Destroy(gameObject);
        }

        public long GetID() => _uid;

        public SystemEnum.eCharType GetCharType()
        {
            return CharType;
        }

        public void SetStateAnimationIndex(PlayerState state, int index = 0)
        {
            _indexPair[state] = index;
        }
        public void PlayStateAnimation(PlayerState state)
        {
            _charAnim.PlayAnimation(state);
        }

    }
}

