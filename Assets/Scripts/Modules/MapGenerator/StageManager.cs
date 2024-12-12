using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    private MapParameterList Parameters;

    // 싱글턴...해야하나?
    int stageNum = 0;

    private Map _stageMap;
    public Map StageMap { get { return _stageMap; } }


    public void SetStage(int stageNum)
    {        
        MapParameter param = Parameters.Objects[stageNum];
        _stageMap = MapGenerator.CreateMap(param);
        _stageMap.DebugMap();
    }

    private void Start()
    {
        Parameters = GameManager.Resource.Load<MapParameterList>("ScriptableObjects/MapParameterList");
        SetStage(stageNum);
    }

    private void OnDestroy()
    {
        Resources.UnloadAsset(Parameters);
    }
    //그 외 추가적 메소드를 두자...
}