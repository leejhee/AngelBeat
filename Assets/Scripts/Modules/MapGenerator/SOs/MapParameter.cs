using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 한 스테이지에서 지도를 생성하는 데 필요한 정보. 
/// 스프레드시트 데이터에서 이 필드를 column으로 하여 스테이지별 맵을 생성하도록 한다.
/// </summary>
[CreateAssetMenu]
public class MapParameter : ScriptableObject
{
    public List<PointNodeData> availablePoints; //가능한 지점 타입
    public List<EventNodeData> availableEvents; //가능한 경로 중 이벤트 타입
    public int maxDepth;    // 최대 층 수
    public int trialNum;    // 경로 지정 횟수
    public int width;       // 층당 최대 가능한 노드 수
}