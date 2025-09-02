using Character;
using Core.Attributes;
using Core.Scripts.Foundation.Define;
using GamePlay.Character;
using GamePlay.Entities.Scripts.Character;
using GamePlay.Features.Scripts.Battle;
using GamePlay.Features.Scripts.Battle.Unit;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Core.Scripts.Foundation.Define.SystemEnum;

namespace AngelBeat
{
    // 전투 시작시 생성될 맵 오브젝트
[RequireComponent(typeof(Grid)), System.Serializable]
public class StageField : MonoBehaviour
{
    private Dictionary<eCharType, List<SpawnData>> _spawnDict = new();
    

    [SerializeReference, CustomDisable] // 데이터 클래스에서 바로 파싱할 수 있도록 그냥 큰 단위 하나를 만듬
    private BattleFieldSpawnInfo battleSpawnerData = new();

    public Transform ObjectRoot { 
        get
        {
            GameObject root = GameObject.Find("ObjectRoot");
            if(root == null)
            {
                root = new GameObject("ObjectRoot");
                root.transform.SetParent(transform);
            }
            return root.transform;
        } }

    private void Awake()
    {
        Debug.Log("Initializing Spawn Data...");
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
            GeneratePos = _spawnDict[type][squadOrder].SpawnPosition,
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
                GeneratePos = _spawnDict[type][i].SpawnPosition,
                CharIndex = characters[i].Index
            });
        }
    }
    
    /// <summary>
    /// 스테이지에서 플레이어 파티와 적 캐릭터들을 인스턴스화하고, 그 전체 목록을 반환.
    /// </summary>
    /// <param name="playerParty"> 플레이어 측의 파티 </param>
    /// <returns> 스폰된 모든 캐릭터를 반환합니다. </returns>
    public List<CharBase> SpawnAllUnits(Party playerParty)
    {
        Debug.Log("Spawning All Units...");
        List<CharBase> battleMembers = new();
        
        //PlayerSide
        List<CharacterModel> partyInfo = playerParty.partyMembers;
        for (int i = 0; i < partyInfo.Count; i++)
        {
            CharacterModel character = partyInfo[i];
            SpawnData data = _spawnDict[eCharType.Player][i];
            CharBase battlePrefab = BattleCharManager.Instance.CharGenerate(new CharParameter
            {
                CharIndex = character.Index,
                GeneratePos = data.SpawnPosition,
                Scene = eScene.BattleTestScene
            });
            battlePrefab.UpdateCharacterInfo(character);
            battleMembers.Add(battlePrefab);
        }

        //EnemySide
        foreach (var spawnData in _spawnDict[eCharType.Enemy])
        {
            CharBase battlePrefab = BattleCharManager.Instance.CharGenerate(new CharParameter()
            {
                CharIndex = spawnData.SpawnCharacterIndex,
                GeneratePos = spawnData.SpawnPosition,
                Scene = eScene.BattleTestScene
            });
            battleMembers.Add(battlePrefab);
        }

        return battleMembers;
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





}
