using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharBase : MonoBehaviour
{
    [SerializeField] private long _index;
    [SerializeField] private GameObject _SkillRoot;
    [SerializeField] protected Animator _Animator;

    private Transform       _charTransform;

    private CharData        _charData;
    private ExecutionInfo   _executionInfo;
    private SkillInfo       _skillInfo;
    private StackInfo       _stackInfo;

    public ExecutionInfo ExecutionInfo => _executionInfo;
    public SkillInfo SkillInfo => _skillInfo;
    public StackInfo StackInfo => _stackInfo;
    public SystemEnum.eCharType CharType => _charData.defaultCharType;

    private void Awake()
    {
        _charTransform = transform;
        _charData = DataManager.Instance.GetData<CharData>(_index);
        
        // 스탯 필수 초기화 사항
    }

    private void Start()
    {
        CharInit();
    }

    protected virtual void CharInit()
    {
        //CharManager.Instance.SetChar<CharBase>(this);

        // 스킬
        _skillInfo = new SkillInfo(this);
        _skillInfo?.Init(_charData.charSkillList);


    }

}
