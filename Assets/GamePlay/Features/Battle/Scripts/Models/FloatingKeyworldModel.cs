using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.Models
{
    public class FloatingKeywordModel
    {
        public Sprite Icon;
        public int Count;
        public int Amount;

        public FloatingKeywordModel(Sprite icon, int count, int amount)
        {
            Icon = icon;
            Count = count;
            Amount = amount;
        }

        public FloatingKeywordModel(int count, int amount)
        {
            Count = count;
            Amount = amount;
        }
    }
}
