using System.Text;
using UnityEngine;
public class SystemConst
{
    public const float fps = 60f;
}

public class SystemString
{
    
    public const string SkillIconPath = "Sprites/SkillIcon/";
    public const string KeywordIconPath = "Sprites/KeywordIcon/";
    public const string PlayerHitCollider = "PlayerHitCollider";
    public const string MonsterHitCollider = "MonsterHitCollider";
    public const string MapConfigDBPath = "ScriptableObjects/ExploreMapConfigs/ConfigDB";

    public const string GlobalSaveDataPath = "SaveData";
    public const string SlotPrefix = "GameSlot_";
    
    public static string GetSlotName(int slotIndex)
    {
        return new StringBuilder(SlotPrefix).Append(slotIndex.ToString()).ToString();
    } 
}