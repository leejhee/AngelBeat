using AngelBeat;
using Character;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Managers;
using GamePlay.Battle;
using GamePlay.Character;
using GamePlay.Entities.Scripts.Character;
using GamePlay.Features.Battle.Scripts;
using GamePlay.Features.Scripts.Battle;
using System.Collections.Generic;
using UIs.Runtime;
using UnityEngine;

namespace Scene
{
    /// <summary>
    /// BattleScene에서만 테스트하는 용도의 씬 초기화용 클래스
    /// </summary>
    public class MonoBattleTestScene : MonoBehaviour
    {

        public void TestButton()
        {
            //UIManager.Instance.ShowViewAsync(ViewID.BattleSceneView);
        }
        
    }
}