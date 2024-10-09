using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Base : MonoBehaviour
{
    static Unit_Base nowSelectedUnit;
    void Start()
    {
        
    }

    private void OnMouseEnter()
    {
        if(nowSelectedUnit != null)
        {
            float distance = Vector3.Distance(nowSelectedUnit.transform.position, transform.position);
        }
        //커서 올라간 유닛 띄우는 패널에 정보를 띄워야 함.
    }

    private void OnMouseExit()
    {
        //Panel 내의 내용들을 다 비워야 함.

    }

    private void OnMouseUpAsButton()
    {
        nowSelectedUnit = this;
        //선택된 유닛 띄우는 패널에 정보를 띄워야 함.
    }
}
