
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

    public enum eMapNode
    {
        None,
        Location,
        Event
    }

    /// <summary>
    /// 엑셀 데이터로 받아올 예정
    /// </summary>
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

    public enum eCharType
    {
        Player,
        Enemy,
    }

    public enum eState
    {
        None,

        STR,
        AGI,
        INT,

        HP, // 기본 HP
        NHP, // 현재 HP
        NMHP, // 현재 최고 HP

        Defence, // 기본 방어력
        NDefence, // 현재 방어력

        SP, // 기본 SP
        NSP, // 현재 SP
        NMSP, // 현재 최고 SP

        Speed, // 기본 스피드 
        NSpeed, // 현재 Speed

        Attack, // 공격력
        NAttack, // 현재 공격력

        MaxCount
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
