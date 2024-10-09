using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherBuildingDoor : MonoBehaviour
{
    [SerializeField] Transform warpPosition;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"{collision.gameObject.name}과 유닛 간 충돌 일어남. 입구 통해 이동.");
        collision.gameObject.GetComponent<Mover>().StopAllCoroutines();
        PathManager.Path.FinalNodeList.Clear();
        Warp(collision.gameObject);
    }

    void Warp(GameObject go)
    {             
        go.transform.position = warpPosition.position;     
    }
}
