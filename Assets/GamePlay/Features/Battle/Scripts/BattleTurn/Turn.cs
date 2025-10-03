using Core.Scripts.Foundation.Define;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.BattleTurn
{
    public class Turn
    {
        public enum Side { None, Player, Enemy, Neutral, SideMax }

        public CharBase TurnOwner { get; private set; }
        public Side WhoseSide { get; private set; }
        public bool IsValid => TurnOwner;
        
        private readonly Action _onBeginTurn =     delegate { };
        private readonly Action _onEndTurn =       delegate { };

        public Turn(CharBase turnOwner)
        {
            TurnOwner = turnOwner;
            WhoseSide = turnOwner.GetCharType() == SystemEnum.eCharType.Enemy ?
                Side.Enemy : Side.Player;
            
            _onBeginTurn += DefaultTurnBegin;
            _onEndTurn += DefaultTurnEnd;
        }
        
        public void Begin() => _onBeginTurn();
        public void End() => _onEndTurn();

        private void DefaultTurnBegin()
        {
            // Control Camera.
            FocusCamera();
                
            #region Control Logic
            if (TurnOwner.GetCharType() == SystemEnum.eCharType.Enemy)
            {
                Debug.Log("Monster turn : AI not implemented");
            }
            
            //TurnOwner.KeywordInfo.ExecuteByPhase(SystemEnum.eExecutionPhase.SoT, TriggerType.EoT);
            #endregion
        }

        private void DefaultTurnEnd()
        {
            FocusCamera();
            if (TurnOwner.GetCharType() == SystemEnum.eCharType.Enemy)
            {
                Debug.Log("Monster turn ended");
                //CharMonster monster = TurnOwner as CharMonster;
                //if (monster)
                //    monster.StopAI();
            }
            
            //TurnOwner.KeywordInfo.ExecuteByPhase(SystemEnum.eExecutionPhase.EoT, TriggerType.EoT);
        }

        private void FocusCamera()
        {
            float z = TurnOwner.MainCamera.transform.position.z;
            Vector3 charPos = TurnOwner.CharTransform.position;
            TurnOwner.MainCamera.transform.position = new Vector3(charPos.x, charPos.y, z);
        }
    }
}
