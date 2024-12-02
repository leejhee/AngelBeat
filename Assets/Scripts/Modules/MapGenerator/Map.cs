using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Map : MonoBehaviour
{
    private List<MapFloor> _mapNodes;
    private List<MapPath> _mapPaths;
    public List<MapFloor> MapNodes { get { return _mapNodes; } }
    public List<MapPath> MapPaths { get { return _mapPaths; } }

    public Map()
    {
        _mapNodes = new List<MapFloor>();
        _mapPaths = new List<MapPath>();
    }

    public Map(List<MapFloor> mapNodes, List<MapPath> mapPaths)
    {
        _mapNodes = mapNodes;
        _mapPaths = mapPaths;
    }

    #region Add or Delete
    public void AddNode(int floor, MapNode target)
    {
        _mapNodes[floor].AddNode(target);
    }

    public void DeleteNode(int floor, MapNode target)
    {
        _mapNodes[floor].RemoveNode(target);
    }

    public void AddPath(MapPath path)
    {
        if (!_mapPaths.Contains(path))
            _mapPaths.Add(path);
    }

    public void DeletePath(MapPath path)
    {
        if (_mapPaths.Contains(path))
            _mapPaths.Remove(path);
    }
    #endregion

    public void DebugMap()
    {
        var debugMap = new StringBuilder("생성된 맵입니다. 생성 일시 : " + DateTime.Now.ToString("MM/dd/yyyy") + '\n');
        _mapNodes.Reverse();
        foreach(var mapfloor in _mapNodes)
        {
            var debugFloor = new StringBuilder(mapfloor.FloorNum.ToString() + '\t');
            int xInterval = 0;
            
            foreach(var node in mapfloor.FloorMembers)
            {
                xInterval = node.GridPoint.x - xInterval;
                if(xInterval == 0) continue;
                debugFloor.Append(xInterval * '\t' + node.NodeID.ToString());
            }            
            debugMap.AppendLine(debugFloor.ToString());
        }

        Debug.Log(debugMap.ToString());
    }

    public void ClearMap()
    {
        _mapNodes.Clear();
        _mapPaths.Clear();
    }
}