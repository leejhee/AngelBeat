public class SystemEnum
{
    public enum UIEvent
    {
        Click,
        Drag,
    }

    public enum MouseEvent
    {
        Press,
        Click,
    }

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount
    }

    public enum eScene
    {
        None,
        
        Title,
        Lobby,
        
        LoadingScene,
        ExploreScene,
        BattleTestScene,
        
        MaxCount
    }

    #region SO 관련 데이터 enum
    /// <summary>
    /// 엑셀 데이터로 받아올 예정
    /// </summary>
    /// 
    public enum eMapNode
    {
        None,
        Location,
        Event
    }
   
    public enum eNodeType
    {
        None,
        Hospital,
        Resistance,
        Treasure,
        Inn,
        Hazard,
        Combat,

        MaxValue
    }

    public enum eMystery
    {
        None,
        //추가 예정
    }

    public enum eEdgeEvents
    {
        None,
        //추가 예정.
    }

    #endregion

    public enum eCharType
    {
        None, 

        Player,
        Enemy,
        Neutral,

        eMax
    }

    public enum eStats
    {
        None,

        STR,    // 기존 힘
        NSTR,   // 현재 힘

        AGI,    // 기존 민첩
        NAGI,   // 현재 민첩

        INT,    // 기존 지력
        NINT,   // 현재 지력

        HP, // 기본 HP
        NHP, // 현재 HP
        NMHP, // 현재 최고 HP

        /*//////////////// 아래는 스탯 종속값 ////////////////////////////////*/

        Defence, // 기본 방어력
        NDefence, // 현재 방어력

        Speed, // 기본 속도 
        NSpeed, // 현재 속도

        Cost,   // 기본 코스트 양
        NCost,  // 현재 코스트 양

        eMax
    }

    public enum eSkillType
    {
        None,

        PhysicalAttack,
        MagicAttack,
        Buff,
        Debuff,

        eMax

    }

    public enum eDungeon
    {
        None,
        
        MOUNTAIN_BACK,
        
        eMax
    }
    
    
    public enum eExecutionType
    {
        None,
        Avoidance,
        StateBuff,
        StateBuffPer,
        StateBuffNPer,
        Parrying,
        DotDamage,


        eMax
    }

    public enum eIsAttack
    {
        Player,
        Monster,

        eMax
    }

    public enum NovelCommand
    {
        NormalText,
        PersonText,
        BackGround,
        Stand,
        BGM,
        SFX,
        Choice,
        Effect,
        End
    }
}
