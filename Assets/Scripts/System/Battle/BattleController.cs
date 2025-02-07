using System;
using System.Collections.Generic;
using UnityEngine;

// BattleScene 초기화 시 작동 시작.
public class BattleController : MonoBehaviour
{
    // 상호작용 가능을 따지는 ENUM
    //private Dictionary<Type, Dictionary<long, List<CharBase>>> battleCharDict;
    private List<CharBase> _battleCharList;
    private TurnController _turnManager;
    

    private void Start()
    {
        _turnManager = new TurnController(_battleCharList);
        Init();
    }

    private void Init()
    {
        InitEnvironment();
        SetTurnOrder();
    }

    private void SetTurnOrder()
    {
        //
    }

    private void InitEnvironment()
    {

    }

    public void EndBattle()
    {

    }


}


