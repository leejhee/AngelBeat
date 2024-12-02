using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField]
    private List<MapParameter> Parameters;

    // 싱글턴...해야하나?
    int stageNum = 0;

    private Map _stageMap;
    public Map StageMap { get { return _stageMap; } }

    public void SetStage(int stageNum)
    {
        MapParameter param = Parameters[stageNum];
        _stageMap = MapGenerator.CreateMap(param);
        _stageMap.DebugMap();
    }

    private void Start()
    {
        SetStage(stageNum);
    }
    //그 외 추가적 메소드를 두자...
}