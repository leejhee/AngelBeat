using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Tilemaps;
using static SystemEnum;

// 전투 시작시 생성될 맵 오브젝트
[RequireComponent(typeof(Grid))]
public class StageField : MonoBehaviour
{
    private Dictionary<eCharType, List<Vector3>> _spawnDict = new();

    [SerializeReference, CustomDisable] // 데이터 클래스에서 바로 파싱할 수 있도록 그냥 큰 단위 하나를 만듬
    private BattleFieldSpawnInfo battleSpawnerData = new();

    private void Start()
    {
        _spawnDict = battleSpawnerData.Convert2Dict();
    }

    // 이거는 CharManager에서 해줘야 하는 일이다.
    public void SpawnUnit(CharBase charBase, int squadOrder)
    {
        eCharType type = charBase.GetCharType();
        if (squadOrder >= _spawnDict[type].Count)
        {
            Debug.LogError("현재 편성 인원 기준을 맵이 수용하지 못합니다. 데이터 체크 요망");
            return;
        }
        BattleCharManager.Instance.CharGenerate(new CharParameter()
        {
            Scene = eScene.BattleTestScene,
            GeneratePos = _spawnDict[type][squadOrder],
            CharIndex = charBase.Index
        });
    }

    public void SpawnAllUnitsByType(eCharType type, List<CharBase> characters)
    {
        if (characters.Count > _spawnDict[type].Count)
        {
            Debug.LogError("현재 편성 인원 기준을 맵이 수용하지 못합니다. 데이터 체크 요망");
            return;
        }
        for (int i = 0; i < characters.Count; i++)
        {
            BattleCharManager.Instance.CharGenerate(new CharParameter()
            {
                Scene = eScene.BattleTestScene,
                GeneratePos = _spawnDict[type][i],
                CharIndex = characters[i].Index
            });
        }
    }



    #region 에디터 툴용
#if UNITY_EDITOR
    public BattleFieldSpawnInfo LoadSpawnerOnlyInEditor()
    {
        return battleSpawnerData;
    }
#endif
    #endregion


}




