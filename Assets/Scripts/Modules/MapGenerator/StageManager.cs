using UnityEngine;

/// <summary>
/// 탐험 단계에서 스테이지를 관리하는 역할
/// </summary>
public class StageManager : SingletonObject<StageManager>
{
    private MapParameterList _parameters;

    private int stageNum = 0;

    private Map _stageMap;
    public Map StageMap { get { return _stageMap; } }

    public override void Init()
    {
        _parameters = GameManager.Resource.Load<MapParameterList>("ScriptableObjects/MapParameterList");
    }

    /// <summary>
    /// 탐험 시작 시 호출
    /// </summary>
    /// <param name="stageNum"></param>
    public void SetStage(int stageNum, bool isFirst = false)
    {
        if (isFirst) 
        { 
            MapParameter param = _parameters.Objects[stageNum];
            _stageMap = MapGenerator.CreateMap(param);
            _stageMap.DebugMap();
        }
        else
        {
            //불러오기...
        }
    }

    /// <summary> 노드 이벤트 진행 시 호출 </summary>
    public void ProceedStage()
    {

    }
    
    /// <summary>
    /// 탐험 이탈 시 호출
    /// </summary>
    public void ExitStage()
    {

    }
}