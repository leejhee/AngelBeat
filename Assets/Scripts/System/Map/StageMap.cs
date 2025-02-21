using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static SystemEnum;

// 전투 시작시 생성될 맵 오브젝트
public class StageMap : MonoBehaviour
{
    //추후 기능 필요 시 작성 요함.

    private Dictionary<eCharType, List<Vector3>> _spawnDict = new();
    
    public void SpawnUnit(CharBase charBase, int squadOrder)
    {
        eCharType type = charBase.GetCharType();
        if (squadOrder >= _spawnDict[type].Count)
        {
            Debug.LogError("현재 편성 인원 기준을 맵이 수용하지 못합니다. 데이터 체크 요망");
            return;
        }
        Instantiate(charBase, _spawnDict[type][squadOrder], Quaternion.identity);
        //charmanager 등록 로직으로 변경할 것.
    }
}