using System.Collections.Generic;
using System.Resources;
using Core.Scripts.Managers;
using UnityEngine;
using ResourceManager = Core.Scripts.Managers.ResourceManager;

namespace GamePlay.Features.Battle.Scripts.UI.UIObjects
{
    public class TurnHUD : MonoBehaviour
    {
        [SerializeField] private List<TurnPortrait> turns;
        [SerializeField] private int currentIndex;
        
        public void MoveToNextTurn()
        {
            if (currentIndex >= 0)
            {
                turns[currentIndex++].SetCurrentTurn(false);
            }
            turns[currentIndex].SetCurrentTurn(true);
        }

        public void OnRoundStart()
        {
            currentIndex = -1;
        }

        public void AddToTurnList(TurnPortrait turn)
        {
            turns.Add(turn);
        }

        public void ClearList()
        {
            foreach (TurnPortrait turn in turns)
            {
                ResourceManager.Instance.ReleaseInstance(turn.gameObject);
            }
            turns.Clear();
        }

        public void FindDeadCharacter(int idx)
        {
            turns[idx].OnCharacterDie();
        }
    }
}
