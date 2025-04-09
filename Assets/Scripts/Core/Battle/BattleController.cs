using System;
using System.Collections.Generic;
using UnityEngine;

// BattleScene 초기화 시 작동 시작.
public class BattleController : MonoBehaviour
{
    private List<CharBase> _battleCharList;
    private TurnController _turnManager;
    

    private void Start()
    {
        _turnManager = new TurnController();
        Init();
    }

    private void Init()
    {
        InitEnvironment();
    }

    private void InitEnvironment()
    {
        //일단 맵을 로드해야한다.
    }

    public void EndBattle()
    {

    }


}


