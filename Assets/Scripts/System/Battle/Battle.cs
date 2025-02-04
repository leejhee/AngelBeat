using UnityEngine;

public class BattleParameter
{

}


// 싱글턴 안쓰고 매번 전투 인스턴스 생성하는 게 좋겠다.
// 메모리 관리 용도.
public class Battle
{


    /// <summary>
    /// 내부 클래스로 턴을 둔다.
    /// </summary>
    public class Turn
    {

    }

    public Battle(BattleParameter param)
    {

    }

}