using System;
using System.Collections.Generic;
using UnityEngine;

// BattleScene 초기화 시 작동 시작.
public class BattleController : MonoBehaviour
{
    // 상호작용 가능을 따지는 ENUM

    private TurnController _turnManager;

    private void Start()
    {
        _turnManager = new TurnController();
        Init();
    }

    private void Init()
    {
        InitEnvironment();
        SetTurnOrder();
    }

    private void SetTurnOrder()
    {
        _turnManager.InitController();
    }

    private void InitEnvironment()
    {

    }

    public void EndBattle()
    {

    }


}


