using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] GameObject pathFinder;
    [SerializeField] float speed;
    private PathManager path;


    void Start()
    {
        path = pathFinder.GetComponent<PathManager>();
    }

    void Update()
    {       
        if (Input.GetMouseButtonDown(1))
        {
            path.startPos = Vector2Int.RoundToInt(transform.position);
            Vector2 clickedPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            path.targetPos = Vector2Int.RoundToInt(clickedPos);
            path.PathFinding();
            MoveObject();
        }
    }

    void MoveObject()
    {
        foreach(var node in path.FinalNodeList)
        {
            transform.position = new Vector2(node.x, node.y);
        }
    }
}
