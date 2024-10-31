using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] float speed;
    private Animator anim;
    private Coroutine moveCoroutine;
    AStarGrid testBG;

    void Start()
    {
        testBG = new AStarGrid(100, 100);
        anim = GetComponent<Animator>();
        GameManager.Input.KeyAction -= OnClickMouse;
        GameManager.Input.KeyAction += OnClickMouse;
    }

    public void OnClickMouse()
    {
        if (Input.GetMouseButtonDown(1))
        {
            anim.SetBool("doMove", true);

            Vector2Int startPos = Vector2Int.RoundToInt(transform.position);
            Vector2 clickedPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int targetPos = Vector2Int.RoundToInt(clickedPos);

            AStarParameter param = GetAStarParam(startPos, targetPos);
            var pathPoints = AStarPathFinder.AStarPath(param);
            var pathVector = PointConverter.ToVector2Int(pathPoints);

            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(nameof(MoveObject), pathVector);
        }
    }

    private AStarParameter GetAStarParam(Vector2Int startPos, Vector2Int endPos)
    {
        Node startNode = new Node(startPos.x, startPos.y);
        Node endNode = new Node(endPos.x, endPos.y);
        AStarParameter aStar = 
            new AStarParameter(testBG, startNode, endNode, DiagonalPermit.Always);
        return aStar;
    }

    private IEnumerator MoveObject(List<Vector2Int> path)
    {
        foreach(var node in path)
        {
            bool arrived = false;
            while (!arrived)
            {
                transform.position += speed * Time.deltaTime * 
                    (new Vector3(node.x, node.y, 0) - new Vector3(transform.position.x, transform.position.y, 0)).normalized;

                if(Vector3.Distance(transform.position, new Vector3(node.x, node.y, 0))<0.1f)
                    arrived = true;

              yield return new WaitForFixedUpdate();
            }           
        }

        anim.SetBool("doMove", false);
    }
}
