using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public partial class BaseMapNodeData
{
    public int index; // 노드종류Index
	public eNodeType nodeType; // 노드 타입
	public string nodeSprite; // 노드 아이콘
	public bool selectable; // 노드 선택 여부
	   
}