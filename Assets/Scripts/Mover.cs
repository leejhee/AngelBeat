using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] GameObject pathFinder;
    [SerializeField] float speed;
    private PathManager path;
    private Animator anim;
    private Coroutine moveCoroutine;

    void Start()
    {
        path = pathFinder.GetComponent<PathManager>();
        anim = GetComponent<Animator>();
        GameManager.Input.KeyAction -= OnClickMouse;
        GameManager.Input.KeyAction += OnClickMouse;
    }

    public void OnClickMouse()
    {
        if (Input.GetMouseButtonDown(1))
        {
            anim.SetBool("doMove", true);
            path.startPos = Vector2Int.RoundToInt(transform.position);
            Vector2 clickedPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            path.targetPos = Vector2Int.RoundToInt(clickedPos);
            path.PathFinding();

            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(nameof(MoveObject));
        }
    }

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

        anim.SetBool("doMove", false);
    }
}
