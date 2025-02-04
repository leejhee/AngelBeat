using System;
using System.Collections.Generic;
using UnityEngine;

public class Battle
{
    // 상호작용 가능을 따지는 ENUM
    
    private TurnCalculator _turnManager;

    public Battle(BattleParameter param)
    {
        _turnManager = new TurnCalculator();
    }

    public void SetTurnOrder()
    {

    }

}

public class TurnCalculator
{   
    private List<CharBase> Participants;
    public TurnCalculator()
    {

    }

    public void InitCalculator()
    {

    }
}

public class Turn
{
    public enum Side
    {
        None,
        Player,
        Enemy,
        Neutral,
        SideMax
    }
    
    private CharBase _turnOwner = null;

    public CharBase TurnOwner => _turnOwner;
    public Side WhoseSide => Side.None;


    public Turn(CharBase TurnOwner)
    {
        _turnOwner = TurnOwner;

    }
}

public class TurnComparer : IComparer<Turn> 
{ 
    private Func<Turn, Turn, int>[] comparisonFunctions; 
    public TurnComparer(params Func<Turn, Turn, int>[] comparisonFunctions) 
    { 
        this.comparisonFunctions = comparisonFunctions; 
    } 
    public int Compare(Turn x, Turn y) 
    { 
        foreach (var func in comparisonFunctions) 
        { 
            int result = func(x, y); 
            if (result != 0) 
                return result; 
        } 
        return 0; 
    } 
}

public static class TurnComparisonMethods
{

}