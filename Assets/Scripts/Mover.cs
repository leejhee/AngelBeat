using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] GameObject pathFinder;
    [SerializeField] float speed;
    private PathManager path;
    private Coroutine moveCoroutine;

    void Start()
    {
        path = pathFinder.GetComponent<PathManager>();
        GameManager.Input.KeyAction -= OnClickMouse;
        GameManager.Input.KeyAction += OnClickMouse;
    }

    public void OnClickMouse()
    {
        if (Input.GetMouseButtonDown(1))
        {
            PlayerController.Player.SetPlayerMove(true);
            path.startPos = Vector2Int.RoundToInt(transform.position);
            Vector2 clickedPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            path.targetPos = Vector2Int.RoundToInt(clickedPos);
            path.PathFinding();

            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(nameof(MoveObject));
        }
    }


    /// <summary>
    /// A* 알고리즘을 통해 도출해낸 경유 지점을 플레이어가 등속으로 통과하며 목표 지점까지 이동하게 함.
    /// 벡터3를 이용하니까 물리 처리라고 생각하여 WaitForFixedUpdate() 사용.
    /// 도착 판정의 경우 0.1로 해도 문제없겠다 판단됨. 엄격히 할 경우 노드 경유 시 버벅임 현상 발생.
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveObject()
    {
        foreach(var node in path.FinalNodeList)
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

        PlayerController.Player.SetPlayerMove(false);
    }
}
