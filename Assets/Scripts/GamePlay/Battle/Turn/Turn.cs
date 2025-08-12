using AngelBeat.Core;
using AngelBeat.Core.SingletonObjects.Managers;
using Character.Unit;
using Core.Foundation;
using Core.Foundation.Define;
using System;
using UnityEngine;

namespace AngelBeat
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
            TurnOwner.OnUpdate += FocusCamera;
                
            // Control UI. TODO : 이거 이벤트버스를 써야할까? 그냥 model이 BattleCharManager인거 아닐까..?
            EventBus.Instance.SendMessage(new OnTurnChanged { TurnOwner = TurnOwner });
                
            #region Control Logic
            if (TurnOwner.GetCharType() == SystemEnum.eCharType.Player)
            {
                CharPlayer player = TurnOwner as CharPlayer;
                if (player)
                    player.OnUpdate += player.OnPlayerMoveUpdate;
            }
            else if (TurnOwner.GetCharType() == SystemEnum.eCharType.Enemy)
            {
                Debug.Log("Monster turn : AI not implemented");
                //CharMonster monster = TurnOwner as CharMonster;
                //if(monster)
                //    monster.StartAI();
            }
            
            TurnOwner.KeywordInfo.ExecuteByPhase(SystemEnum.eExecutionPhase.SoT);
            #endregion
        }

        private void DefaultTurnEnd()
        {
            // Release Camera Control.
            TurnOwner.OnUpdate -= FocusCamera;
            // Release Character Control.
            if (TurnOwner.GetCharType() == SystemEnum.eCharType.Player)
            {
                CharPlayer player = TurnOwner as CharPlayer;
                if (player)
                    player.OnUpdate -= player.OnPlayerMoveUpdate;
            }
            else if (TurnOwner.GetCharType() == SystemEnum.eCharType.Enemy)
            {
                Debug.Log("Monster turn ended");
                //CharMonster monster = TurnOwner as CharMonster;
                //if (monster)
                //    monster.StopAI();
            }
            
            TurnOwner.KeywordInfo.ExecuteByPhase(SystemEnum.eExecutionPhase.EoT);
        }

        private void FocusCamera()
        {
            float z = TurnOwner.MainCamera.transform.position.z;
            Vector3 charPos = TurnOwner.CharTransform.position;
            TurnOwner.MainCamera.transform.position = new Vector3(charPos.x, charPos.y, z);
        }
    }
}
