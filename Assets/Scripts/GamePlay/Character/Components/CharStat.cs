using System;
using UnityEngine;
using static SystemEnum;
// ReSharper disable All

namespace AngelBeat
{
    [Serializable]
    public class CharStat
    {
        private long[] _charStat = new long[(int)eStats.eMax];
        public CharStat(CharStatData charStat)
        {
            #region Init Stat Array
            _charStat[(int)eStats.BLUE] = charStat.blue;
            _charStat[(int)eStats.N_BLUE] = charStat.blue;
            
            _charStat[(int)eStats.RED] = charStat.red;
            _charStat[(int)eStats.N_RED] = charStat.red;
            
            _charStat[(int)eStats.YELLOW] = charStat.yellow;
            _charStat[(int)eStats.N_YELLOW] = charStat.yellow;
            
            _charStat[(int)eStats.WHITE] = charStat.white;
            _charStat[(int)eStats.N_WHITE] = charStat.white;
            
            _charStat[(int)eStats.BLACK] = charStat.black;
            _charStat[(int)eStats.N_BLACK] = charStat.black;
            
            _charStat[(int)eStats.HP] = charStat.HP;
            _charStat[(int)eStats.NHP] = charStat.HP;
            _charStat[(int)eStats.NMHP] = charStat.HP;

            _charStat[(int)eStats.ARMOR] = charStat.armor;
            _charStat[(int)eStats.NARMOR] = charStat.armor;
            
            _charStat[(int)eStats.MAGIC_RESIST] = charStat.magicResist;
            _charStat[(int)eStats.NMAGIC_RESIST] = charStat.magicResist;

            _charStat[(int)eStats.MELEE_ATTACK] = charStat.meleeAttack;
            _charStat[(int)eStats.NMELEE_ATTACK] = charStat.meleeAttack;
            
            _charStat[(int)eStats.MAGICAL_ATTACK] = charStat.magicalAttack;
            _charStat[(int)eStats.NMAGICAL_ATTACK] = charStat.magicalAttack;
            
            _charStat[(int)eStats.CRIT_CHANCE] = charStat.critChance;
            _charStat[(int)eStats.NCRIT_CHANCE] = charStat.critChance;
            
            _charStat[(int)eStats.SPEED] = charStat.speed;
            _charStat[(int)eStats.NSPEED] = charStat.speed;

            _charStat[(int)eStats.ACTION_POINT] = charStat.actionPoint;
            _charStat[(int)eStats.NACTION_POINT] = charStat.actionPoint;

            _charStat[(int)eStats.MOVE_POINT] = charStat.movePoint;
            
            _charStat[(int)eStats.DODGE] = charStat.actionPoint;
            _charStat[(int)eStats.NDODGE] = charStat.actionPoint;
            
            _charStat[(int)eStats.RESISTANCE] = charStat.resistance;
            _charStat[(int)eStats.NRESISTANCE] = charStat.resistance;

            _charStat[(int)eStats.RANGE_INCREASE] = 0;
            _charStat[(int)eStats.DAMAGE_INCREASE] = 0;
            _charStat[(int)eStats.ACCURACY_INCREASE] = 0;
            #endregion
        }
        #region Stat Helper
        private eStats GetProperStatAttribute(eStats stat)
        {
            switch (stat)
            {
                case eStats.BLUE:           return eStats.N_BLUE;
                case eStats.RED:            return eStats.N_RED;
                case eStats.YELLOW:         return eStats.N_YELLOW;
                case eStats.WHITE:          return eStats.N_WHITE;
                case eStats.BLACK:          return eStats.N_BLACK;
                case eStats.HP:             return eStats.NHP;
                case eStats.ARMOR:          return eStats.NARMOR;
                case eStats.MAGIC_RESIST:   return eStats.NMAGIC_RESIST;
                case eStats.MELEE_ATTACK:   return eStats.NMELEE_ATTACK;
                case eStats.MAGICAL_ATTACK: return eStats.NMAGICAL_ATTACK;
                case eStats.CRIT_CHANCE:    return eStats.NCRIT_CHANCE;
                case eStats.SPEED:          return eStats.NSPEED;
                case eStats.ACTION_POINT:   return eStats.NACTION_POINT;
                case eStats.DODGE:          return eStats.NDODGE;
                case eStats.RESISTANCE:     return eStats.NRESISTANCE;
            }
            return stat;
        }
        #endregion
        
        public float GetStat(eStats stat)
        {
            return _charStat[(int)GetProperStatAttribute(stat)];
        }

        public event Action<eStats, long> OnStatChanged;
        public void ClearChangeEvent() => OnStatChanged = null;
        public void ChangeStat(eStats stat, long valueDelta)
        {
            eStats realStat = GetProperStatAttribute(stat);
            _charStat[(int)realStat] += valueDelta;
            OnStatChanged?.Invoke(realStat, _charStat[(int)realStat]);
        }
    
        #region Damage Part
        
        public void ReceiveDamage(float damage)
        {
            ChangeStat(eStats.HP, -(long)damage);
            if (_charStat[(int)eStats.NHP] <= 0)
            {
                _charStat[(int)eStats.NHP] = 0;
            }
        }
        
        #endregion
        
        #region Action Point Part

        public bool UseActionPoint(float value=1f)
        {
            if (GetStat(eStats.NACTION_POINT) >= value)
            {
                _charStat[(int)eStats.NACTION_POINT] -= (long)value;
            }
            else
            {
                Debug.Log($"Action Point 부족함. {GetStat(eStats.NACTION_POINT)}");
                return false;
            }
            return true;
        }
        
        #endregion
    }
}
