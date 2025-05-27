using AngelBeat.Core;
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
        [SerializeField] private GameObject _charSnapShot;
        
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

        public long Index => _index;
        public Transform CharTransform => _charTransform;
        public Transform CharUnitRoot => _charUnitRoot;
        public GameObject SkillRoot => _SkillRoot;
        public Collider BattleCollider => _battleCollider;
        public GameObject CharCameraPos => _CharCameraPos;
        public GameObject CharSnapShot => _charSnapShot;
        public CharAnim CharAnim => _charAnim;
        public CharStat CharStat => _charStat;

        public ExecutionInfo ExecutionInfo => _executionInfo;
        public SkillInfo SkillInfo => _skillInfo;
        public KeywordInfo KeywordInfo => _keywordInfo;
        public PlayerState PlayerState => _currentState;

        // RULE : set은 반드시 초기화 시에 사용하고, 값 수정 필요 시 get으로만 할 것.
        public CharacterModel CharInfo
        {
            get => _charInfo;
            private set
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

                _charStat = value.Stat;
            }
        }
        
        protected virtual SystemEnum.eCharType CharType => _charData.defaultCharType;

        protected long _uid;
        protected CharBase() { }

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
                _charStat = new CharStat(charStat);
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

        protected virtual void CharInit(){}
        
        public void UpdateCharacterInfo(CharacterModel charInfo)
        {
            // property 기반으로 set에서 발동되도록 설정
            // TODO : 나중에 MODEL을 제외. 
            CharInfo = charInfo;
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

        public CharacterModel SaveCharModel()
        {
            return new CharacterModel(_charData, _charStat, _charInfo.Skills);
        }
        
    }
}

