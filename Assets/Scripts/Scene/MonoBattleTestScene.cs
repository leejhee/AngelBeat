using AngelBeat;
using AngelBeat.Core.Battle;
using AngelBeat.Core.Character;
using System.Collections.Generic;
using UnityEngine;

namespace Scene
{
    /// <summary>
    /// BattleScene에서만 테스트하는 용도의 씬 초기화용 클래스
    /// </summary>
    public class MonoBattleTestScene : MonoBehaviour
    {
        [SerializeField] 
        private List<GameObject> battleUI;
        
        private DebugMockSource _src;
        void Awake()
        {
            GameManager instance = GameManager.Instance;
        }
        
        void Start()
        {
            var testXiaoModel = new CharacterModel(88888888);
            Party playerParty = new (new List<CharacterModel> { testXiaoModel });
            _src = new DebugMockSource(SystemEnum.eDungeon.MOUNTAIN_BACK, playerParty, "TestMap");
            
            BattleController.Instance.SetStageSource(_src);
            
            foreach(var go in battleUI)
                UIManager.Instance.ShowUI(go);
        }
        
        
    }
}