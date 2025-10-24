using Core.Scripts.Data;
using GamePlay.Features.Battle.Scripts.Models;
using GamePlay.Features.Battle.Scripts.UI;
using System;
using Unity.Mathematics;
using UnityEngine;
using static Core.Scripts.Foundation.Define.SystemEnum;
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
            _charStat[(int)eStats.HP] = charStat.HealthPoint;
            _charStat[(int)eStats.NHP] = charStat.HealthPoint;
            _charStat[(int)eStats.NMHP] = charStat.HealthPoint;

            _charStat[(int)eStats.DEFENSE] = charStat.Defense;
            _charStat[(int)eStats.NDEFENSE] = charStat.Defense;
            
            _charStat[(int)eStats.MAGIC_RESIST] = charStat.MagicResist;
            _charStat[(int)eStats.NMAGIC_RESIST] = charStat.MagicResist;

            _charStat[(int)eStats.PHYSICAL_ATTACK] = charStat.PhysicalAttack;
            _charStat[(int)eStats.NPHYSICAL_ATTACK] = charStat.PhysicalAttack;
            
            _charStat[(int)eStats.MAGIC_ATTACK] = charStat.MagicAttack;
            _charStat[(int)eStats.NMAGIC_ATTACK] = charStat.MagicAttack;
            
            _charStat[(int)eStats.CRIT_RATE] = charStat.CriticalRate;
            _charStat[(int)eStats.NCRIT_RATE] = charStat.CriticalRate;
            
            _charStat[(int)eStats.SPEED] = charStat.Speed;
            _charStat[(int)eStats.NSPEED] = charStat.Speed;

            _charStat[(int)eStats.ACTION_POINT] = charStat.Movement;
            _charStat[(int)eStats.NACTION_POINT] = charStat.Movement;

            //_charStat[(int)eStats.MOVE_POINT] = charStat.movePoint;
            
            _charStat[(int)eStats.EVATION] = charStat.Evasion;
            _charStat[(int)eStats.NEVATION] = charStat.Evasion;

            _charStat[(int)eStats.RANGE_INCREASE] = 0;
            _charStat[(int)eStats.DAMAGE_INCREASE] = 0;
            _charStat[(int)eStats.ACCURACY_INCREASE] = 0;
            #endregion
        }
        public CharStat(DokkaebiData dokkaebiData)
        {
            int r = dokkaebiData.ObangRed;
            int b = dokkaebiData.ObangBlue;
            int y = dokkaebiData.ObangYellow;
            int bl = dokkaebiData.ObangBlack;
            int w =  dokkaebiData.ObangWhite;
            
            _charStat[((int)eStats.RED)] = r;
            _charStat[((int)eStats.N_RED)] = r;

            _charStat[((int)eStats.BLUE)] = b;
            _charStat[((int)eStats.N_BLUE)] = b;

            _charStat[((int)eStats.YELLOW)] = y;
            _charStat[((int)eStats.N_YELLOW)] = y;
            
            _charStat[((int)eStats.BLACK)] = bl;
            _charStat[((int)eStats.N_BLACK)] = bl;

            _charStat[((int)eStats.WHITE)] = w;
            _charStat[((int)eStats.N_WHITE)] = w;

            #region 파생 스탯 초기화
            _charStat[(int)eStats.PHYSICAL_ATTACK] = _charStat[(int)eStats.NPHYSICAL_ATTACK] = r * 4 + 2;
            _charStat[(int)eStats.CRIT_RATE] =  _charStat[(int)eStats.NCRIT_RATE] = r * 2;
            
            _charStat[(int)eStats.DEFENSE] =  _charStat[(int)eStats.NDEFENSE] = b * 3 + 5;
            _charStat[(int)eStats.MAGIC_RESIST] = _charStat[(int)eStats.NMAGIC_RESIST] = b * 3 + 5;
            _charStat[(int)eStats.AILMENT_RESISTANCE] = _charStat[(int)eStats.NAILMENT_RESISTANCE] = b * 3;

            _charStat[(int)eStats.MAGIC_ATTACK] = _charStat[(int)eStats.NMAGIC_ATTACK] = bl * 4 + 2;
            _charStat[(int)eStats.ACTION_POINT] =  _charStat[(int)eStats.NACTION_POINT] = _charStat[(int)eStats.NMACTION_POINT]= bl * 2 + 4;
            
            _charStat[(int)eStats.AILMENT_INFLICT] = _charStat[(int)eStats.NAILMENT_INFLICT] = w * 3;
            _charStat[(int)eStats.ACCURACY] = _charStat[(int)eStats.NACCURACY] = w * 3;
            
            _charStat[(int)eStats.HP] = y * 5 + 12;
            _charStat[((int)eStats.NHP)] = y * 5 + 12;
            _charStat[((int)eStats.NMHP)] = y * 5 + 12;
            _charStat[(int)eStats.EVATION] = _charStat[(int)eStats.NEVATION] = y * 2 + 5;
            _charStat[(int)eStats.SPEED] = _charStat[(int)eStats.NSPEED] = y * 2 + 3;
            
            #endregion
            
            #region 스탯 의존 구독
            OnStatChanged += (stat, delta, result) =>
            {
                switch (stat)
                {
                    case eStats.N_RED:
                        _charStat[(int)eStats.NPHYSICAL_ATTACK] += delta * 4;
                        _charStat[(int)eStats.NCRIT_RATE] += delta * 2;
                        break;
                    case eStats.N_BLUE:
                        _charStat[(int)eStats.NDEFENSE] += delta * 3;
                        _charStat[(int)eStats.NMAGIC_RESIST] += delta * 3;
                        _charStat[(int)eStats.NAILMENT_RESISTANCE] += delta * 3;
                        break;
                    case eStats.N_YELLOW:
                        _charStat[(int)eStats.NMHP] += delta * 5;
                        _charStat[(int)eStats.NSPEED] += delta * 2;
                        _charStat[(int)eStats.NEVATION] += delta * 2;
                        break;
                    case eStats.N_BLACK:
                        _charStat[(int)eStats.NMAGIC_ATTACK] += delta * 4;
                        _charStat[(int)eStats.NACTION_POINT] += delta * 2;
                        break;
                    case eStats.N_WHITE:
                        _charStat[(int)eStats.NAILMENT_INFLICT] += delta * 3;
                        _charStat[(int)eStats.NACCURACY] += delta * 3;
                        break;
                    default: break;
                }
            };
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
                case eStats.DEFENSE:          return eStats.NDEFENSE;
                case eStats.MAGIC_RESIST:   return eStats.NMAGIC_RESIST;
                case eStats.PHYSICAL_ATTACK:   return eStats.NPHYSICAL_ATTACK;
                case eStats.MAGIC_ATTACK: return eStats.NMAGIC_ATTACK;
                case eStats.CRIT_RATE:    return eStats.NCRIT_RATE;
                case eStats.SPEED:          return eStats.NSPEED;
                case eStats.ACTION_POINT:   return eStats.NACTION_POINT;
                case eStats.EVATION:          return eStats.NEVATION;
            }
            return stat;
        }

        private long GetMaxOf(eStats stat)
        {
            return stat switch
            {
                eStats.NHP => _charStat[(int)eStats.NMHP],
                eStats.NACTION_POINT => _charStat[(int)eStats.NMACTION_POINT],
                _ => long.MaxValue
            };
        }

        private static long ClampLong(long v, long min, long max) => v < min ? min : (v > max ? max : v);
        
        private long ClampStat(eStats stat, long target)
        {
            switch (stat)
            {
                case eStats.NHP:
                case eStats.NACTION_POINT:
                    return ClampLong(target, 0, GetMaxOf(stat));
                
                case eStats.NCRIT_RATE:
                case eStats.NEVATION:
                case eStats.NACCURACY:
                case eStats.NAILMENT_INFLICT:
                case eStats.NAILMENT_RESISTANCE:
                    return ClampLong(target, 0, 100);
                
                case eStats.N_RED:
                case eStats.N_BLUE:
                case eStats.N_YELLOW:
                case eStats.N_WHITE:
                case eStats.N_BLACK:
                case eStats.NPHYSICAL_ATTACK:
                case eStats.NMAGIC_ATTACK:
                case eStats.NDEFENSE:
                case eStats.NMAGIC_RESIST:
                case eStats.NSPEED:
                    return Math.Max(0, target);
                
                default:
                    return target;
            }    
        }
        
        #endregion
        
        public long GetStat(eStats stat)
        {
            return _charStat[(int)GetProperStatAttribute(stat)];
        }

        /// <summary>
        /// 바뀐 스탯, 변화량, 결과
        /// </summary>
        public event Action<eStats, long, long> OnStatChanged;
        
        public void ClearChangeEvent() => OnStatChanged = null;
        public void ChangeStat(eStats stat, long valueDelta)
        {
            eStats realStat = GetProperStatAttribute(stat);
            long before = _charStat[(int)realStat];
            long afterUnclamped = before + valueDelta;
            long after = ClampStat(realStat, afterUnclamped);
            _charStat[(int)realStat] = after;
            long appliedDelta = after - before;
            
            #region Special Case
            switch (realStat)
            {
                case eStats.NMHP:
                    long hpBefore = _charStat[(int)eStats.NHP];
                    long hpAfter = ClampStat(eStats.NHP, hpBefore);
                    if (hpAfter != hpBefore)
                    {
                        _charStat[(int)eStats.NHP] = hpAfter;
                        OnStatChanged?.Invoke(eStats.NHP, hpAfter - hpBefore, hpAfter);
                    }
                    break;
                case eStats.NMACTION_POINT:
                    long apBefore = _charStat[(int)eStats.NACTION_POINT];
                    long apAfter = ClampStat(eStats.NACTION_POINT, apBefore);
                    if (apAfter != apBefore)
                    {
                        _charStat[(int)eStats.NACTION_POINT] = apAfter;
                        OnStatChanged?.Invoke(eStats.NACTION_POINT, apAfter - apBefore, apAfter);
                    }
                    break;
            }
            #endregion
            
            OnStatChanged?.Invoke(realStat, valueDelta, _charStat[(int)realStat]);
        }
        
        #region API - UI Model
        // 얘네는 현재 Focus된 놈 체력 깎일때
        public event Action<HPModel> OnFocusedCharHpChanged;
        public event Action<ApModel> OnFocusedCharApChanged;
        
        [Obsolete]
        public void ChangeHP(int delta)
        {
            Debug.Log("HP 바뀜 아무튼 바뀜");
            OnFocusedCharHpChanged?.Invoke(new HPModel(delta));
        }
        
        [Obsolete]
        public void ChangeAP(int delta)
        {
            Debug.Log("AP 바뀜 일단 바꼈음");
            OnFocusedCharApChanged?.Invoke(new ApModel(delta));
        }
        
        #endregion
        
        #region Damage Part
        
        public void ReceiveDamage(float damage)
        {
            ChangeStat(eStats.HP, -(long)damage);
        }

        public void ReceiveHPPercentDamage(float percent)
        {
            var hp = GetStat(eStats.NMHP);
            int delta = Mathf.RoundToInt(hp * percent / 100f); 
            ReceiveDamage(delta);
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
