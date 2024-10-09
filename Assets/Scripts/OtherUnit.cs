using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타 유닛 간 충돌 시의 이벤트
/// </summary>
public class OtherUnit : Unit_Base
{
    Collider2D col;
    void Start()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("유닛 충돌 발생.");
    }
}
