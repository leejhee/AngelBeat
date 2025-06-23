using System;

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

        BLUE,
        N_BLUE,
        RED,
        N_RED,
        YELLOW,
        N_YELLOW,
        WHITE,
        N_WHITE,
        BLACK,
        N_BLACK,
        
        HP,
        NHP,
        NMHP,
        
        ARMOR,
        NARMOR,
        MAGIC_RESIST,
        NMAGIC_RESIST,
        
        MELEE_ATTACK,
        NMELEE_ATTACK,
        MAGICAL_ATTACK,
        NMAGICAL_ATTACK,
        
        CRIT_CHANCE,
        NCRIT_CHANCE,
        
        SPEED, 
        NSPEED,
        
        ACTION_POINT,
        NACTION_POINT,
        
        DODGE,
        NDODGE,
        
        RESISTANCE,
        NRESISTANCE,
        
        RANGE_INCREASE,
        DAMAGE_INCREASE,
        ACCURACY_INCREASE,
        
        eMax
    }

    public enum eConditionCheckType
    {
        STACK_화상,
        
        STAT_NMHP,
        STAT_NHP,
        
    }

    public enum eConditionOpcode
    {
        None,
        
        BIGGER,
        SMALLER,
        EQUAL,
        NOT_EQUAL,
        GREATER_EQUAL,
        LESS_EQUAL,
        
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

    public enum ePivot
    {
        None,
        
        SELF,
        TARGET_ENEMY,
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

    public enum eKeyword
    {
        None,
        
        Burn,
        Ember,
        BurningHeart,
        MendeulMendeul,
        
        eMax
    }

    public enum eExecutionPhase
    {
        None,
        SoR,
        EoR,
        SoT,
        EoT,
        Instant,
        Always,
        eMax
    }
    
    public enum eKeywordTargetType
    {
        None,
        Self,
        AllyAll,
        EnemyAll,
        EnemyNearest,
        EnemyRandom,
        // 추가 가능
    }

    public enum eInfluenceType
    {
        None,
        
        Negative,
        Positive,
        Neutral,
        
        eMax
    }
    
    public enum eIsAttack
    {
        Player,
        Monster,

        eMax
    }
    
    [Flags]
    public enum eSkillUnlock
    {
        EUINYEO_SKILL_1,
        EUINYEO_SKILL_2,
        EUINYEO_SKILL_3,
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
