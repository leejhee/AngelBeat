using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public partial class MapParameter
{
    public int index; // 맵종류Index
	public string mapName; // 맵이름
	public int maxDepth; // 최대 스테이지
	public int trialNum; // 경로 지정 횟수
	public int width; // 가로길이
	public List<int> availablePoints; // 가능한 지점
	public List<int> avaliableEvents; // 가능한 경로
	   
}